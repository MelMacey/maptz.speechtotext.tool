using Newtonsoft.Json;
namespace Maptz.SpeechToText.Bing.Client
{
    public class SpeechStartDetectedMessage : MessageBase
    {
        /// <summary>
        /// Offset in 100 nanosecond units
        /// </summary>
        public int Offset { get; set; }
    }
}