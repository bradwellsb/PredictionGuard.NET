using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PredictionGuard.Models.Embeddings
{
    public class EmbeddingsRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("input")]
        public List<EmbeddingInput> Input { get; set; }
    }

    public class EmbeddingInput
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }
}
