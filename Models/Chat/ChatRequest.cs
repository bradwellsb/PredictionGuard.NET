using PredictionGuard.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PredictionGuard.Models.Chat
{
    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("max_tokens")] //Deprecated. Should use max_completion_tokens, but not working
        public int MaxCompletionTokens { get; set; }

        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; }
    }
}
