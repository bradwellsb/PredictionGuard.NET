using System;

namespace PredictionGuard.Config
{
    public class PredictionGuardEmbeddingsClientOptions
    {
        public string ApiKey { get; set; }
        public Uri Endpoint { get; set; }
        public string Model { get; set; } = "bridgetower-large-itm-mlm-itc"; //default model if not specified
    }
}
