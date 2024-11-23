# PredictionGuard.NET Class Library

The PredictionGuard.NET class library provides a .NET Standard 2.1 wrapper for the PredictionGuard API. The library supports the chat completions endpoint (with and without streaming), including function calling.

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

### Retrieve the PredictionGuard Client from the Service Container
```csharp
var predictionGuardClient = app.Services.GetRequiredService<PredictionGuardClient>();
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

static string GetWeather(string location)
{
    return Random.Shared.NextDouble() > 0.5 ? $"It's sunny in {location}" : $"It's raining in {location}";
}
```

## Note
To avoid runtime errors when referencing the PredictionGuard.NET library as a .dll in your project, ensure that all necessary dependencies are included.
```
<ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
</ItemGroup>
```