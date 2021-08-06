using System;
using VpNet.Entities;

namespace VpNet.EventData
{
    /// <summary>
    ///     Provides event arguments for <see cref="VirtualParadiseClient.ObjectClicked" />.
    /// </summary>
    public sealed class ObjectClickedEventArgs : EventArgs
    {
        /// <inheritdoc />
        public ObjectClickedEventArgs(
            VirtualParadiseAvatar avatar,
            VirtualParadiseObject virtualParadiseObject,
            Vector3d clickPoint)
        {
            Avatar = avatar;
            Object = virtualParadiseObject;
            ClickPoint = clickPoint;
        }

        /// <summary>
        ///     Gets the avatar responsible for the click.
        /// </summary>
        /// <value>The avatar responsible for the click.</value>
        public VirtualParadiseAvatar Avatar { get; }

        /// <summary>
        ///     Gets the point at which the avatar clicked the object.
        /// </summary>
        /// <value>The click point.</value>
        public Vector3d ClickPoint { get; }

        /// <summary>
        ///     Gets the clicked object.
        /// </summary>
        /// <value>The clicked object.</value>
        public VirtualParadiseObject Object { get; }
    }
}
