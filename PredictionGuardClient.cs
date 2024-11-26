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

    public async IAsyncEnumerable<string> CompleteStreamingAsync(string input, ChatOptions options = null)
    {
        CheckAddSystemMessage(options);
        _messages.Add(new ChatMessage(ChatRole.User, input));

        var chatRequest = BuildChatRequest(true, options);
        var apiResponse = await SendChatCompletionRequest(chatRequest);
        var apiResponseText = new StringBuilder();

        using var responseStream = await apiResponse.Content.ReadAsStreamAsync();
        using var streamReader = new StreamReader(responseStream);
        string line;
        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var jsonData = line.Substring(6);
                if (jsonData == "[DONE]") break;

                var chunk = JsonSerializer.Deserialize<ChatCompletion>(jsonData);
                if (chunk?.Choices != null)
                {
                    foreach (var choice in chunk.Choices)
                    {
                        if (choice.Delta?.Content != null)
                        {
                            apiResponseText.Append(choice.Delta.Content);
                            yield return choice.Delta.Content;
                        }
                    }
                }
            }
        }
        var assistantResponse = new ChatMessage(ChatRole.Assistant, apiResponseText.ToString());
        _messages.Add(assistantResponse);

        if (EnableFunctionInvocation)
        {
            var results = CheckForFunctionCalls(apiResponseText.ToString(), options);
            if (results.Count > 0)
            {
                input = string.Empty;
                foreach (var result in results)
                {
                    input += $"<tool_response>{JsonSerializer.Serialize(result)}</tool_response>";
                }
                _messages.Add(new ChatMessage(ChatRole.User, input));
                chatRequest = BuildChatRequest(false, options);
                apiResponse = await SendChatCompletionRequest(chatRequest);
                apiResponseText = new StringBuilder(await apiResponse.Content.ReadAsStringAsync());
                assistantResponse = ProcessResponse(apiResponseText.ToString(), options);
                _messages.Add(assistantResponse);
                yield return assistantResponse.Content;
            }
        }
    }

    public async Task<string> CompleteAsync(string input, ChatOptions options = null)
    {
        CheckAddSystemMessage(options);
        _messages.Add(new ChatMessage(ChatRole.User, input));

        var chatRequest = BuildChatRequest(false, options);
        var apiResponse = await SendChatCompletionRequest(chatRequest);
        var apiResponseText = await apiResponse.Content.ReadAsStringAsync();
        var assistantResponse = ProcessResponse(apiResponseText, options);
        _messages.Add(assistantResponse);
        if (EnableFunctionInvocation)
        {
            var results = CheckForFunctionCalls(assistantResponse.Content, options);
            if (results.Count > 0)
            {
                input = string.Empty;
                foreach (var result in results)
                {
                    input += $"<tool_response>{JsonSerializer.Serialize(result)}</tool_response>";
                }
                _messages.Add(new ChatMessage(ChatRole.User, input));
                chatRequest = BuildChatRequest(false, options);
                apiResponse = await SendChatCompletionRequest(chatRequest);
                apiResponseText = await apiResponse.Content.ReadAsStringAsync();
                assistantResponse = ProcessResponse(apiResponseText.ToString(), options);
                _messages.Add(assistantResponse);
                return assistantResponse.Content;
            }
        }
        return assistantResponse.Content;
    }

    private void CheckAddSystemMessage(ChatOptions options = null)
    {
        if (EnableFunctionInvocation && !_messages.Any(m => m.Role == ChatRole.System))
        {
            _messages.Add(ToolBuilder.CreateSystemMessage(options?.Tools));
        }
    }

    private ChatRequest BuildChatRequest(bool stream = false, ChatOptions options = null)
    {
        var request = new ChatRequest
        {
            Model = _options.Model,
            Messages = _messages,
            Stream = stream,
            Tools = options?.Tools?.Select(ToolBuilder.ConstructToolFromMethod).ToList()
        };
        return request;
    }

    private async Task<HttpResponseMessage> SendChatCompletionRequest(ChatRequest request)
    {
        var requestJson = JsonSerializer.Serialize(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint}chat/completions")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private ChatMessage ProcessResponse(string responseText, ChatOptions options)
    {
        var completionResponse = JsonSerializer.Deserialize<ChatCompletion>(responseText);

        if (completionResponse?.Choices != null)
        {
            foreach (var choice in completionResponse.Choices)
            {
                if (choice.Message?.Content != null)
                {
                    return new ChatMessage(ChatRole.Assistant, choice.Message.Content);
                }
            }
        }
        return new ChatMessage(ChatRole.Assistant, string.Empty);
    }

    private List<object> CheckForFunctionCalls(string content, ChatOptions options)
    {
        var functionCalls = new List<object>();
        var functionCallMatches = Regex.Matches(content, @"<tool_call>(.*?)</tool_call>", RegexOptions.Singleline);

        foreach (Match match in functionCallMatches)
        {
            var functionCallJson = match.Groups[1].Value.Replace("\\n", "").Trim();
            var functionCall = JsonSerializer.Deserialize<FunctionCall>(functionCallJson);

            if (functionCall != null)
            {
                var result = InvokeTool(functionCall, options?.Tools);
                if (result != null)
                {
                    functionCalls.Add(result);
                }
            }
        }

        return functionCalls;
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
                        args[i] = parameters[i].ParameterType == typeof(string) ? value.ToString() : JsonSerializer.Deserialize(value.ToString(), parameters[i].ParameterType);
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

            return method.Invoke(instance, args);
        }
        return null;
    }
}
