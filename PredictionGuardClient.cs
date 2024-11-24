using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using PredictionGuard.Config;
using PredictionGuard.Models;
using PredictionGuard.Services;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;

public class PredictionGuardClient
{
    private readonly PredictionGuardClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly List<ChatMessage> _messages = new List<ChatMessage>();
    public static bool EnableFunctionInvocation { get; set; } = false;

    public PredictionGuardClient(PredictionGuardClientOptions options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = _options.Endpoint;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async IAsyncEnumerable<string> CompleteStreamingAsync(string userInput, ChatOptions options = null)
    {        
        if (EnableFunctionInvocation && !_messages.Any(m => m.Role == ChatRole.System))
        {
            _messages.Add(ToolBuilder.CreateSystemMessage(options?.Tools));
        }
        _messages.Add(new ChatMessage(ChatRole.User, userInput));

        var request = new ChatRequest
        {
            Model = _options.Model,
            Messages = _messages,
            Stream = true,
        };
        var tools = options?.Tools?.Select(method => ToolBuilder.ConstructToolFromMethod(method)).ToList();

        if (tools != null)
            request.Tools = tools;

        var requestJson = JsonSerializer.Serialize(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint}chat/completions")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var responseText = new StringBuilder();
        using var responseStream = await response.Content.ReadAsStreamAsync();
        using var streamReader = new StreamReader(responseStream);
        string line;
        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var jsonData = line.Substring(6);
                if (jsonData == "[DONE]") break;

                var chunk = JsonSerializer.Deserialize<ChatCompletion>(jsonData);
                if (chunk != null && chunk.Choices != null)
                {
                    foreach (var choice in chunk.Choices)
                    {
                        if (choice.Delta != null && choice.Delta.Content != null)
                        {
                            responseText.Append(choice.Delta.Content);
                            yield return choice.Delta.Content;

                            if (EnableFunctionInvocation)
                            {
                                var result = CheckForFunctionCall(responseText.ToString(), options);
                                if (result != null)
                                {
                                    if(result is string)
                                    {
                                        yield return result as string;
                                    }
                                    yield break;
                                }
                            }
                        }
                    }
                }
            }
        }

        _messages.Add(new ChatMessage(ChatRole.Assistant, responseText.ToString()));
    }

    public async Task<object> CompleteAsync(string userInput, ChatOptions options = null)
    {
        if (EnableFunctionInvocation && !_messages.Any(m => m.Role == ChatRole.System))
        {
            _messages.Add(ToolBuilder.CreateSystemMessage(options?.Tools));
        }
        _messages.Add(new ChatMessage(ChatRole.User, userInput));

        var request = new ChatRequest
        {
            Model = _options.Model,
            Messages = _messages,
            Stream = false,
        };
        var tools = options?.Tools?.Select(method => ToolBuilder.ConstructToolFromMethod(method)).ToList();

        if (tools != null)
            request.Tools = tools;

        var requestJson = JsonSerializer.Serialize(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint}chat/completions")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();
        var result = ProcessResponse(responseText, options);

        //if result is string, add to messages
        if (result is string)
            _messages.Add(new ChatMessage(ChatRole.Assistant, result as string));
        return result;
    }

    private object ProcessResponse(string responseText, ChatOptions options)
    {
        var completionResponse = JsonSerializer.Deserialize<ChatCompletion>(responseText);

        if (completionResponse != null && completionResponse.Choices != null)
        {
            foreach (var choice in completionResponse.Choices)
            {
                if (choice.Message != null && choice.Message.Content != null)
                {
                    if (EnableFunctionInvocation)
                    {
                        var result = CheckForFunctionCall(choice.Message.Content, options);
                        if (result != null) return result;
                    }

                    return choice.Message.Content;
                }
            }
        }
        return string.Empty;
    }

    private object CheckForFunctionCall(string content, ChatOptions options)
    {
        var functionCallMatch = Regex.Match(content, @"<tool_call>(.*?)</tool_call>", RegexOptions.Singleline);
        if (functionCallMatch.Success)
        {
            var functionCallJson = functionCallMatch.Groups[1].Value.Replace("\\n", "").Trim();
            var functionCall = JsonSerializer.Deserialize<FunctionCall>(functionCallJson);

            if (functionCall != null)
            {
                var result = InvokeTool(functionCall, options?.Tools);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return null;
    }

    private object InvokeTool(FunctionCall functionCall, List<MethodInfo> toolMethods)
    {
        var method = toolMethods?.FirstOrDefault(m => m.Name == functionCall.Name);
        if (method != null)
        {
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (functionCall.Arguments.TryGetValue(parameters[i].Name, out var value))
                {
                    try
                    {
                        if (parameters[i].ParameterType == typeof(string))
                        {
                            args[i] = value.ToString();
                        }
                        else
                        {
                            args[i] = JsonSerializer.Deserialize(value.ToString(), parameters[i].ParameterType);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting argument '{parameters[i].Name}': {ex.Message}");
                        return null;
                    }
                }
            }

            object instance = null;
            if (!method.IsStatic)
            {
                var declaringType = method.DeclaringType;
                if (declaringType != null)
                {
                    instance = Activator.CreateInstance(declaringType);
                }
            }

            var result = method.Invoke(instance, args);
            return result;
        }
        return null;
    }
}
