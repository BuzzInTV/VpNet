using VpNet.Internal.Attributes;

namespace VpNet
{
    /// <summary>
    ///     An enumeration of particle types.
    /// </summary>
    public enum ParticleType
    {
        Sprite,

        Facer,

        [SerializationKey("flat_panel")]
        FlatPanel,

        [SerializationKey("double_flat_panel")]
        DoubleSidedFlatPanel
    }
}
