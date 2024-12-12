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
        public static IServiceCollection AddPredictionGuardClient(this IServiceCollection services, string apiKey, Action<PredictionGuardConfigureOptions> configureOptions = null)
        {
            var configureOptionsInstance = new PredictionGuardConfigureOptions();
            configureOptions?.Invoke(configureOptionsInstance);

            services.Configure<PredictionGuardClientOptions>(options =>
            {
                options.ApiKey = apiKey;
                options.Endpoint = configureOptionsInstance.Endpoint;
                options.Model = configureOptionsInstance.Model;
            });

            services.AddHttpClient();
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>(); // Remove HTTP client logging
            services.AddScoped<PredictionGuardClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<PredictionGuardClientOptions>>().Value;
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new PredictionGuardClient(options, httpClientFactory);
            });
            return services;
        }

        public static IServiceCollection UseFunctionInvocation(this IServiceCollection services)
        {
            PredictionGuardClient.EnableFunctionInvocation = true;
            return services;
        }
    }
}