using System.IO;
using Cysharp.Text;
using VpNet.Extensions;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class Vector3dConverter : ValueConverter<Vector3d>
    {
        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out Vector3d result)
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

                result = builder.AsSpan().ToVector3d();
                break;
            }
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, Vector3d value)
        {
            writer.Write($"{value.X} {value.Y} {value.Z}");
        }
    }
}
