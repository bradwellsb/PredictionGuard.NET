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
You are a function-calling AI assistant. You have access to the following functions:
{toolsJson}
Use the following JSON schema for function calls:
{{
    ""title"": ""FunctionCall"",
    ""type"": ""object"",
    ""properties"": {{
    ""arguments"": {{""type"": ""object""}},
    ""name"": {{""type"": ""string""}}
    }},
    ""required"": [""arguments"", ""name""]
}}
When you need to call a function, output a JSON object within `function_call` tags as follows:
`function_call`
{{
    ""name"": ""<function_name>"", ""arguments"": <args_dict>
}}
`function_call`
";

            return new ChatMessage(ChatRole.System, systemContent);
        }
    }
}
