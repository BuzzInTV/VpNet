using System;
using System.Drawing;

namespace VpNet.Entities
{
    /// <summary>
    ///     Represents a world.
    /// </summary>
    public sealed class VirtualParadiseWorld : IEquatable<VirtualParadiseWorld>
    {
        internal VirtualParadiseWorld(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the number of avatars currently in this world.
        /// </summary>
        /// <value>The number of avatars currently in this world.</value>
        public int AvatarCount { get; internal set; }

        /// <summary>
        ///     Gets the name of this world.
        /// </summary>
        public string Name { get; } = string.Empty;
        
        /// <summary>
        ///     Gets the size of this world.
        /// </summary>
        /// <value>The size of this world.</value>
        public Size Size { get; internal set; }

        /// <summary>
        ///     Gets the state of this world.
        /// </summary>
        /// <value>The state of this world.</value>
        public WorldState State { get; internal set; } = WorldState.Unknown;

        /// <summary>
        ///     Returns a value indicating whether the two given worlds are equal.
        /// </summary>
        /// <param name="left">The first world to compare.</param>
        /// <param name="right">The second world to compare.</param>
        /// <returns><see langword="true" /> if the two worlds are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(VirtualParadiseWorld left, VirtualParadiseWorld right) => Equals(left, right);

        /// <summary>
        ///     Returns a value indicating whether the two given worlds are equal.
        /// </summary>
        /// <param name="left">The first world to compare.</param>
        /// <param name="right">The second world to compare.</param>
        /// <returns><see langword="true" /> if the two worlds are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(VirtualParadiseWorld left, VirtualParadiseWorld right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value indicating whether this world and another world are equal.
        /// </summary>
        /// <param name="other">The world to compare with this instance.</param>
        /// <returns><see langword="true" /> if the two worlds are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(VirtualParadiseWorld other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return obj is VirtualParadiseWorld other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => (Name != null ? Name.GetHashCode() : 0);

        /// <inheritdoc />
        public override string ToString() => $"World [Name={Name}]";
    }
}
