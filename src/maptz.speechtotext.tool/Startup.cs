using Maptz.SpeechToText.Bing.Client;
using Maptz.SpeechToText.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Maptz.SpeechToText.Tool
{

    public class AppSettings
    {
        public string BingSpeechToTextKey { get; set; }
    }

    public class Startup
    {
        /* #region Private Methods */
        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddBingSpeechToTextService(options: new BingSpeechToTextServiceOptions()
            {
                AuthenticationKey = this.AppSettings.BingSpeechToTextKey
            });
            serviceCollection.AddBuiltInSocketAdapter();
        }
        /* #endregion Private Methods */
        /* #region Public Properties */
        public ServiceProvider ServiceProvider { get; }
        public IConfigurationRoot Configuration { get; }
        public AppSettings AppSettings { get; }

        /* #endregion Public Properties */
        /* #region Public Constructors */
        public Startup()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets("Maptz.SpeechToText.Tool");
            this.Configuration = builder.Build();

            this.AppSettings = new AppSettings();
            this.Configuration.Bind(this.AppSettings);

            var serviceCollection = new ServiceCollection();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            serviceCollection.AddTransient<ILoggerFactory>(sp => loggerFactory);
            serviceCollection.AddOptions();

            this.ConfigureServices(serviceCollection);

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
        /* #endregion Public Constructors */
        /* #region Public Methods */
        public async Task Convert(string audioFilePath)
        {
            var speechToTextService = this.ServiceProvider.GetService<ISpeechToTextService>();
            var speechResult = await speechToTextService.Convert(audioFilePath);

        }
        /* #endregion Public Methods */
    }
}