using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PredictionGuard.Models.Embeddings
{
    public class EmbeddingsResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }
    }

    public class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
