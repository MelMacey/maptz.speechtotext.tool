using Newtonsoft.Json;
namespace Maptz.SpeechToText.Bing.Client
{

    public class SpeechFragmentMessage : MessageBase
    {
        public string Text { get; set; }
        public long Offset { get; set; }
        public long Duration { get; set; }
    }
}