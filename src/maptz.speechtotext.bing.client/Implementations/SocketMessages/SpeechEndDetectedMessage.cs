using Newtonsoft.Json;
namespace Maptz.SpeechToText.Bing.Client
{

    public class SpeechEndDetectedMessage : MessageBase
    {
        public long Offset { get; set; }
    }
}