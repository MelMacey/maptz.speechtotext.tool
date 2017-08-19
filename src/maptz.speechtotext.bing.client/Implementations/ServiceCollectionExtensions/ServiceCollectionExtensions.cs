using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maptz.SpeechToText.Bing.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBingSpeechToTextService(this IServiceCollection serviceCollection, BingSpeechToTextServiceOptions options = null)
        {
            serviceCollection.AddScoped<ISpeechToTextService, BingSpeechToTextSocketService>();
            if (options != null)
            {
                serviceCollection.AddScoped<IOptions<BingSpeechToTextServiceOptions>>(sp => Options.Create(options));
            }
            return serviceCollection;
        }
    }
}
