using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PredictionGuard.Models.Embeddings
{
    public class EmbeddingsRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("input")]
        [JsonConverter(typeof(EmbeddingInputConverter))]
        public object Input { get; set; }
    }

    public class EmbeddingInput
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }

    public class EmbeddingInputConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var inputs = new List<EmbeddingInput>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    if (reader.TokenType == JsonTokenType.String)
                    {
                        inputs.Add(new EmbeddingInput { Text = reader.GetString() });
                    }
                    else if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var input = JsonSerializer.Deserialize<EmbeddingInput>(ref reader, options);
                        inputs.Add(input);
                    }
                }
                return inputs;
            }
            throw new JsonException("Unexpected JSON format for EmbeddingInput.");
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            if (value is IEnumerable<EmbeddingInput> embeddingInputs)
            {
                foreach (var input in embeddingInputs)
                {
                    if (!string.IsNullOrEmpty(input.Text) && string.IsNullOrEmpty(input.Image))
                    {
                        writer.WriteStringValue(input.Text);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, input, options);
                    }
                }
            }
            else if (value is IEnumerable<string> stringInputs)
            {
                foreach (var input in stringInputs)
                {
                    writer.WriteStringValue(input);
                }
            }
            writer.WriteEndArray();
        }
    }
}
