using Newtonsoft.Json;
namespace Maptz.SpeechToText.Bing.Client
{



    public class TurnStartMessage : MessageBase
    {
        public class Context
        {
            public string serviceTag { get; set; }
        }

        public Context context { get; set; }
    }
}