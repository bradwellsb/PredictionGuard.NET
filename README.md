# PredictionGuard.NET Class Library

PredictionGuard.NET is a .NET Standard 2.1 wrapper for the PredictionGuard API. The library supports the chat completions endpoint (with and without streaming), including function calling.

## Features

- **Chat Completions**: Generate text completions based on input prompts.
- **Function Calling**: Invoke functions based on chat input.
- **Embeddings**: Generate embeddings for text and image inputs.

## Usage
### Chat Completions
#### Add the PredictionGuard Chat Client to the service container
```csharp
builder.Services.AddPredictionGuardChatClient(config["ApiKey"]);
```

#### Add the PredictionGuard Chat Client with Options
Customize the endpoint, specify a model, etc.
```csharp
builder.Services.AddPredictionGuardChatClient(config["ApiKey"], options =>
{
    options.Endpoint = new Uri(config["Endpoint"]);
    options.Model = config["Model"];
});
```

#### Retrieve the PredictionGuard Chat Client from the Service Container through dependency injection
Console Application
```csharp
var predictionGuardChatClient = app.Services.GetRequiredService<PredictionGuardChatClient>();
```

Blazor
```csharp
@inject PredictionGuardChatClient PredictionGuardChatClient
```

API controller
```csharp
private readonly PredictionGuardChatClient _predictionGuardChatClient;

public MyController(PredictionGuardChatClient predictionGuardChatClient)
{
    _predictionGuardChatClient = predictionGuardChatClient;
}
```

#### Generate Chat Completions
```csharp
var responseText = await predictionGuardChatClient.CompleteAsync(input);
```

#### Generate Chat Completions with Streaming
```csharp
await foreach (var chunk in predictionGuardChatClient.CompleteStreamingAsync(input))
{
    Console.Write(chunk);
}
```

#### Enable Function Calling and Make Tools Available
The library is capable of invoking one or more functions to assist with the user query. Function output is sent back to the API, which then generates a natural language response based on the data.
```csharp
builder.Services.AddPredictionGuardChatClient(config["ApiKey"])
    .UseFunctionInvocation();

MethodInfo getForecastMethod = ToolBuilder.GetMethod<WeatherService>("GetForecast");
ChatOptions chatOptions = new()
{
    Tools = [ getForecastMethod ]
};

var predictionGuardChatClient = app.Services.GetRequiredService<PredictionGuardChatClient>();

var responseText = await predictionGuardChatClient.CompleteAsync("Do I need an umbrella today in Nantes?", chatOptions);

public class Weather
{
    public int Day { get; set; }
    public string Location { get; set; }    
    public string Summary { get; set; }    
}

[Description("Gets a weather forecast for the given number of days")]
public List<Weather> GetForecast(string location, int numDays)
{
    List<Weather> forecast = new();
    for (int i = 0; i < numDays; i++)
    {
        forecast.Add(new Weather() { Day = i, Location = location, Summary = Random.Shared.NextDouble() > 0.5 ? "Sunny" : "Rainy" });
    }
    return forecast;
}
```

### Embeddings
#### Add the PredictionGuard Embeddings Client to the service container
```csharp
builder.Services.AddPredictionGuardEmbeddingsClient(config["ApiKey"]);
```

#### Add the PredictionGuard Embeddings Client with Options
Customize the endpoint, specify a model, etc.
```csharp
builder.Services.AddPredictionGuardEmbeddingsClient(config["ApiKey"], options =>
{
    options.Endpoint = new Uri(config["Endpoint"]);
    options.Model = config["Model"];
});
```

#### Generate Embeddings from a Single Text Input
```csharp
var embeddings = await predictionGuardEmbeddingsClient.GenerateEmbeddingsAsync("Your input text here");
```

#### Generate Embeddings from Multiple Inputs
```csharp
var inputs = new List<EmbeddingInput>
{
    new EmbeddingInput
    {
        Text = "Your input text here"
    },
    new EmbeddingInput
    {
        Text = "Another input"
    }
};
var embeddings = await predictionGuardEmbeddingsClient.GenerateEmbeddingsAsync(inputs);
    foreach (var embeddingData in embeddings)
    {
        Console.WriteLine($"Index: {embeddingData.Index}");
        Console.WriteLine($"Embedding: {string.Join(", ", embeddingData.Embedding)}");
    }
```