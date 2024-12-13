using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using PredictionGuard.Config;
using System;
using System.Net.Http;

namespace PredictionGuard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPredictionGuardChatClient(this IServiceCollection services, string apiKey, Action<PredictionGuardConfigureOptions> configureOptions = null)
        {
            var configureOptionsInstance = new PredictionGuardConfigureOptions();
            configureOptions?.Invoke(configureOptionsInstance);

            services.Configure<PredictionGuardChatClientOptions>(options =>
            {
                options.ApiKey = apiKey;
                options.Endpoint = configureOptionsInstance.Endpoint;
                options.Model = configureOptionsInstance.Model ?? new PredictionGuardChatClientOptions().Model;
            });

            services.AddHttpClient();
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>(); // Remove HTTP client logging
            services.AddScoped<PredictionGuardChatClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<PredictionGuardChatClientOptions>>().Value;
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new PredictionGuardChatClient(options, httpClientFactory);
            });
            return services;
        }

        public static IServiceCollection AddPredictionGuardEmbeddingsClient(this IServiceCollection services, string apiKey, Action<PredictionGuardConfigureOptions> configureOptions = null)
        {
            var configureOptionsInstance = new PredictionGuardConfigureOptions();
            configureOptions?.Invoke(configureOptionsInstance);

            services.Configure<PredictionGuardEmbeddingsClientOptions>(options =>
            {
                options.ApiKey = apiKey;
                options.Endpoint = configureOptionsInstance.Endpoint;
                options.Model = configureOptionsInstance.Model ?? new PredictionGuardEmbeddingsClientOptions().Model;
            });

            services.AddHttpClient();
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>(); // Remove HTTP client logging
            services.AddScoped<PredictionGuardEmbeddingsClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<PredictionGuardEmbeddingsClientOptions>>().Value;
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new PredictionGuardEmbeddingsClient(options, httpClientFactory);
            });
            return services;
        }

        public static IServiceCollection UseFunctionInvocation(this IServiceCollection services)
        {
            PredictionGuardChatClient.EnableFunctionInvocation = true;
            return services;
        }
    }
}
