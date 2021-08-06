using System.IO;
using Cysharp.Text;
using VpNet.Extensions;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class IntToBoolConverter : ValueConverter<bool>
    {
        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out bool result)
        {
            using var builder = new Utf8ValueStringBuilder(false);
            int read;
            while ((read = reader.Read()) != -1)
            {
                char current = (char)read;
                builder.Append(current);
            }

            result = builder.AsSpan().ToInt32() != 0;
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, bool value)
        {
            writer.Write(value ? 1 : 0);
        }
    }
}
