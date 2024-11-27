using System.Text.Json.Serialization;

namespace PredictionGuard.Models
{
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}
