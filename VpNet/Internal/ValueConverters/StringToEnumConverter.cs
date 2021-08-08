using System;
using System.IO;
using System.Linq;
using System.Reflection;
using VpNet.Internal.Attributes;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class StringToEnumConverter<T> : ValueConverter<T>
        where T : struct, Enum
    {
        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out T result)
        {
            string value = reader.ReadToEnd();
            
            var field = typeof(T).GetFields().FirstOrDefault(f => string.Equals(f.GetCustomAttribute<SerializationKeyAttribute>()?.Key, value));
            if (field is not null)
            {
                result = (T)field.GetValue(Enum.GetValues<T>()[0])!;
            }
            else
            {
                result = Enum.Parse<T>(value, true);
            }
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, T value)
        {
            writer.Write(value.ToString().ToLowerInvariant());
        }
    }
}
