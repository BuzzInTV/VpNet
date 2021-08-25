using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using VpNet.Internal.Attributes;

namespace VpNet.Internal.ValueConverters
{
    internal static class WorldSettingsConverter
    {
        public static IReadOnlyDictionary<string, string> ToDictionary(WorldSettingsBuilder settings)
        {
            var dictionary = new Dictionary<string, string>();
            var properties = settings.GetType().GetProperties();

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<SerializationKeyAttribute>();
                if (attribute is null)
                    continue;

                object propertyValue = property.GetValue(settings);
                Type propertyType = property.PropertyType;

                if (propertyValue is null)
                    continue;

                string result = propertyValue.ToString();

                var converterAttribute = property.GetCustomAttribute<ValueConverterAttribute>();
                if (converterAttribute is not null)
                {
#pragma warning disable 612
                    var converterType = converterAttribute.ConverterType;

                    ValueConverter converter;
                    if (converterAttribute.UseArgs)
                        converter = Activator.CreateInstance(converterType, converterAttribute.Args) as ValueConverter;
                    else
                        converter = Activator.CreateInstance(converterType) as ValueConverter;

                    if (converter is not null)
                    {
                        using var writer = new StringWriter();
                        converter.Serialize(writer, propertyValue);
                        result = writer.ToString();
                    }
#pragma warning restore 612
                }
                else
                {
                    if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                        result = (bool)propertyValue ? "1" : "0";
                }

                if (result is not null)
                    dictionary.Add(attribute.Key, result);
            }

            return dictionary;
        }

        public static WorldSettings FromDictionary(IReadOnlyDictionary<string, string> dictionary)
        {
            var settings = new WorldSettings();
            var properties = typeof(WorldSettings).GetProperties();

            foreach (var property in properties)
            {
                var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValueAttribute is null)
                    continue;

                property.SetValue(settings, defaultValueAttribute.Value);
            }

            foreach ((string key, string value) in dictionary)
            {
                var property = properties.FirstOrDefault(p =>
                    string.Equals(p.GetCustomAttribute<SerializationKeyAttribute>()?.Key, key,
                        StringComparison.OrdinalIgnoreCase));

                if (property is null)
                    continue;

                using var reader = new StringReader(value);
                object propertyValue = value;
                Type converterType = null;

                var converterAttribute = property.GetCustomAttribute<ValueConverterAttribute>();
                if (converterAttribute is not null)
                {
                    converterType = converterAttribute.ConverterType;
                }
                else
                {
                    Type propertyType = property.PropertyType;

                    if (propertyType == typeof(bool))
                        propertyValue = value == "1" || (bool.TryParse(value, out var result) && result);
                    else if (propertyType == typeof(int))
                        propertyValue = int.TryParse(value, out var result) ? result : 0;
                    else if (propertyType == typeof(float))
                        propertyValue = float.TryParse(value, out var result) ? result : 0.0f;
                    else if (propertyType == typeof(double))
                        propertyValue = double.TryParse(value, out var result) ? result : 0.0;
                    else if (propertyType.IsEnum && int.TryParse(value, out var result))
                        propertyValue = Convert.ChangeType(result, propertyType);
                }

                // ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable 612
                if (converterType is not null && converterAttribute is not null)
                {
                    ValueConverter converter;
                    if (converterAttribute.UseArgs)
                        converter = Activator.CreateInstance(converterType, converterAttribute.Args) as ValueConverter;
                    else
                        converter = Activator.CreateInstance(converterType) as ValueConverter;

                    converter?.Deserialize(reader, out propertyValue);
                }
#pragma warning restore 612
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                property.SetValue(settings, propertyValue);
            }

            return settings;
        }
    }
}
