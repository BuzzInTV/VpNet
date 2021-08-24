using System.IO;
using Cysharp.Text;
using VpNet.Extensions;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class VectorToNthComponentConverter : ValueConverter<float>
    {
        private readonly int _componentNumber;

        /// <inheritdoc />
        public VectorToNthComponentConverter(int componentNumber)
        {
            _componentNumber = componentNumber;
        }

        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out float result)
        {
            using var builder = new Utf8ValueStringBuilder(false);
            int spaceCount = 0;

            while (true)
            {
                int readChar = reader.Read();

                if (readChar == -1)
                    break;

                char currentChar = (char) readChar;
                if (currentChar == ' ')
                    spaceCount++;
                else if (spaceCount == _componentNumber - 1)
                    builder.Append(currentChar);
            }

            result = builder.AsSpan().ToSingle();
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, float value)
        {
            writer.Write(value);
        }
    }
}
