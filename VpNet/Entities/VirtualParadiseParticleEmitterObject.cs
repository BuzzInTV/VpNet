﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using Cysharp.Text;
using VpNet.Internal.Attributes;
using VpNet.Internal.ValueConverters;

namespace VpNet.Entities
{
    public sealed class VirtualParadiseParticleEmitterObject : VirtualParadiseObject
    {
        /// <inheritdoc />
        internal VirtualParadiseParticleEmitterObject(VirtualParadiseClient client, int id)
            : base(client, id)
        {
        }

        /// <summary>
        ///     Gets the maximum acceleration.
        /// </summary>
        /// <value>The maximum acceleration.</value>
        [SerializationKey("acceleration_max")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d AccelerationMax { get; private set; }

        /// <summary>
        ///     Gets the minimum acceleration.
        /// </summary>
        /// <value>The minimum acceleration.</value>
        [SerializationKey("acceleration_min")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d AccelerationMin { get; private set; }

        /// <summary>
        ///     Gets the blend mode.
        /// </summary>
        /// <value>The blend more.</value>
        [SerializationKey("blend")]
        [ValueConverter(typeof(StringToEnumConverter<ParticleBlendMode>))]
        public ParticleBlendMode BlendMode { get; private set; }

        /// <summary>
        ///     Gets the maximum color.
        /// </summary>
        /// <value>The maximum color.</value>
        [SerializationKey("color_max")]
        [ValueConverter(typeof(HexToColorConverter))]
        public Color ColorMax { get; private set; }

        /// <summary>
        ///     Gets the minimum color.
        /// </summary>
        /// <value>The minimum color.</value>
        [SerializationKey("color_min")]
        [ValueConverter(typeof(HexToColorConverter))]
        public Color ColorMin { get; private set; }

        /// <summary>
        ///     Gets the emitter lifespan.
        /// </summary>
        /// <value>The emitter lifespan.</value>
        [SerializationKey("emitter_lifespan")]
        [ValueConverter(typeof(MillisecondToTimeSpanConverter))]
        public TimeSpan EmitterLifespan { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this emitter interpolates its values.
        /// </summary>
        /// <value><see langword="true" /> if this emitter interpolates its values; otherwise, <see langword="false" />.</value>
        [SerializationKey("interpolate")]
        [ValueConverter(typeof(IntToBoolConverter))]
        public bool Interpolate { get; private set; }

        /// <summary>
        ///     Gets the opacity.
        /// </summary>
        /// <value>The opacity.</value>
        [SerializationKey("opacity")]
        public double Opacity { get; private set; }

        /// <summary>
        ///     Gets the particle lifespan.
        /// </summary>
        /// <value>The particle lifespan.</value>
        [SerializationKey("particle_lifespan")]
        [ValueConverter(typeof(MillisecondToTimeSpanConverter))]
        public TimeSpan ParticleLifespan { get; private set; }

        /// <summary>
        ///     Gets the particle type.
        /// </summary>
        /// <value>The particle type.</value>
        [SerializationKey("particle_type")]
        [ValueConverter(typeof(StringToEnumConverter<ParticleType>))]
        public ParticleType ParticleType { get; private set; }

        /// <summary>
        ///     Gets the release count.
        /// </summary>
        /// <value>The release count.</value>
        [SerializationKey("release_count")]
        public int ReleaseCount { get; private set; }

        [SerializationKey("release_time")]
        [ValueConverter(typeof(MillisecondToTimeSpanConverter))]
        public TimeSpan ReleaseTime { get; private set; }

        /// <summary>
        ///     Gets the maximum size.
        /// </summary>
        /// <value>The maximum size.</value>
        [SerializationKey("size_max")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d SizeMax { get; private set; }

        /// <summary>
        ///     Gets the minimum size.
        /// </summary>
        /// <value>The minimum size.</value>
        [SerializationKey("size_min")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d SizeMin { get; private set; }

        /// <summary>
        ///     Gets the maximum speed.
        /// </summary>
        /// <value>The maximum speed.</value>
        [SerializationKey("speed_max")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d SpeedMax { get; private set; }

        /// <summary>
        ///     Gets the minimum speed.
        /// </summary>
        /// <value>The minimum speed.</value>
        [SerializationKey("speed_min")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d SpeedMin { get; private set; }

        /// <summary>
        ///     Gets the maximum spin.
        /// </summary>
        /// <value>The maximum spin.</value>
        [SerializationKey("spin_max")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d SpinMax { get; private set; }

        /// <summary>
        ///     Gets the minimum spin.
        /// </summary>
        /// <value>The minimum spin.</value>
        [SerializationKey("spin_min")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d SpinMin { get; private set; }

        /// <summary>
        ///     Gets the maximum start angle.
        /// </summary>
        /// <value>The maximum start angle.</value>
        [SerializationKey("start_angle_max")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d StartAngleMax { get; private set; }

        /// <summary>
        ///     Gets the minimum start angle.
        /// </summary>
        /// <value>The minimum start angle.</value>
        [SerializationKey("start_angle_min")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d StartAngleMin { get; private set; }

        /// <summary>
        ///     Gets the maximum volume.
        /// </summary>
        /// <value>The maximum volume.</value>
        [SerializationKey("volume_max")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d VolumeMax { get; private set; }

        /// <summary>
        ///     Gets the minimum volume.
        /// </summary>
        /// <value>The minimum volume.</value>
        [SerializationKey("volume_min")]
        [ValueConverter(typeof(Vector3dConverter))]
        public Vector3d VolumeMin { get; private set; }

        /// <summary>
        ///     Gets the tag.
        /// </summary>
        /// <value>The tag.</value>
        [SerializationKey("tag")]
        public string Tag { get; private set; }

        /// <summary>
        ///     Gets the texture.
        /// </summary>
        /// <value>The texture.</value>
        [SerializationKey("texture")]
        public string Texture { get; private set; }

        protected override void ExtractFromData(ReadOnlySpan<byte> data)
        {
#pragma warning disable 612
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo[] properties = typeof(VirtualParadiseParticleEmitterObject).GetProperties(bindingFlags);
            var keymap = new Dictionary<string, PropertyInfo>();
            var converterMap = new Dictionary<string, ValueConverter>();

            foreach (var property in properties)
            {
                var serializationKeyAttribute = property.GetCustomAttribute<SerializationKeyAttribute>();
                if (serializationKeyAttribute is null)
                    continue;

                keymap.Add(serializationKeyAttribute.Key, property);

                var converterAttribute = property.GetCustomAttribute<ValueConverterAttribute>();
                if (converterAttribute is not null)
                {
                    var converterType = converterAttribute.ConverterType;
                    if (Activator.CreateInstance(converterType) is ValueConverter converter)
                        converterMap.Add(serializationKeyAttribute.Key, converter);
                }
            }
#pragma warning restore 612

            Span<char> text = stackalloc char[data.Length];
            Encoding.UTF8.GetChars(data, text);

            using var keyBuffer = new Utf8ValueStringBuilder(false);
            using var valueBuffer = new Utf8ValueStringBuilder(false);
            bool isKey = true;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                switch (current)
                {
                    case '=' when isKey:
                        isKey = false;
                        break;

                    case '\n':
                    case '\r':
                        string key = keyBuffer.ToString();
                        string valueString = valueBuffer.ToString();
                        object value = valueString;

                        if (keymap.TryGetValue(key, out var property))
                        {
                            if (converterMap.TryGetValue(key, out var converter))
                            {
                                using var reader = new StringReader(valueString);
                                converter.Deserialize(reader, out value);
                            }
                            else if (property.PropertyType != typeof(string))
                            {
                                value = Convert.ChangeType(value, property.PropertyType);
                            }

                            property.SetValue(this, value);
                        }

                        keyBuffer.Clear();
                        valueBuffer.Clear();
                        isKey = true;
                        break;

                    case var _ when isKey:
                        keyBuffer.Append(current);
                        break;

                    case var _:
                        valueBuffer.Append(current);
                        break;
                }
            }
        }
    }
}