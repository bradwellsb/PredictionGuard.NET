using PredictionGuard.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace PredictionGuard.Services
{
    public class ToolBuilder
    {
        public static Tool ConstructToolFromMethod(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var tool = new Tool
            {
                Type = "function",
                Function = new Function
                {
                    Name = methodInfo.Name,
                    Description = GetMethodDescription(methodInfo),
                    Parameters = GetMethodParameters(methodInfo)
                }
            };

            return tool;
        }

        public static MethodInfo GetMethod<T>(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            var methodInfo = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                throw new ArgumentException($"Method '{methodName}' not found in type '{typeof(T).FullName}'.");
            }

            return methodInfo;
        }

        private static string GetMethodDescription(MethodInfo methodInfo)
        {
            var descriptionAttribute = methodInfo.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute != null ? descriptionAttribute.Description : methodInfo.Name;
        }

        private static List<Parameter> GetMethodParameters(MethodInfo methodInfo)
        {
            var parameters = new List<Parameter>();
            foreach (var param in methodInfo.GetParameters())
            {
                parameters.Add(new Parameter
                {
                    Name = param.Name,
                    Type = param.ParameterType.Name,
                });
            }
            return parameters;
        }

        public static ChatMessage CreateSystemMessage(List<MethodInfo> toolMethods)
        {
            var tools = toolMethods?.Select(method => ConstructToolFromMethod(method)).ToList();
            var toolsJson = JsonSerializer.Serialize(tools, new JsonSerializerOptions { WriteIndented = true });

            var systemContent = $@"
You are a function calling AI model. You are provided with function signatures within <tools></tools> XML tags. You may call one or more functions to assist with the user query. Don't make assumptions about what values to plug into functions. Here are the available tools: <tools>{toolsJson}</tools>
Use the following pydantic model json schema for each tool call you will make:
{{
    ""title"": ""FunctionCall"",
    ""type"": ""object"",
    ""properties"": {{
    ""arguments"": {{""type"": ""object""}},
    ""name"": {{""type"": ""string""}}
    }},
    ""required"": [""arguments"", ""name""]
}}
For each function call return a json object with function name and arguments within <tool_call></tool_call> XML tags as follows:
<tool_call>
{{""name"": ""<function_name>"", ""arguments"": <args_dict>}}
</tool_call>
";

            return new ChatMessage(ChatRole.System, systemContent);
        }
    }
}
