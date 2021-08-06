using System;

namespace VpNet.Internal.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class SerializationKeyAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializationKeyAttribute" /> class.
        /// </summary>
        /// <param name="key">The key name.</param>
        public SerializationKeyAttribute(string key)
        {
            Key = key ?? throw ThrowHelper.ArgumentNullException(nameof(key));
        }

        /// <summary>
        ///     Gets the value of the key.
        /// </summary>
        /// <value>The value of the key.</value>
        public string Key { get; }
    }
}
