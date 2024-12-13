using System.Collections.Generic;
using System.Reflection;

namespace PredictionGuard.Models.Chat
{
    public class ChatOptions
    {
        public List<MethodInfo> Tools { get; set; } = new List<MethodInfo>();
    }
}
