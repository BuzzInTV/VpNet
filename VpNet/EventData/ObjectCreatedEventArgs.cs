using System;
using VpNet.Entities;

namespace VpNet.EventData
{
    /// <summary>
    ///     Provides event arguments for <see cref="VirtualParadiseClient.ObjectCreated" />.
    /// </summary>
    public sealed class ObjectCreatedEventArgs : EventArgs
    {
        /// <inheritdoc />
        public ObjectCreatedEventArgs(VirtualParadiseAvatar avatar, VirtualParadiseObject theObject)
        {
            Avatar = avatar;
            Object = theObject;
        }

        /// <summary>
        ///     Gets the avatar responsible for the object being created.
        /// </summary>
        /// <value>The avatar responsible for the object being created.</value>
        public VirtualParadiseAvatar Avatar { get; }

        /// <summary>
        ///     Gets the object which was created.
        /// </summary>
        /// <value>The object which was created.</value>
        public VirtualParadiseObject Object { get; }
    }
}
