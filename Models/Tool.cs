using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PredictionGuard.Models
{
    public class Tool
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public Function Function { get; set; }
    }

    public class Function
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("parameters")]
        public List<Parameter> Parameters { get; set; }
    }

    public class FunctionCall
    {
        [JsonPropertyName("arguments")]
        public Dictionary<string, object> Arguments { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Parameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
