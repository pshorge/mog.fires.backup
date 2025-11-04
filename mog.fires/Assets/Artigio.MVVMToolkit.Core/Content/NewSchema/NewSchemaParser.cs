using Newtonsoft.Json;

namespace Artigio.MVVMToolkit.Core.Content.NewSchema
{
    public static class NewSchemaParser
    {
        public static NewRoot Parse(string json) =>
            string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<NewRoot>(json);
    }
}