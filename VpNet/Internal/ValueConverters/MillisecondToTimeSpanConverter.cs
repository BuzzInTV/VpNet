using System;
using System.IO;
using Cysharp.Text;
using VpNet.Extensions;

namespace VpNet.Internal.ValueConverters
{
    internal sealed class MillisecondToTimeSpanConverter : ValueConverter<TimeSpan>
    {
        /// <inheritdoc />
        public override void Deserialize(TextReader reader, out TimeSpan result)
        {
            using var builder = new Utf8ValueStringBuilder(false);
            int read;
            while ((read = reader.Read()) != -1)
            {
                char current = (char)read;
                builder.Append(current);
            }

            result = TimeSpan.FromMilliseconds(builder.AsSpan().ToDouble());
        }

        /// <inheritdoc />
        public override void Serialize(TextWriter writer, TimeSpan value)
        {
            writer.Write(value.TotalMilliseconds);
        }
    }
}
