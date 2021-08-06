using System;
using System.IO;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class StringToEnumConverter<T> : ValueConverter<T>
        where T : struct, Enum
    {
        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out T result)
        {
            result = Enum.Parse<T>(reader.ReadToEnd(), true);
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, T value)
        {
            writer.Write(value.ToString().ToLowerInvariant());
        }
    }
}
