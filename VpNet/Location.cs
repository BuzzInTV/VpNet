using System;
using System.Numerics;
using VpNet.Entities;

namespace VpNet
{
    /// <summary>
    ///     Represents a location within Virtual Paradise. This structure is readonly.
    /// </summary>
    public readonly struct Location : IEquatable<Location>
    {
        /// <summary>
        ///     Represents a location which does not correspond to anywhere in Virtual Paradise.
        /// </summary>
        public static readonly Location Nowhere = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Location" /> struct.
        /// </summary>
        /// <param name="world">The world which this location will represent.</param>
        /// <param name="position">The position which this location will represent.</param>
        /// <param name="rotation">The rotation which this location will represent.</param>
        public Location(VirtualParadiseWorld world, Vector3d position, Quaternion rotation)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        ///     Gets the cell corresponding to this location.
        /// </summary>
        public Cell Cell
        {
            get
            {
                int x = (int)Math.Floor(Position.X);
                int z = (int)Math.Floor(Position.Z);
                return new Cell(x, z);
            }
        }

        /// <summary>
        ///     Gets the position which this location represents.
        /// </summary>
        /// <value>The position which this location represents.</value>
        public Vector3d Position { get; }

        /// <summary>
        ///     Gets the rotation which this location represents.
        /// </summary>
        /// <value>The rotation which this location represents.</value>
        public Quaternion Rotation { get; }

        /// <summary>
        ///     Gets the world which this location represents.
        /// </summary>
        /// <value>The world which this location represents.</value>
        public VirtualParadiseWorld World { get; }

        /// <inheritdoc />
        public bool Equals(Location other) =>
            Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Equals(World, other.World);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Location other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Position, Rotation, World);

        public static bool operator ==(Location left, Location right) => left.Equals(right);

        public static bool operator !=(Location left, Location right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"Location [World={World}, Position={Position}, Rotation={Rotation}]";
    }
}
