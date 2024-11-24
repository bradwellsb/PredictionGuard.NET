using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PredictionGuard.Models
{
    public enum ChatCompletionResponseType
    {
        Text,
        Function
    }
    public class ChatCompletionResult
    {
        public ChatCompletionResponseType Type { get; set; }
        public string Text { get; set; }
        public List<object> FunctionResults { get; set; }
    }

    public class ChatCompletion
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public List<ChatCompletionChoice> Choices { get; set; }
    }

    public class ChatCompletionChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("delta")]
        public ChatCompletionDelta Delta { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public ChatCompletionMessage Message { get; set; }
    }

    public class ChatCompletionDelta
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class ChatCompletionMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }    
}
