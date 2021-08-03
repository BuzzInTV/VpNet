using System;
using System.Threading.Tasks;
using VpNet.Exceptions;
using VpNet.Internal;
using VpNet.ManagedApi.System;
using VpNet.NativeApi;
using static VpNet.Internal.Native;

namespace VpNet.Entities
{
    /// <summary>
    ///     Represents a user.
    /// </summary>
    public sealed class VirtualParadiseUser : IEquatable<VirtualParadiseUser>
    {
        private readonly VirtualParadiseClient _client;

        internal VirtualParadiseUser(VirtualParadiseClient client, int id)
        {
            _client = client;
            Id = id;
        }

        /// <summary>
        ///     Gets the email address of this user.
        /// </summary>
        /// <value>The email address of this user.</value>
        public string EmailAddress { get; internal set; }

        /// <summary>
        ///     Gets the unique ID of this user.
        /// </summary>
        /// <value>The unique ID of this user.</value>
        public int Id { get; }

        /// <summary>
        ///     Gets the date and time at which this user was last online.
        /// </summary>
        /// <value>A <see cref="DateTimeOffset" /> representing the date and time this user was last online.</value>
        public DateTimeOffset LastLogin { get; internal set; }

        /// <summary>
        ///     Gets the name of this user.
        /// </summary>
        /// <value>The name of this user.</value>
        public string Name { get; internal set; }

        /// <summary>
        ///     Gets the duration for which this user has been online.
        /// </summary>
        /// <value>A <see cref="TimeSpan" /> representing the duration for which this user has been online.</value>
        public TimeSpan OnlineTime { get; internal set; }

        /// <summary>
        ///     Gets the date and time at which this user was registered.
        /// </summary>
        /// <value>A <see cref="DateTimeOffset" /> representing the date and time this user was registered.</value>
        public DateTimeOffset RegistrationTime { get; internal set; }

        /// <summary>
        ///     Returns a value indicating whether the two given users are equal.
        /// </summary>
        /// <param name="left">The first user to compare.</param>
        /// <param name="right">The second user to compare.</param>
        /// <returns><see langword="true" /> if the two users are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(VirtualParadiseUser left, VirtualParadiseUser right) => Equals(left, right);

        /// <summary>
        ///     Returns a value indicating whether the two given users are equal.
        /// </summary>
        /// <param name="left">The first user to compare.</param>
        /// <param name="right">The second user to compare.</param>
        /// <returns><see langword="true" /> if the two users are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(VirtualParadiseUser left, VirtualParadiseUser right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value indicating whether this user and another user are equal.
        /// </summary>
        /// <param name="other">The user to compare with this instance.</param>
        /// <returns><see langword="true" /> if the two users are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(VirtualParadiseUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_client, other._client) && Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return obj is VirtualParadiseUser other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_client, Id);

        /// <summary>
        ///     Invites this user to a specified location.
        /// </summary>
        /// <param name="location">
        ///     The invitation location. If <see langword="null" />, the client's current location is used.
        /// </param>
        public async Task<InviteResponse> InviteAsync(Location? location = null)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            location ??= _client.CurrentAvatar.Location;
            TaskCompletionSource<ReasonCode> taskCompletionSource;

            lock (_client.Lock)
            {
                int reference = ObjectReferenceCounter.GetNextReference();
                taskCompletionSource = _client.AddInviteCompletionSource(reference);

                string world = location.Value.World.Name;
                (double x, double y, double z) = location.Value.Position;
                (double pitch, double yaw, _) = location.Value.Rotation;

                vp_int_set(_client.NativeInstanceHandle, IntegerAttribute.ReferenceNumber, reference);
                vp_invite(_client.NativeInstanceHandle, Id, world, x, y, z, (float) yaw, (float) pitch);
            }

            ReasonCode reason = await taskCompletionSource.Task;
            return reason switch
            {
                ReasonCode.Success => InviteResponse.Accepted,
                ReasonCode.InviteDeclined => InviteResponse.Declined,
                ReasonCode.Timeout => InviteResponse.TimeOut,
                ReasonCode.NoSuchUser => throw new UserNotFoundException($"Cannot invite non-existent user {Id}."),
                var _ => throw new InvalidOperationException(
                    $"An error occurred trying to invite the user: {reason:D} ({reason:G})")
            };
        }

        /// <summary>
        ///     Sends a to join request to the user.
        /// </summary>
        /// <param name="suppressTeleport">
        ///     If <see langword="true" />, the client's avatar will not teleport to the requested location automatically.
        ///     Be careful, there is no way to retrieve
        /// </param>
        /// <returns>The result of the request.</returns>
        /// <exception cref="UserNotFoundException">This user is invalid and cannot be joined.</exception>
        /// <exception cref="InvalidOperationException">An unexpected error occurred trying to join the user.</exception>
        public async Task<JoinResult> JoinAsync(bool suppressTeleport = false)
        {
            // ReSharper disable InconsistentlySynchronizedField
            IntPtr handle = _client.NativeInstanceHandle;
            TaskCompletionSource<ReasonCode> taskCompletionSource;

            lock (_client.Lock)
            {
                int reference = ObjectReferenceCounter.GetNextReference();
                vp_int_set(handle, IntegerAttribute.ReferenceNumber, reference);
                vp_join(handle, Id);

                taskCompletionSource = _client.AddJoinCompletionSource(reference);
            }

            ReasonCode reason = await taskCompletionSource.Task;
            Location? location = null;

            if (reason == ReasonCode.Success)
            {
                string worldName;
                double x, y, z;
                double yaw, pitch;

                lock (_client.Lock)
                {
                    x = vp_double(handle, FloatAttribute.JoinX);
                    y = vp_double(handle, FloatAttribute.JoinY);
                    z = vp_double(handle, FloatAttribute.JoinZ);

                    yaw = vp_double(handle, FloatAttribute.JoinYaw);
                    pitch = vp_double(handle, FloatAttribute.JoinPitch);

                    worldName = vp_string(handle, StringAttribute.JoinWorld);
                }

                var position = new Vector3d(x, y, z);
                var rotation = new Vector3d(yaw, pitch, 0);
                var world = await _client.GetWorldAsync(worldName);

                location = new Location(world, position, rotation);

                if (!suppressTeleport)
                    await _client.CurrentAvatar.TeleportAsync(location.Value);
            }

            JoinResponse response = reason switch
            {
                ReasonCode.Success => JoinResponse.Accepted,
                ReasonCode.JoinDeclined => JoinResponse.Declined,
                ReasonCode.Timeout => JoinResponse.TimeOut,
                ReasonCode.NoSuchUser => throw new UserNotFoundException($"Cannot join non-existent user {Id}."),
                var _ => throw new InvalidOperationException(
                    $"An error occurred trying to join the user: {reason:D} ({reason:G})")
            };
            // ReSharper enable InconsistentlySynchronizedField

            return new JoinResult(response, location);
        }

        /// <inheritdoc />
        public override string ToString() => $"User [Id={Id}, Name={Name}]";
    }
}
