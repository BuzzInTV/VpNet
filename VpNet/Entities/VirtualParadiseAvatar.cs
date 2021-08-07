using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using VpNet.Extensions;
using VpNet.Internal;
using VpNet.NativeApi;
using static VpNet.Internal.Native;

namespace VpNet.Entities
{
    /// <summary>
    ///     Represents an avatar, which is an online instance of a user. An avatar may be a bot, in which case its name will be
    ///     surrounded by the characters [ and ].
    /// </summary>
    public sealed class VirtualParadiseAvatar : ICloneable, IEquatable<VirtualParadiseAvatar>
    {
        private readonly VirtualParadiseClient _client;

        internal VirtualParadiseAvatar(VirtualParadiseClient client, int session)
        {
            _client = client;
            Session = session;
        }

        /// <summary>
        ///     Gets the details of the application this avatar is using.
        /// </summary>
        /// <value>The avatar's application details.</value>
        public Application Application { get; internal set; }

        /// <summary>
        ///     Gets a value indicating whether this avatar is a bot.
        /// </summary>
        /// <value><see langword="true" /> if this avatar is a bot; otherwise, <see langword="false" />.</value>
        public bool IsBot => Name is { Length: >1 } name && name[0] == '[' && name[^1] == ']';

        /// <summary>
        ///     Gets the location of this avatar.
        /// </summary>
        /// <value>The location of this avatar.</value>
        public Location Location { get; internal set; }

        /// <summary>
        ///     Gets the name of this avatar.
        /// </summary>
        /// <value>The name of this avatar.</value>
        public string Name { get; internal set; }

        /// <summary>
        ///     Gets the session ID of this avatar.
        /// </summary>
        /// <value>The session ID of this avatar.</value>
        public int Session { get; }

        /// <summary>
        ///     Gets the type of this avatar.
        /// </summary>
        /// <value>The type of this avatar.</value>
        public int Type { get; internal set; }

        /// <summary>
        ///     Gets the user associated with this avatar.
        /// </summary>
        /// <value>The user associated with this avatar.</value>
        public VirtualParadiseUser User { get; internal set; }

        /// <summary>
        ///     Returns a value indicating whether the two given avatars are equal.
        /// </summary>
        /// <param name="left">The first avatar to compare.</param>
        /// <param name="right">The second avatar to compare.</param>
        /// <returns><see langword="true" /> if the two avatars are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(VirtualParadiseAvatar left, VirtualParadiseAvatar right) => Equals(left, right);

        /// <summary>
        ///     Returns a value indicating whether the two given avatars are not equal.
        /// </summary>
        /// <param name="left">The first avatar to compare.</param>
        /// <param name="right">The second avatar to compare.</param>
        /// <returns><see langword="true" /> if the two avatars are not equal; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(VirtualParadiseAvatar left, VirtualParadiseAvatar right) => !Equals(left, right);

        /// <summary>
        ///     Clicks this avatar.
        /// </summary>
        /// <param name="clickPoint">The position at which the avatar should be clicked.</param>
        /// <exception cref="InvalidOperationException">
        ///     <para>The action cannot be performed on the client's current avatar.</para>
        ///     -or-
        ///     <para>An attempt was made to click an avatar outside of a world.</para>
        /// </exception>
        public ValueTask ClickAsync(Vector3d? clickPoint = null)
        {
            if (this == _client.CurrentAvatar)
                return ValueTask.FromException(ThrowHelper.CannotUseSelfException());

            clickPoint ??= Location.Position;
            (double x, double y, double z) = clickPoint.Value;

            lock (_client.Lock)
            {
                IntPtr handle = _client.NativeInstanceHandle;

                vp_double_set(handle, FloatAttribute.ClickHitX, x);
                vp_double_set(handle, FloatAttribute.ClickHitY, y);
                vp_double_set(handle, FloatAttribute.ClickHitZ, z);

                var reason = (ReasonCode)vp_avatar_click(handle, Session);
                if (reason == ReasonCode.NotInWorld)
                    return ValueTask.FromException(ThrowHelper.NotInWorldException());
            }

            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     Performs a shallow copy of the avatar.
        /// </summary>
        /// <returns>The shallow copy.</returns>
        public object Clone()
        {
            return new VirtualParadiseAvatar(_client, Session)
            {
                Application = Application,
                Name = Name,
                Location = Location,
                Type = Type
            };
        }

        /// <summary>
        ///     Returns a value indicating whether this avatar and another avatar are equal.
        /// </summary>
        /// <param name="other">The avatar to compare with this instance.</param>
        /// <returns><see langword="true" /> if the two avatars are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(VirtualParadiseAvatar other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_client, other._client) && Location.Equals(other.Location) && Session == other.Session;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return obj is VirtualParadiseAvatar other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_client, Location, Session);

        /// <summary>
        ///     Sends a console message to this avatar.
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
        /// <exception cref="InvalidOperationException">
        ///     <para>The action cannot be performed on the client's current avatar.</para>
        ///     -or-
        ///     <para>The action cannot be performed outside of a world.</para>
        /// </exception>
        public ValueTask SendConsoleMessageAsync(string message, FontStyle style = FontStyle.Regular, Color? color = null)
        {
            return SendConsoleMessageAsync(string.Empty, message, style, color);
        }

        /// <summary>
        ///     Sends a console message to this avatar.
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
        /// <exception cref="InvalidOperationException">
        ///     <para>The action cannot be performed on the client's current avatar.</para>
        ///     -or-
        ///     <para>The action cannot be performed outside of a world.</para>
        /// </exception>
        public ValueTask SendConsoleMessageAsync(string name, string message, FontStyle style = FontStyle.Regular, Color? color = null)
        {
            name ??= string.Empty;

            if (message is null)
                return ValueTask.FromException(ThrowHelper.ArgumentNullException(nameof(message)));

            if (this == _client.CurrentAvatar)
                return ValueTask.FromException(ThrowHelper.CannotUseSelfException());

            (byte r, byte g, byte b) = color ?? Color.Black;

            lock (_client.Lock)
            {
                IntPtr handle = _client.NativeInstanceHandle;
                var reason = (ReasonCode)vp_console_message(handle, Session, name, message, (int)style, r, g, b);
                switch (reason)
                {
                    case ReasonCode.NotInWorld:
                        return ValueTask.FromException(ThrowHelper.NotInWorldException());

                    case ReasonCode.StringTooLong:
                        return ValueTask.FromException(ThrowHelper.StringTooLongException(nameof(message)));
                }
            }

            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     Sends a URI to this avatar.
        /// </summary>
        /// <param name="uri">The URI to send.</param>
        /// <param name="target">The URL target. See <see cref="UriTarget" /> for more information.</param>
        /// <exception cref="InvalidOperationException">The action cannot be performed on the client's current avatar.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="uri" /> is <see langword="null" />.</exception>
        public ValueTask SendUriAsync(Uri uri, UriTarget target = UriTarget.Browser)
        {
            if (this == _client.CurrentAvatar)
                return ValueTask.FromException(ThrowHelper.CannotUseSelfException());

            if (uri is null)
                return ValueTask.FromException(ThrowHelper.ArgumentNullException(nameof(uri)));

            lock (_client.Lock)
            {
                vp_url_send(_client.NativeInstanceHandle, Session, uri.ToString(), target);
            }

            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     Teleports the avatar to a new position within the current world.
        /// </summary>
        /// <param name="position">The position to which this avatar should be teleported.</param>
        public ValueTask TeleportAsync(Vector3d position)
        {
            var location = new Location(Location.World, position, Location.Rotation);
            return TeleportAsync(location);
        }

        /// <summary>
        ///     Teleports the avatar to a new position and rotation within the current world.
        /// </summary>
        /// <param name="position">The position to which this avatar should be teleported.</param>
        /// <param name="rotation">The rotation to which this avatar should be teleported</param>
        public ValueTask TeleportAsync(Vector3d position, Quaternion rotation)
        {
            var location = new Location(Location.World, position, rotation);
            return TeleportAsync(location);
        }

        /// <summary>
        ///     Teleports this avatar to a new location, which may or may not be a new world.
        /// </summary>
        /// <param name="location">The location to which this avatar should be teleported.</param>
        public async ValueTask TeleportAsync(Location location)
        {
            var isSelf = this == _client.CurrentAvatar;
            var isNewWorld = location.World != Location.World;
            string newWorldName = location.World.Name;
            string worldName = location.World.Name;

            if (location.World == Location.World)
            {
                worldName = string.Empty;
            }

            if (isSelf && isNewWorld)
            {
                await _client.EnterAsync(newWorldName);
            }

            IntPtr handle = _client.NativeInstanceHandle;

            if (this == _client.CurrentAvatar)
            {
                if (!string.IsNullOrWhiteSpace(worldName))
                {
                    await _client.EnterAsync(worldName);
                }

                // state change self
                lock (_client.Lock)
                {
                    (double x, double y, double z) = location.Position;
                    (double pitch, double yaw, double _) = location.Rotation.ToEulerAngles();

                    vp_double_set(handle, FloatAttribute.MyX, x);
                    vp_double_set(handle, FloatAttribute.MyY, y);
                    vp_double_set(handle, FloatAttribute.MyZ, z);
                    vp_double_set(handle, FloatAttribute.MyPitch, pitch);
                    vp_double_set(handle, FloatAttribute.MyYaw, yaw);

                    var reason = (ReasonCode)vp_state_change(_client.NativeInstanceHandle);
                    if (reason == ReasonCode.NotInWorld)
                        ThrowHelper.ThrowNotInWorldException();
                }
            }
            else
            {
                lock (_client.Lock)
                {
                    (float x, float y, float z) = (Vector3)location.Position;
                    (float pitch, float yaw, float _) = location.Rotation.ToEulerAngles();

                    var reason = (ReasonCode)vp_teleport_avatar(handle, Session, worldName, x, y, z, yaw, pitch);
                    if (reason == ReasonCode.NotInWorld)
                        ThrowHelper.ThrowNotInWorldException();
                }
            }

            Location = location;
        }

        /// <inheritdoc />
        public override string ToString() => $"Avatar [Session={Session}, Name={Name}]";
    }
}
