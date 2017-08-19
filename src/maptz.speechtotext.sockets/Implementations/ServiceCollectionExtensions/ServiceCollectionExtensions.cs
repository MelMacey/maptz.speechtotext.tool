using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maptz.SpeechToText.Sockets
{
    public static class ServiceCollectionExtensions
    {
        /* #region Public Static Methods */
        public static IServiceCollection AddBuiltInSocketAdapter(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISocketAdapter, BuiltInSocketAdapter>();
            return serviceCollection;
        }
        /* #endregion Public Static Methods */
    }
}
