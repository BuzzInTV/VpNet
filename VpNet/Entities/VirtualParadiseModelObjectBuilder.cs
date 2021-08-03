using System;
using VpNet.Internal;
using VpNet.NativeApi;
using static VpNet.Internal.Native;

namespace VpNet.Entities
{
    /// <summary>
    ///     Provides mutability for <see cref="VirtualParadiseObject" />.
    /// </summary>
    public sealed class VirtualParadiseModelObjectBuilder
    {
        private readonly VirtualParadiseClient _client;
        private readonly ObjectBuilderMode _mode;

        internal VirtualParadiseModelObjectBuilder(VirtualParadiseClient client, ObjectBuilderMode mode)
        {
            _client = client;
            _mode = mode;
        }

        /// <summary>
        ///     Gets or sets the value of this object's <c>Action</c> field.
        /// </summary>
        /// <value>The value of this object's <c>Action</c> field, or <see langword="null" /> to leave unchanged.</value>
        public string Action { get; set; }

        /// <summary>
        ///     Gets or sets the value of this object's <c>Description</c> field.
        /// </summary>
        /// <value>The value of this object's <c>Description</c> field, or <see langword="null" /> to leave unchanged.</value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the value of this object's <c>Model</c> field.
        /// </summary>
        /// <value>The value of this object's <c>Model</c> field, or <see langword="null" /> to leave unchanged.</value>
        public string Model { get; set; }

        /// <summary>
        ///     Gets or sets the date and time at which this object was last modified.
        /// </summary>
        /// <value>
        ///     The date and time at which this object was last modified, or <see langword="null" /> to leave unchanged.
        /// </value>
        /// <remarks>
        ///     This property may only be set during an object load, and will throw <see cref="InvalidOperationException" /> at
        ///     any other point.
        /// </remarks>
        public DateTimeOffset? ModificationTimestamp { get; set; }

        /// <summary>
        ///     Gets or sets the owner of this object.
        /// </summary>
        /// <value>The owner of this object, or <see langword="null" /> to leave unchanged.</value>
        /// <remarks>
        ///     This property may only be set during an object load, and will throw <see cref="InvalidOperationException" /> at
        ///     any other point.
        /// </remarks>
        public VirtualParadiseUser Owner { get; set; }

        /// <summary>
        ///     Gets or sets the position of the object.
        /// </summary>
        /// <value>The position of the object, or <see langword="null" /> to leave unchanged.</value>
        public Vector3d? Position { get; set; }

        /// <summary>
        ///     Gets or sets the rotation of the object.
        /// </summary>
        /// <value>The rotation of the object, or <see langword="null" /> to leave unchanged.</value>
        public Vector3d? Rotation { get; set; }

        /// <summary>
        ///     Sets the value of this object's <c>Action</c> field.
        /// </summary>
        /// <param name="action">The new value of the <c>Action</c> field, or <see langword="null" /> to leave unchanged.</param>
        /// <returns>The current instance of this builder.</returns>
        public VirtualParadiseModelObjectBuilder WithAction(string action)
        {
            Action = action;
            return this;
        }

        /// <summary>
        ///     Sets the value of this object's <c>Description</c> field.
        /// </summary>
        /// <param name="description">
        ///     The new value of the <c>Description</c> field, or <see langword="null" /> to leave unchanged.
        /// </param>
        /// <returns>The current instance of this builder.</returns>
        public VirtualParadiseModelObjectBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        ///     Sets the value of this object's <c>Model</c> field.
        /// </summary>
        /// <param name="model">The new value of the <c>Model</c> field, or <see langword="null" /> to leave unchanged.</param>
        /// <returns>The current instance of this builder.</returns>
        public VirtualParadiseModelObjectBuilder WithModel(string model)
        {
            Model = model;
            return this;
        }

        internal void ApplyChanges()
        {
            IntPtr handle = _client.NativeInstanceHandle;

            if (Action is { } action) vp_string_set(handle, StringAttribute.ObjectAction, action);
            if (Description is { } description) vp_string_set(handle, StringAttribute.ObjectDescription, description);
            if (Model is { } model) vp_string_set(handle, StringAttribute.ObjectModel, model);

            if (Position is { } position)
            {
                (double x, double y, double z) = position;
                vp_double_set(handle, FloatAttribute.ObjectX, x);
                vp_double_set(handle, FloatAttribute.ObjectY, y);
                vp_double_set(handle, FloatAttribute.ObjectZ, z);
            }
            else if (_mode == ObjectBuilderMode.Create)
            {
                throw new ArgumentException("Position must be assigned when creating a new object.");
            }

            if (Rotation is null && _mode == ObjectBuilderMode.Create)
            {
                Rotation = Vector3d.Zero;
            }

            if (Rotation is { } rotation)
            {
                // TODO add angle/axis support (see issue #3)
                (double x, double y, double z) = rotation;
                vp_double_set(handle, FloatAttribute.ObjectRotationX, x);
                vp_double_set(handle, FloatAttribute.ObjectRotationY, y);
                vp_double_set(handle, FloatAttribute.ObjectRotationZ, z);
                vp_double_set(handle, FloatAttribute.ObjectRotationAngle, double.PositiveInfinity);
            }

            if (ModificationTimestamp is { } modificationTimestamp)
            {
                if (_mode != ObjectBuilderMode.Load)
                    throw new InvalidOperationException("Modification timestamp can only be assigned during an object load.");

                vp_int_set(handle, IntegerAttribute.ObjectTime, (int) modificationTimestamp.ToUnixTimeSeconds());
            }

            if (Owner is { } owner)
            {
                if (_mode != ObjectBuilderMode.Load)
                    throw new InvalidOperationException("Owner can only be assigned during an object load.");

                vp_int_set(handle, IntegerAttribute.ObjectUserId, owner.Id);
            }
        }
    }
}
