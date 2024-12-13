using System;
using System.Collections.Generic;
using System.Text;

namespace PredictionGuard.Config
{
    public class PredictionGuardConfigureOptions
    {
        public Uri Endpoint { get; set; } = new Uri("https://api.predictionguard.com");
        public string Model { get; set; }
    }
}
