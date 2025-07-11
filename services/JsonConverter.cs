﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Xml;

namespace Util.Services
{
    public class ConverterJson
    {
        public ConverterJson() { }

        public TContext DeserializeFromJson<TContext>(string json) => JsonConvert.DeserializeObject<TContext>(json, Converter.Settings);
        public TContext[] DeserializeFromArrayJson<TContext>(string json) => JsonConvert.DeserializeObject<TContext[]>(json, Converter.Settings);
        public string SerializeToJson<TContext>(TContext self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public TContext XmlToJson<TContext>(string xml)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml);
            string fromXml = JsonConvert.SerializeXmlNode(xmldoc).Replace("@", "");
            return JsonConvert.DeserializeObject<TContext>(fromXml, Converter.Settings);
        }
    }
    public static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
  
    public class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
