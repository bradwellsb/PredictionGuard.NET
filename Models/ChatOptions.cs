using System.Collections.Generic;
using System.Reflection;

namespace PredictionGuard.Models
{
    public class ChatOptions
    {
        public List<MethodInfo> Tools { get; set; } = new List<MethodInfo>();
    }
}
