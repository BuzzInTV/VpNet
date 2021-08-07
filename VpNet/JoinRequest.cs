using System;
using System.Threading.Tasks;
using VpNet.Entities;
using VpNet.Extensions;
using VpNet.Internal;

namespace VpNet
{
    /// <summary>
    ///     Represents a join request, which the client may either accept or decline.
    /// </summary>
    public sealed class JoinRequest : IEquatable<JoinRequest>
    {
        private readonly VirtualParadiseClient _client;
        private readonly int _requestId;

        internal JoinRequest(VirtualParadiseClient client, int requestId, string name, VirtualParadiseUser user)
        {
            Name = name;
            User = user;
            _client = client;
            _requestId = requestId;
        }

        /// <summary>
        ///     Gets the name of the avatar which sent the request.
        /// </summary>
        /// <value>The name of the avatar which sent the request.</value>
        public string Name { get; }

        /// <summary>
        ///     Gets the user which sent the request.
        /// </summary>
        /// <value>The user which sent the request.</value>
        public VirtualParadiseUser User { get; }

        /// <inheritdoc />
        public bool Equals(JoinRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _requestId == other._requestId && _client.Equals(other._client);
        }

        /// <summary>
        ///     Accepts this join request.
        /// </summary>
        /// <param name="location">
        ///     Optional. The target location of the join. Defaults to the client's current avatar location.
        /// </param>
        public ValueTask AcceptAsync(Location? location = null)
        {
            location ??= _client.CurrentAvatar.Location;
            string worldName = location.Value.World.Name;
            (double x, double y, double z) = location.Value.Position;
            (double pitch, double yaw, double _) = location.Value.Rotation.ToEulerAngles();

            lock (_client.Lock)
                Native.vp_join_accept(_client.NativeInstanceHandle, _requestId, worldName, x, y, z, (float)yaw, (float)pitch);

            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     Declines this join request.
        /// </summary>
        public ValueTask DeclineAsync()
        {
            lock (_client.Lock) Native.vp_join_decline(_client.NativeInstanceHandle, _requestId);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return obj is JoinRequest other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_client, _requestId);

        public static bool operator ==(JoinRequest left, JoinRequest right) => Equals(left, right);

        public static bool operator !=(JoinRequest left, JoinRequest right) => !Equals(left, right);
    }
}
