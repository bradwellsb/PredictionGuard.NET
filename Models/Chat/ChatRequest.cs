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

        [JsonPropertyName("tools")]
        public List<Tool> Tools { get; set; }
    }
}
