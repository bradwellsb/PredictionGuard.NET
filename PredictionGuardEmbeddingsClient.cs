﻿using PredictionGuard.Config;
using PredictionGuard.Models.Embeddings;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

public class PredictionGuardEmbeddingsClient
{
    private readonly PredictionGuardEmbeddingsClientOptions _options;
    private readonly HttpClient _httpClient;

    public PredictionGuardEmbeddingsClient(PredictionGuardEmbeddingsClientOptions options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = _options.Endpoint;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<float[]> GenerateEmbeddingsAsync(string input)
    {
        var inputs = new List<EmbeddingInput>
        {
            new EmbeddingInput { Text = input }
        };

        var embeddings = await GenerateEmbeddingsAsync(inputs);
        return embeddings.FirstOrDefault()?.Embedding;
    }

    public async Task<List<EmbeddingData>> GenerateEmbeddingsAsync(List<EmbeddingInput> inputs)
    {
        var request = new EmbeddingsRequest
        {
            Model = _options.Model,
            Input = inputs
        };

        return await SendEmbeddingsRequestAsync(request);
    }

    public async Task<List<EmbeddingData>> GenerateEmbeddingsAsync(IEnumerable<string> inputs)
    {
        var request = new EmbeddingsRequest
        {
            Model = _options.Model,
            Input = inputs
        };

        return await SendEmbeddingsRequestAsync(request);
    }

    private async Task<List<EmbeddingData>> SendEmbeddingsRequestAsync(EmbeddingsRequest request)
    {
        var requestJson = JsonSerializer.Serialize(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint}embeddings")
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var embeddingsResponse = JsonSerializer.Deserialize<EmbeddingsResponse>(responseJson);

        return embeddingsResponse?.Data ?? new List<EmbeddingData>();
    }
}
