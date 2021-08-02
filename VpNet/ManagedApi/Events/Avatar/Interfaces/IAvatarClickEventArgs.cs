using System.Numerics;

namespace VpNet.Interfaces
{
    /// <summary>
    /// Avater enter event arguments templated interface specifications.
    /// </summary>
    /// <typeparam name="TAvatar">The type of the avatar.</typeparam>
    public interface IAvatarClickEventArgs
    {
        /// <summary>
        /// Gets or sets the avatar.
        /// </summary>
        /// <value>
        /// The avatar.
        /// </value>
        IAvatar Avatar { get; set; }
        /// <summary>
        /// Gets or sets the clicked avatar.
        /// </summary>
        /// <value>
        /// The avatar.
        /// </value>
        IAvatar ClickedAvatar { get; set; }
        /// <summary>
        /// Gets or sets the world hit coordinates
        /// </summary>
        /// <value>
        /// The world hit coordinates
        /// </value>
        Vector3 WorldHit { get; set; }
    }
}