using System;

namespace PredictionGuard.Config
{
    public class PredictionGuardChatClientOptions
    {
        public string ApiKey { get; set; }
        public Uri Endpoint { get; set; }
        public string Model { get; set; } = "Hermes-3-Llama-3.1-8B"; //default model if not specified
    }
}
