using System;
using System.Threading.Tasks;
using VpNet.Internal;
using VpNet.NativeApi;
using static VpNet.Internal.Native;

namespace VpNet.Entities
{
    /// <summary>
    ///     Represents an object which renders as a 3D model. A "model" object will contain a <c>Model</c>, <c>Description</c>
    ///     and <c>Action</c> field.
    /// </summary>
    public class VirtualParadiseModelObject : VirtualParadiseObject
    {
        /// <inheritdoc />
        internal VirtualParadiseModelObject(VirtualParadiseClient client, int id)
            : base(client, id)
        {
        }

        /// <summary>
        ///     Gets the value of this object's <c>Description</c> field.
        /// </summary>
        /// <value>The value of this object's <c>Description</c> field.</value>
        public string Action { get; internal set; }

        /// <summary>
        ///     Gets the value of this object's <c>Description</c> field.
        /// </summary>
        /// <value>The value of this object's <c>Description</c> field.</value>
        public string Description { get; internal set; }

        /// <summary>
        ///     Gets the value of this object's <c>Model</c> field.
        /// </summary>
        /// <value>The value of this object's <c>Model</c> field.</value>
        public string Model { get; internal set; }

        /// <summary>
        ///     Modifies the object.
        /// </summary>
        /// <param name="action">The builder which defines parameters to change.</param>
        /// <exception cref="InvalidOperationException">
        ///     <para><see cref="VirtualParadiseModelObjectBuilder.ModificationTimestamp" /> was assigned.</para>
        ///     -or-
        ///     <para><see cref="VirtualParadiseModelObjectBuilder.Owner" /> was assigned.</para>
        /// </exception>
        public async Task ModifyAsync(Action<VirtualParadiseModelObjectBuilder> action)
        {
            if (action is null) ThrowHelper.ThrowArgumentNullException(nameof(action));

            var builder = new VirtualParadiseModelObjectBuilder(Client, ObjectBuilderMode.Modify);
            await Task.Run(() => action!(builder));

            lock (Client.Lock)
            {
                IntPtr handle = Client.NativeInstanceHandle;
                vp_int_set(handle, IntegerAttribute.ObjectId, Id);
                builder.ApplyChanges();

                vp_object_change(handle);
            }
        }
    }
}
