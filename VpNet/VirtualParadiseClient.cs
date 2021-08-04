using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Channels;
using System.Threading.Tasks;
using VpNet.Entities;
using VpNet.EventData;
using VpNet.Exceptions;
using VpNet.Internal;
using VpNet.NativeApi;
using static VpNet.Internal.Native;

namespace VpNet
{
    /// <summary>
    ///     Provides a managed API which offers full encapsulation of the native Virtual Paradise SDK.
    /// </summary>
    public sealed partial class VirtualParadiseClient : IDisposable
    {
        private const string DefaultUniverseHost = "universe.virtualparadise.org";
        private const int DefaultUniversePort = 57000;

        private readonly VirtualParadiseConfiguration _configuration;

        private readonly ConcurrentDictionary<int, VirtualParadiseAvatar> _avatars = new();
        private readonly ConcurrentDictionary<int, VirtualParadiseObject> _objects = new();
        private readonly ConcurrentDictionary<int, VirtualParadiseUser> _users = new();

        private readonly ConcurrentDictionary<NativeCallback, NativeCallbackHandler> _nativeCallbackHandlers = new();
        private readonly ConcurrentDictionary<NativeEvent, NativeEventHandler> _nativeEventHandlers = new();

        private readonly Dictionary<int, TaskCompletionSource<ReasonCode>> _joinCompletionSources = new();
        private readonly Dictionary<int, TaskCompletionSource<ReasonCode>> _inviteCompletionSources = new();
        private readonly ConcurrentDictionary<int, TaskCompletionSource<VirtualParadiseUser>> _usersCompletionSources = new();

        private TaskCompletionSource<ReasonCode> _connectCompletionSource;
        private TaskCompletionSource<ReasonCode> _enterCompletionSource;
        private TaskCompletionSource<ReasonCode> _loginCompletionSource;
        private TaskCompletionSource _worldSettingsCompletionSource = new();

        private Channel<VirtualParadiseWorld> _worldListChannel = Channel.CreateUnbounded<VirtualParadiseWorld>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualParadiseClient" /> class with the default configuration.
        /// </summary>
        public VirtualParadiseClient() : this(new VirtualParadiseConfiguration())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualParadiseClient" /> class with a specified configuration.
        /// </summary>
        /// <value>The configuration for this client.</value>
        public VirtualParadiseClient(VirtualParadiseConfiguration configuration)
        {
            _configuration = new VirtualParadiseConfiguration(configuration);
            Initialize();
        }

        /// <inheritdoc />
        ~VirtualParadiseClient()
        {
            ReleaseUnmanagedResources();
        }

        /// <summary>
        ///     Occurs when an avatar has entered the vicinity of the client.
        /// </summary>
        public event AsyncEventHandler<AvatarJoinedEventArgs> AvatarJoined;

        /// <summary>
        ///     Occurs when an avatar has left the vicinity of the client.
        /// </summary>
        public event AsyncEventHandler<AvatarLeftEventArgs> AvatarLeft;

        /// <summary>
        ///     Occurs when an avatar has changed their position or rotation.
        /// </summary>
        public event AsyncEventHandler<AvatarMovedEventArgs> AvatarMoved;

        /// <summary>
        ///     Occurs when an avatar has changed their type.
        /// </summary>
        public event AsyncEventHandler<AvatarTypeChangedEventArgs> AvatarTypeChanged;

        /// <summary>
        ///     Occurs when an invite request has been received.
        /// </summary>
        public event AsyncEventHandler<InviteRequestReceivedEventArgs> InviteRequestReceived;

        /// <summary>
        ///     Occurs when a join request has been received.
        /// </summary>
        public event AsyncEventHandler<JoinRequestReceivedEventArgs> JoinRequestReceived;

        /// <summary>
        ///     Occurs when a chat message or console message has been received.
        /// </summary>
        public event AsyncEventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Gets a read-only view of the avatars in the vicinity of this client.
        /// </summary>
        /// <value>A read-only view of the avatars in the vicinity of this client.</value>
        public IReadOnlyList<VirtualParadiseAvatar> Avatars => _avatars.Values.OrderBy(a => a.Session).ToArray();

        /// <summary>
        ///     Gets the current avatar for this client.
        /// </summary>
        /// <value>
        ///     The current avatar for this client, or <see langword="null" /> if the client is not currently in a world.
        /// </value>
        public VirtualParadiseAvatar CurrentAvatar { get; private set; }

        /// <summary>
        ///     Gets the current user account associated with this client.
        /// </summary>
        /// <value>
        ///     The user account associated with this client, or <see langword="null" /> if this client is not currently logged
        ///     in.
        /// </value>
        public VirtualParadiseUser CurrentUser { get; private set; }

        /// <summary>
        ///     Gets the world to which this client is currently connected.
        /// </summary>
        /// <value>
        ///     The world to which this client is currently connected, or <see langword="null" /> if this client is not currently
        ///     in a world.
        /// </value>
        public VirtualParadiseWorld CurrentWorld { get; internal set; }

        /// <summary>
        ///     Sends a console message to all avatars in the world.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="style">
        ///     Optional. Specifies font styling of the console message. Defaults to <see cref="FontStyle.Regular" />.
        ///     The <see cref="FontStyle.Strikeout" /> and <see cref="FontStyle.Underline" /> flags are ignored.
        /// </param>
        /// <param name="color">
        ///     Optional. The color of the message. Defaults to <see cref="Color.Black" />. The <see cref="Color.A" /> property is
        ///     ignored.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="message" /> is too long to send.</exception>
        /// <exception cref="InvalidOperationException">The client is not connected to a world.</exception>
        public Task BroadcastConsoleMessageAsync(string message, FontStyle style = FontStyle.Regular, Color? color = null)
        {
            return BroadcastConsoleMessageAsync(string.Empty, message, style, color);
        }

        /// <summary>
        ///     Sends a console message to all avatars in the world.
        /// </summary>
        /// <param name="name">The apparent sender of the message.</param>
        /// <param name="message">The message content.</param>
        /// <param name="style">
        ///     Optional. Specifies font styling of the console message. Defaults to <see cref="FontStyle.Regular" />.
        ///     The <see cref="FontStyle.Strikeout" /> and <see cref="FontStyle.Underline" /> flags are ignored.
        /// </param>
        /// <param name="color">
        ///     Optional. The color of the message. Defaults to <see cref="Color.Black" />. The <see cref="Color.A" /> property is
        ///     ignored.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="message" /> is too long to send.</exception>
        /// <exception cref="InvalidOperationException">The client is not connected to a world.</exception>
        public Task BroadcastConsoleMessageAsync(
            string name,
            string message,
            FontStyle style = FontStyle.Regular,
            Color? color = null)
        {
            name ??= string.Empty;

            if (message is null!)
                return ThrowHelper.ArgumentNullExceptionAsync(nameof(message));

            byte r = color?.R ?? 0;
            byte g = color?.G ?? 0;
            byte b = color?.B ?? 0;

            lock (Lock)
            {
                var reason = (ReasonCode)vp_console_message(NativeInstanceHandle, 0, name, message, (int)style, r, g, b);
                switch (reason)
                {
                    case ReasonCode.NotInWorld:
                        return ThrowHelper.NotInWorldExceptionAsync();

                    case ReasonCode.StringTooLong:
                        return ThrowHelper.StringTooLongExceptionAsync(nameof(message));
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Establishes a connection to the universe at the specified remote endpoint.
        /// </summary>
        /// <param name="host">The remote host.</param>
        /// <param name="port">The remote port.</param>
        /// <remarks>
        ///     If <paramref name="host" /> is <see langword="null" /> and/or <paramref name="port" /> is less than 1, the client
        ///     will use the default host and port values respectively.
        /// </remarks>
        public async Task ConnectAsync(string host = null, int port = -1)
        {
            if (string.IsNullOrWhiteSpace(host)) host = DefaultUniverseHost;
            if (port < 1) port = DefaultUniversePort;

            ReasonCode reason;

            lock (Lock)
            {
                _connectCompletionSource = new TaskCompletionSource<ReasonCode>();

                reason = (ReasonCode)vp_connect_universe(NativeInstanceHandle, host, port);
                if (reason != ReasonCode.Success)
                    goto NoSuccess;
            }

            reason = await _connectCompletionSource.Task;

            NoSuccess:
            switch (reason)
            {
                case ReasonCode.Success:
                    break;

                case ReasonCode.ConnectionError:
                    throw new SocketException();

                default:
                    throw new SocketException((int)reason);
            }
        }

        /// <summary>
        ///     Establishes a connection to the universe at the specified remote endpoint.
        /// </summary>
        /// <param name="remoteEP">The remote endpoint of the universe.</param>
        /// <exception cref="ArgumentNullException"><paramref name="remoteEP" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="remoteEP" /> is not a supported endpoint.</exception>
        public Task ConnectAsync(EndPoint remoteEP)
        {
            if (remoteEP is null) throw new ArgumentNullException(nameof(remoteEP));

            string host;
            int port;

            switch (remoteEP)
            {
                case IPEndPoint ip:
                    host = ip.Address.ToString();
                    port = ip.Port;
                    break;

                case DnsEndPoint dns:
                    host = dns.Host;
                    port = dns.Port;
                    break;

                default:
                    throw new ArgumentException("The specified remote endpoint is not supported.", nameof(remoteEP));
            }

            return ConnectAsync(host, port);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Enters a specified world at a specified position.
        /// </summary>
        /// <param name="worldName">The name of the world to enter.</param>
        /// <param name="position">The position at which to enter the world.</param>
        /// <exception cref="ArgumentNullException"><paramref name="worldName" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        ///     A world enter was attempted before the client was connected to a universe.
        /// </exception>
        /// <exception cref="Exception">Connection to the universe server was lost, or connecting to the world failed.</exception>
        /// <exception cref="WorldNotFoundException">The specified world was not found.</exception>
        /// <exception cref="TimeoutException">Connection to the world server timed out.</exception>
        public async Task<VirtualParadiseWorld> EnterAsync(string worldName, Vector3d position)
        {
            await EnterAsync(worldName);
            await CurrentAvatar.TeleportAsync(position, Vector3d.Zero);
            return CurrentWorld;
        }

        /// <summary>
        ///     Enters a specified world at a specified position and rotation.
        /// </summary>
        /// <param name="worldName">The name of the world to enter.</param>
        /// <param name="position">The position at which to enter the world.</param>
        /// <param name="rotation">The rotation at which to enter the world.</param>
        /// <exception cref="ArgumentNullException"><paramref name="worldName" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        ///     A world enter was attempted before the client was connected to a universe.
        /// </exception>
        /// <exception cref="Exception">Connection to the universe server was lost, or connecting to the world failed.</exception>
        /// <exception cref="WorldNotFoundException">The specified world was not found.</exception>
        /// <exception cref="TimeoutException">Connection to the world server timed out.</exception>
        public async Task<VirtualParadiseWorld> EnterAsync(string worldName, Vector3d position, Vector3d rotation)
        {
            await EnterAsync(worldName);
            await CurrentAvatar.TeleportAsync(position, rotation);
            return CurrentWorld;
        }

        /// <summary>
        ///     Enters a specified world at a specified position.
        /// </summary>
        /// <param name="world">The world to enter.</param>
        /// <param name="position">The position at which to enter the world.</param>
        /// <exception cref="ArgumentNullException"><paramref name="world" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        ///     A world enter was attempted before the client was connected to a universe.
        /// </exception>
        /// <exception cref="Exception">Connection to the universe server was lost, or connecting to the world failed.</exception>
        /// <exception cref="WorldNotFoundException">The specified world was not found.</exception>
        /// <exception cref="TimeoutException">Connection to the world server timed out.</exception>
        public async Task EnterAsync(VirtualParadiseWorld world, Vector3d position)
        {
            await EnterAsync(world);
            await CurrentAvatar.TeleportAsync(position, Vector3d.Zero);
        }

        /// <summary>
        ///     Enters a specified world at a specified position and rotation.
        /// </summary>
        /// <param name="world">The world to enter.</param>
        /// <param name="position">The position at which to enter the world.</param>
        /// <param name="rotation">The rotation at which to enter the world.</param>
        /// <exception cref="ArgumentNullException"><paramref name="world" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        ///     A world enter was attempted before the client was connected to a universe.
        /// </exception>
        /// <exception cref="Exception">Connection to the universe server was lost, or connecting to the world failed.</exception>
        /// <exception cref="WorldNotFoundException">The specified world was not found.</exception>
        /// <exception cref="TimeoutException">Connection to the world server timed out.</exception>
        public async Task EnterAsync(VirtualParadiseWorld world, Vector3d position, Vector3d rotation)
        {
            await EnterAsync(world);
            await CurrentAvatar.TeleportAsync(position, rotation);
        }

        /// <summary>
        ///     Enters a specified world.
        /// </summary>
        /// <param name="world">The world to enter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="world" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        ///     A world enter was attempted before the client was connected to a universe.
        /// </exception>
        /// <exception cref="Exception">Connection to the universe server was lost, or connecting to the world failed.</exception>
        /// <exception cref="WorldNotFoundException">The specified world was not found.</exception>
        /// <exception cref="TimeoutException">Connection to the world server timed out.</exception>
        public async Task EnterAsync(VirtualParadiseWorld world)
        {
            if (world is null) throw new ArgumentNullException(nameof(world));
            await EnterAsync(world.Name);
        }

        /// <summary>
        ///     Enters a specified world.
        /// </summary>
        /// <param name="worldName">The world to enter.</param>
        /// <returns>A <see cref="VirtualParadiseWorld" /> representing the world.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="worldName" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        ///     A world enter was attempted before the client was connected to a universe.
        /// </exception>
        /// <exception cref="ArgumentException">The specified world name is too long.</exception>
        /// <exception cref="Exception">Connection to the universe server was lost, or connecting to the world failed.</exception>
        /// <exception cref="WorldNotFoundException">The specified world was not found.</exception>
        /// <exception cref="TimeoutException">Connection to the world server timed out.</exception>
        public async Task<VirtualParadiseWorld> EnterAsync(string worldName)
        {
            if (worldName is null) throw new ArgumentNullException(nameof(worldName));

            if (CurrentWorld is not null)
            {
                lock (Lock)
                {
                    vp_leave(NativeInstanceHandle);
                }
            }

            ReasonCode reason;

            _worldSettingsCompletionSource = new TaskCompletionSource();
            _enterCompletionSource = new TaskCompletionSource<ReasonCode>();

            lock (Lock)
            {
                reason = (ReasonCode)vp_enter(NativeInstanceHandle, worldName);
                if (reason != ReasonCode.Success)
                {
                    goto NoSuccess;
                }
            }

            reason = await _enterCompletionSource.Task;

            NoSuccess:
            switch (reason)
            {
                case ReasonCode.Success:
                    break;

                case ReasonCode.NotInUniverse:
                    throw new InvalidOperationException(
                        "The client must be connected to the universe server in order to enter a world.");

                case ReasonCode.StringTooLong:
                    ThrowHelper.ThrowStringTooLongException(nameof(worldName));
                    break;

                case ReasonCode.ConnectionError:
                case ReasonCode.WorldLoginError:
                    throw new Exception("Connection to the universe server was lost, or connecting to the world failed.");

                case ReasonCode.WorldNotFound:
                    throw new WorldNotFoundException(worldName);

                case ReasonCode.Timeout:
                    throw new TimeoutException("Connection to the world server timed out.");

                default:
                    throw new Exception($"Unknown error: {reason:D} ({reason:G})");
            }

            int size = vp_int(NativeInstanceHandle, IntegerAttribute.WorldSize);
            // await _worldSettingsCompletionSource.Task;

            CurrentWorld = await GetWorldAsync(worldName);
            CurrentWorld.Size = new Size(size, size);
            // CurrentWorld.Settings = WorldSettingsConverter.FromDictionary(_worldSettings);
            // _worldSettings.Clear();

            CurrentAvatar = new VirtualParadiseAvatar(this, -1)
            {
                Application = _configuration.Application,
                Name = _configuration.BotName,
                Location = new Location(CurrentWorld, Vector3d.Zero, Vector3d.Zero),
                User = CurrentUser
            };

            if (_configuration.AutoQuery)
            {
                // TODO auto-query here
            }

            return CurrentWorld;
        }

        /// <summary>
        ///     Gets the avatar with the specified session.
        /// </summary>
        /// <param name="session">The session of the avatar to get.</param>
        /// <returns>
        ///     The avatar whose session is equal to <paramref name="session" />, or <see langword="null" /> if no match was
        ///     found.
        /// </returns>
        public VirtualParadiseAvatar GetAvatar(int session)
        {
            _avatars.TryGetValue(session, out VirtualParadiseAvatar avatar);
            return avatar;
        }

        /// <summary>
        ///     Gets a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to get.</param>
        /// <returns>
        ///     The user whose ID is equal to <paramref name="userId" />, or <see langword="null" /> if no match was found.
        /// </returns>
        public async Task<VirtualParadiseUser> GetUserAsync(int userId)
        {
            if (_users.TryGetValue(userId, out var user))
                return user;

            if (_usersCompletionSources.TryGetValue(userId, out var taskCompletionSource))
                return await taskCompletionSource.Task;

            taskCompletionSource = new TaskCompletionSource<VirtualParadiseUser>();
            _usersCompletionSources.TryAdd(userId, taskCompletionSource);

            lock (Lock)
            {
                vp_user_attributes_by_id(NativeInstanceHandle, userId);
            }

            user = await taskCompletionSource.Task;
            user = AddOrUpdateUser(user);

            _usersCompletionSources.TryRemove(userId, out var _);
            return user;
        }

        /// <summary>
        ///     Gets a world by its name.
        /// </summary>
        /// <param name="name">The name of the world.</param>
        /// <returns>
        ///     A <see cref="VirtualParadiseWorld" /> whose name is equal to <paramref name="name" />, or <see langword="null" />
        ///     if no match was found.
        /// </returns>
        public async Task<VirtualParadiseWorld> GetWorldAsync(string name)
        {
            await foreach (var world in EnumerateWorldsAsync())
            {
                if (string.Equals(world.Name, name))
                    return world;
            }

            return null;
        }

        /// <summary>
        ///     Gets a read-only view of the worlds returned by the universe server. 
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}" /> containing <see cref="VirtualParadiseWorld" /> values.</returns>
        /// <remarks>
        ///     This method will consume the list in full before returning, and therefore can result in apparent "hang" while the
        ///     list is being fetched. For an <see cref="IAsyncEnumerable{T}" /> alternative, use
        ///     <see cref="EnumerateWorldsAsync" />.
        /// </remarks>
        /// <seealso cref="EnumerateWorldsAsync" />
        public async Task<IReadOnlyCollection<VirtualParadiseWorld>> GetWorldsAsync()
        {
            var worlds = new List<VirtualParadiseWorld>();

            await foreach (var world in EnumerateWorldsAsync())
            {
                worlds.Add(world);
            }

            return worlds.AsReadOnly();
        }

        /// <summary>
        ///     Gets an enumerable of the worlds returned by the universe server. 
        /// </summary>
        /// <returns>An <see cref="IAsyncEnumerable{T}" /> containing <see cref="VirtualParadiseWorld" /> values.</returns>
        /// <remarks>
        ///     This method will yield results back as they are received from the world server. To access a consumed collection,
        ///     use <see cref="GetWorldsAsync" />.
        /// </remarks>
        /// <seealso cref="GetWorldsAsync" />
        public IAsyncEnumerable<VirtualParadiseWorld> EnumerateWorldsAsync()
        {
            _worldListChannel ??= Channel.CreateUnbounded<VirtualParadiseWorld>();

            lock (Lock)
            {
                vp_world_list(NativeInstanceHandle, 0);
            }

            var worlds = _worldListChannel.Reader.ReadAllAsync();
            _worldListChannel = null;
            return worlds;
        }

        /// <summary>
        ///     Leaves the current world.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     An attempt was made to leave a world when the client was not present in one.
        /// </exception>
        public Task LeaveAsync()
        {
            lock (Lock)
            {
                var reason = (ReasonCode)vp_leave(NativeInstanceHandle);
                if (reason == ReasonCode.NotInWorld)
                    return ThrowHelper.NotInWorldExceptionAsync();
            }

            _avatars.Clear();
            _objects.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs in to the current universe.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         <see cref="VirtualParadiseConfiguration.Username" /> is <see langword="null" />, empty, or consists only of
        ///         whitespace.
        ///     </para>
        ///     -or-
        ///     <para>
        ///         <see cref="VirtualParadiseConfiguration.Password" /> is <see langword="null" />, empty, or consists only of
        ///         whitespace.
        ///     </para>
        ///     -or-
        ///     <para>
        ///         <see cref="VirtualParadiseConfiguration.BotName" /> is <see langword="null" />, empty, or consists only of
        ///         whitespace.
        ///     </para>
        ///     -or-
        ///     <para>
        ///         A value in the configuration is too long. (Most likely <see cref="VirtualParadiseConfiguration.BotName" />).
        ///     </para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     A login was attempted before a connection with the universe server was established.
        /// </exception>
        /// <exception cref="TimeoutException">The login request timed out.</exception>
        /// <exception cref="InvalidCredentialException">The specified username and password constitute an invalid login.</exception>
        public async Task LoginAsync()
        {
            string username = _configuration.Username;
            string password = _configuration.Password;
            string botName = _configuration.BotName;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(botName))
                throw new ArgumentException("Cannot login due to incomplete configuration.");

            _loginCompletionSource = new TaskCompletionSource<ReasonCode>();

            ReasonCode reason;
            lock (Lock)
            {
                if (_configuration.Application is var (name, version))
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        vp_string_set(NativeInstanceHandle, StringAttribute.ApplicationName, name);
                        vp_string_set(NativeInstanceHandle, StringAttribute.ApplicationVersion, version ?? string.Empty);
                    }
                }

                reason = (ReasonCode)vp_login(NativeInstanceHandle, username, password, botName);
                if (reason != ReasonCode.Success)
                    goto NoSuccess;
            }

            reason = await _loginCompletionSource.Task;
            NoSuccess:
            switch (reason)
            {
                case ReasonCode.Timeout:
                    throw new TimeoutException("The login request timed out.");

                case ReasonCode.InvalidLogin:
                    throw new InvalidCredentialException("The specified username and password constitute an invalid login.");

                case ReasonCode.StringTooLong:
                    throw new ArgumentException($"A value in the configuration is too long. ({nameof(_configuration.BotName)}?)");

                case ReasonCode.NotInUniverse:
                    throw new InvalidOperationException("A connection to the universe server is required to attempt login.");
            }

            int userId;

            lock (Lock)
            {
                userId = vp_int(NativeInstanceHandle, IntegerAttribute.MyUserId);
            }

            CurrentUser = await GetUserAsync(userId);
        }

        /// <summary>
        ///     Sends a chat message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">An attempt was made to send a message outside of a world.</exception>
        /// <exception cref="ArgumentException">The message is too long to send.</exception>
        public Task SendMessageAsync(string message)
        {
            if (message is null!)
                return Task.FromException<ArgumentNullException>(new ArgumentNullException(nameof(message)));

            lock (Lock)
            {
                var reason = (ReasonCode)vp_say(NativeInstanceHandle, message);

                switch (reason)
                {
                    case ReasonCode.NotInWorld:
                        return ThrowHelper.NotInWorldExceptionAsync();

                    case ReasonCode.StringTooLong:
                        return ThrowHelper.StringTooLongExceptionAsync(nameof(message));
                }
            }

            return Task.CompletedTask;
        }

        internal TaskCompletionSource<ReasonCode> AddJoinCompletionSource(int reference)
        {
            var taskCompletionSource = new TaskCompletionSource<ReasonCode>();
            _joinCompletionSources.TryAdd(reference, taskCompletionSource);
            return taskCompletionSource;
        }

        internal TaskCompletionSource<ReasonCode> AddInviteCompletionSource(int reference)
        {
            var taskCompletionSource = new TaskCompletionSource<ReasonCode>();
            _inviteCompletionSources.TryAdd(reference, taskCompletionSource);
            return taskCompletionSource;
        }

        private VirtualParadiseAvatar AddOrUpdateAvatar(VirtualParadiseAvatar avatar)
        {
            return _avatars.AddOrUpdate(avatar.Session, avatar, (_, existing) =>
            {
                existing ??= new VirtualParadiseAvatar(this, avatar.Session);
                existing.Name = avatar.Name;
                existing.Location = avatar.Location;
                existing.Application = avatar.Application;
                existing.Type = avatar.Type;
                return existing;
            });
        }

        private VirtualParadiseUser AddOrUpdateUser(VirtualParadiseUser user)
        {
            return _users.AddOrUpdate(user.Id, user, (_, existing) =>
            {
                existing ??= new VirtualParadiseUser(this, user.Id);
                existing.Name = user.Name;
                existing.EmailAddress = user.EmailAddress;
                existing.LastLogin = user.LastLogin;
                existing.OnlineTime = user.OnlineTime;
                existing.RegistrationTime = user.RegistrationTime;
                return existing;
            });
        }

        private VirtualParadiseAvatar ExtractAvatar(IntPtr sender)
        {
            lock (Lock)
            {
                double x = vp_double(sender, FloatAttribute.AvatarX);
                double y = vp_double(sender, FloatAttribute.AvatarY);
                double z = vp_double(sender, FloatAttribute.AvatarZ);
                double pitch = vp_double(sender, FloatAttribute.AvatarPitch);
                double yaw = vp_double(sender, FloatAttribute.AvatarYaw);

                var position = new Vector3d(x, y, z);
                var rotation = new Vector3d(pitch, yaw, 0);

                string applicationName = vp_string(sender, StringAttribute.AvatarApplicationName);
                string applicationVersion = vp_string(sender, StringAttribute.AvatarApplicationVersion);

                int session = vp_int(sender, IntegerAttribute.AvatarSession);
                return new VirtualParadiseAvatar(this, session)
                {
                    Name = vp_string(sender, StringAttribute.AvatarName),
                    Location = new Location(CurrentWorld, position, rotation),
                    Application = new Application(applicationName, applicationVersion)
                };
            }
        }

        private void RaiseEvent<T>(AsyncEventHandler<T> eventHandler, T args) where T : EventArgs
        {
            if (eventHandler is null) return;
            if (args is null) return;

            Task.Run(() => eventHandler.Invoke(this, args));
        }

        private void ReleaseUnmanagedResources()
        {
            vp_destroy(NativeInstanceHandle);
            _instanceHandle.Free();
        }
    }
}
