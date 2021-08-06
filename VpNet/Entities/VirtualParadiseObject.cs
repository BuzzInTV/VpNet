using System;
using System.Numerics;
using System.Threading.Tasks;
using VpNet.Exceptions;
using VpNet.Extensions;
using VpNet.Internal;
using VpNet.NativeApi;
using static VpNet.Internal.Native;

namespace VpNet.Entities
{
    /// <summary>
    ///     Represents the base class for all in-world objects.
    /// </summary>
    public abstract class VirtualParadiseObject : IEquatable<VirtualParadiseObject>
    {
        protected internal VirtualParadiseObject(VirtualParadiseClient client, int id)
        {
            Client = client;
            Id = id;
        }

        /// <summary>
        ///     Gets the unique ID of this object.
        /// </summary>
        /// <value>The unique ID of this object.</value>
        public int Id { get; }

        /// <summary>
        ///     Gets the location of this object.
        /// </summary>
        /// <value>The location of this object.</value>
        public Location Location { get; internal set; }

        private protected VirtualParadiseClient Client { get; }

        /// <summary>
        ///     Returns a value indicating whether the two given objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><see langword="true" /> if the two objects are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(VirtualParadiseObject left, VirtualParadiseObject right) => Equals(left, right);

        /// <summary>
        ///     Returns a value indicating whether the two given objects are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><see langword="true" /> if the two objects are not equal; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(VirtualParadiseObject left, VirtualParadiseObject right) => !Equals(left, right);

        /// <summary>
        ///     Clicks the object.
        /// </summary>
        /// <param name="position">The position at which to click the object.</param>
        /// <param name="target">
        ///     The target avatar which will receive the event, or <see langword="null" /> to broadcast to every avatar.
        /// </param>
        /// <exception cref="InvalidOperationException"><paramref name="target" /> is the client's current avatar.</exception>
        public Task ClickAsync(Vector3d? position = null, VirtualParadiseAvatar target = null)
        {
            if (target == Client.CurrentAvatar)
                return ThrowHelper.CannotUseSelfExceptionAsync();

            lock (Client.Lock)
            {
                int session = target?.Session ?? 0;
                (float x, float y, float z) = (Vector3)(position ?? Vector3d.Zero);

                vp_object_click(Client.NativeInstanceHandle, Id, session, x, y, z);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Deletes this object.
        /// </summary>
        /// <exception cref="InvalidOperationException">The client is not connected to a world.</exception>
        /// <exception cref="ObjectNotFoundException">The object does not exist.</exception>
        public Task DeleteAsync()
        {
            lock (Client.Lock)
            {
                var reason = (ReasonCode)vp_object_delete(Client.NativeInstanceHandle, Id);

                switch (reason)
                {
                    case ReasonCode.NotInWorld:
                        return ThrowHelper.NotInWorldExceptionAsync();

                    case ReasonCode.ObjectNotFound:
                        return ThrowHelper.ObjectNotFoundExceptionAsync();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Returns a value indicating whether this object and another object are equal.
        /// </summary>
        /// <param name="other">The object to compare with this instance.</param>
        /// <returns><see langword="true" /> if the two objects are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(VirtualParadiseObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Location.World.Equals(other.Location.World) && Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VirtualParadiseObject)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Location.World, Id);

        protected internal virtual void ExtractFromInstance(IntPtr handle)
        {
            Span<byte> data = Span<byte>.Empty;
            IntPtr dataPtr = vp_data(handle, DataAttribute.ObjectData, out int length);

            if (length > 0)
            {
                unsafe
                {
                    data = new Span<byte>(dataPtr.ToPointer(), length);
                }
            }

            ExtractFromData(data);
        }

        protected virtual void ExtractFromData(ReadOnlySpan<byte> data)
        {
        }
    }
}
