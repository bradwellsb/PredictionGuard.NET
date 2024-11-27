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
The library is capable of invoking one or more functions to assist with the user query. Function output is sent back to the API, which then generates a natural language response based on the data.
```csharp
builder.Services.AddPredictionGuardClient(config["ApiKey"])
    .UseFunctionInvocation();

MethodInfo getForecastMethod = ToolBuilder.GetMethod<WeatherService>("GetForecast");
ChatOptions chatOptions = new()
{
    Tools = [ getForecastMethod ]
};

var predictionGuardClient = app.Services.GetRequiredService<PredictionGuardClient>();

var responseText = await predictionGuardClient.CompleteAsync("Do I need an umbrella today in Nantes?", chatOptions);

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