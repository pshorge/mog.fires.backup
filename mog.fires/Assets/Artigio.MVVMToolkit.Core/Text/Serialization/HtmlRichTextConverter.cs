using System;
using System.Linq;
using System.Reflection;
using Artigio.MVVMToolkit.Core.Text.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Artigio.MVVMToolkit.Core.Text.Serialization
{
    public class HtmlRichTextConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            return value?.HtmlToRichText() ?? string.Empty;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
    
    public class HtmlRichTextContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
        
            if (property.PropertyType == typeof(string) && 
                member.GetCustomAttributes(typeof(HtmlRichTextAttribute), true).Any())
            {
                property.Converter = new HtmlRichTextConverter();
            }
        
            return property;
        }
    }
}