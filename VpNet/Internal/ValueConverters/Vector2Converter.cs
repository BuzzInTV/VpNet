﻿using System.IO;
using System.Numerics;
using Cysharp.Text;
using VpNet.Extensions;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class Vector2Converter : ValueConverter<Vector2>
    {
        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out Vector2 result)
        {
            using var builder = new Utf8ValueStringBuilder(false);
            int spaceCount = 0;

            while (true)
            {
                int readChar = reader.Read();

                char currentChar = (char) readChar;
                if (currentChar == ' ')
                    spaceCount++;

                if (spaceCount < 2 && readChar != -1)
                    continue;

                result = builder.AsSpan().ToVector2();
                break;
            }
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, Vector2 value)
        {
            writer.Write($"{value.X} {value.Y}");
        }
    }
}