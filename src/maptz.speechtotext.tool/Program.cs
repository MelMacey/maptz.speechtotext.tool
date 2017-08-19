using Maptz.SpeechToText.Bing.Client;
using Maptz.SpeechToText.Sockets;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Maptz.SpeechToText.Tool
{

    class Program
    {
        static void Main(string[] args)
        {
            /* #region Setup */
            CommandLineApplication cla = new CommandLineApplication(throwOnUnexpectedArg: false);
            var helpOption = cla.HelpOption("-?|-h|--help");
            cla.Description = "Converts a WAV file to text.";
            /* #endregion*/


            /* #region Perform the default execution */
            cla.OnExecute(() =>
            {
                Console.WriteLine(cla.Description);
                cla.ShowHelp();
                return 0;
            });
            /* #endregion*/


            /* #region convert */
            cla.Command("convert", c =>
            {
                var inputFileOption = c.Option("-i|--input <filePath>", "Input file path ", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {
                    var inputFilePath = inputFileOption.Value();
                    var startup = new Startup();
                    startup.Convert(inputFilePath).Wait();
                    return 0;
                });
            });
            /* #endregion*/


            /* #region Run the CommandLineApplication */
            try
            {
                cla.Execute(args);
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                var setColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = setColor;
            }
            /* #endregion*/
        }
    }
}
