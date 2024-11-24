# PredictionGuard.NET Class Library

PredictionGuard.NET is a .NET Standard 2.1 wrapper for the PredictionGuard API. The library supports the chat completions endpoint (with and without streaming), including function calling.

## Features

- **Chat Completions**: Generate text completions based on input prompts.
- **Function Calling**: Invoke functions based on chat input.

## Usage
### Add the PredictionGuard client to the service container
```csharp
builder.Services.AddPredictionGuardClient(config["ApiKey"]);
```

### Add the PredictionGuard Client with Options
Customize the endpoint, specify a model, etc.
```csharp
builder.Services.AddPredictionGuardClient(config["ApiKey"], options =>
    {
        options.Endpoint = new Uri(config["Endpoint"]);
        options.Model = config["Model"];
    });
```

### Retrieve the PredictionGuard Client from the Service Container through dependency injection
Console Application
```csharp
var predictionGuardClient = app.Services.GetRequiredService<PredictionGuardClient>();
```

Blazor
```csharp
@inject PredictionGuardClient PredictionGuardClient
```

API controller
```csharp
private readonly PredictionGuardClient _predictionGuardClient;

public MyController(PredictionGuardClient predictionGuardClient)
{
    _predictionGuardClient = predictionGuardClient;
}
```

### Generate Chat Completions
```csharp
var responseText = await predictionGuardClient.CompleteAsync(input);
```

### Generate Chat Completions with Streaming
```csharp
await foreach (var chunk in predictionGuardClient.CompleteStreamingAsync(input))
{
    Console.Write(chunk);
}
```

### Enable Function Calling and Make Tools Available
```csharp
builder.Services.AddPredictionGuardClient(config["ApiKey"])
    .UseFunctionInvocation();

MethodInfo getWeatherMethod = ToolBuilder.GetMethod<WeatherService>("GetWeather");
ChatOptions chatOptions = new()
{
    Tools = [ getWeatherMethod ]
};

var predictionGuardClient = app.Services.GetRequiredService<PredictionGuardClient>();

var responseText = await predictionGuardClient.CompleteAsync("Do I need an umbrella in Nantes?", chatOptions);

[Description("Gets the weather")]
static string GetWeather(string location)
{
    return Random.Shared.NextDouble() > 0.5 ? $"It's sunny in {location}" : $"It's raining in {location}";
}
```