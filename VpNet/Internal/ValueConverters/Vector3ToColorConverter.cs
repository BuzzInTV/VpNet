using System.IO;
using Cysharp.Text;
using VpNet.Extensions;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class Vector3ToColorConverter : ValueConverter<ColorF>
    {
        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out ColorF result)
        {
            using var builder = new Utf8ValueStringBuilder(false);
            int spaceCount = 0;

            while (true)
            {
                int readChar = reader.Read();

                char currentChar = (char) readChar;
                if (currentChar == ' ')
                    spaceCount++;

                if (spaceCount < 3 && readChar != -1)
                    continue;

                (float x, float y, float z) = builder.AsSpan().ToVector3();
                result = ColorF.FromArgb(x, y, z);
                break;
            }
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, ColorF value)
        {
            writer.Write($"{value.R} {value.G} {value.B} {value.A}");
        }
    }
}
