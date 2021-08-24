using System;
using System.Drawing;
using System.Threading.Tasks;

namespace VpNet.Entities
{
    /// <summary>
    ///     Represents a world.
    /// </summary>
    public sealed class VirtualParadiseWorld : IEquatable<VirtualParadiseWorld>
    {
        private readonly VirtualParadiseClient _client;

        internal VirtualParadiseWorld(VirtualParadiseClient client, string name)
        {
            Name = name;
            _client = client;
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
        ///     Gets the settings for this world.
        /// </summary>
        /// <value>The settings for this world.</value>
        public WorldSettings Settings { get; internal set; } = new();
        
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
        
        /// <summary>
        ///     Modifies the world settings globally.
        /// </summary>
        /// <param name="action">The builder which defines parameters to change.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action" /> is <see langword="null" />.</exception>
        /// <exception cref="UnauthorizedAccessException">The client does not have permission to modify world settings.</exception>
        public async ValueTask ModifyAsync(Action<WorldSettingsBuilder> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));

            var builder = new WorldSettingsBuilder(_client);
            await Task.Run(() => action(builder));

            builder.SendChanges();
        }

        /// <inheritdoc />
        public override string ToString() => $"World [Name={Name}]";
    }
}
