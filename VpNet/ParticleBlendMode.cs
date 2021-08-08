using VpNet.Internal.Attributes;

namespace VpNet
{
    /// <summary>
    ///     An enumeration of particle blend modes.
    /// </summary>
    public enum ParticleBlendMode
    {
        /// <summary>
        ///     Normal blending.
        /// </summary>
        Normal,

        /// <summary>
        ///     Additive blending.
        /// </summary>
        [SerializationKey("add")]
        Additive
    }
}
