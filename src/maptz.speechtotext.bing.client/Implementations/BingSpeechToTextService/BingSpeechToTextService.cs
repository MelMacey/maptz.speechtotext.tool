using Maptz.SpeechToText.Sockets;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Maptz.SpeechToText.Bing.Client
{


    public class BingSpeechToTextSocketService : ISpeechToTextService
    {
        /* #region Public Static Methods */
        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
        /* #endregion Public Static Methods */
        /* #region Private Properties */
        private ConcurrentDictionary<long, BingSocketTextMessage> ReceivedMessages { get; set; }
        /* #endregion Private Properties */
        /* #region Private Methods */
        private string GetSpeechConfigJson()
        {
            var speechConfig = new SpeechConfigMessage();
            var speechConfigJson = JsonConvert.SerializeObject(speechConfig, Formatting.None, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            return speechConfigJson;
        }
        private void OnBingSocketTextMessage(BingSocketTextMessage bingSocketTextMessage)
        {
            //Console.WriteLine("Received: " + bingSocketTextMessage.Path);
            this.ReceivedMessages.GetOrAdd(this.ReceivedMessages.Count, bingSocketTextMessage);

            try
            {
                var message = bingSocketTextMessage.AsMessage();
                if (message is SpeechPhraseMessage)
                {
                    var spm = message as SpeechPhraseMessage;
                    //Console.WriteLine("Phrase: " + spm.DisplayText);
                    Console.WriteLine("speech.phrase");
                    Console.WriteLine("\tRecognitionStatus: " + spm.RecognitionStatus);
                    Console.WriteLine("\tOffset: " + TimeSpan.FromSeconds(spm.Offset * 100.0 / 1000000000));
                    if (spm.NBest != null && spm.NBest.Length > 0)
                    {
                        var nbest = spm.NBest.First();
                        Console.WriteLine("\tText: " + nbest.Display);
                        Console.WriteLine("\tConfidence: " + nbest.Confidence);
                    }

                }
                if (message is TurnEndMessage)
                {
                    var spm = message as TurnEndMessage;
                    Console.WriteLine("Turn end");
                }
                if (message is SpeechHypothesisMessage)
                {
                    var spm = message as SpeechHypothesisMessage;
                    Console.WriteLine("Hypothesis:");
                    Console.WriteLine("\tText: " + spm.Text);
                    Console.WriteLine("\tOffset: " + spm.Offset);
                    Console.WriteLine("\tDuration: " + spm.Offset);
                }
                if (message is SpeechFragmentMessage)
                {
                    var spm = message as SpeechFragmentMessage;
                    Console.WriteLine("Fragment:");
                    Console.WriteLine("\tText: " + spm.Text);
                    Console.WriteLine("\tOffset: " + spm.Offset);
                    Console.WriteLine("\tDuration: " + spm.Offset);
                }
                if (message is SpeechEndDetectedMessage)
                {
                    var spm = message as SpeechEndDetectedMessage;
                    Console.WriteLine("End detected: " + spm.Offset);
                }
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                throw ex;
            }


        }
        private async Task SendAudioParts(string requestId)
        {
            Console.WriteLine($"Sending data from file");
            var currentChunk = this.RiffChunker.Next();
            while (currentChunk != null)
            {
                var cursor = 0;
                while (cursor < currentChunk.SubChunkDataBytes.Length)
                {

                    /* #region Prepare header */
                    var outputBuilder = new StringBuilder();
                    outputBuilder.Append("path:audio\r\n");
                    outputBuilder.Append($"x-requestid:{requestId}\r\n");
                    outputBuilder.Append($"x-timestamp:{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK")}\r\n");
                    outputBuilder.Append($"content-type:audio/x-wav\r\n");

                    var headerBytes = Encoding.ASCII.GetBytes(outputBuilder.ToString());
                    var headerbuffer = new ArraySegment<byte>(headerBytes, 0, headerBytes.Length);
                    var str = "0x" + (headerBytes.Length).ToString("X");
                    var headerHeadBytes = BitConverter.GetBytes((UInt16)headerBytes.Length);
                    var isBigEndian = !BitConverter.IsLittleEndian;
                    var headerHead = !isBigEndian ? new byte[] { headerHeadBytes[1], headerHeadBytes[0] } : new byte[] { headerHeadBytes[0], headerHeadBytes[1] };
                    /* #endregion*/

                    var length = Math.Min(4096 * 2 - headerBytes.Length - 8, currentChunk.AllBytes.Length - cursor); //8bytes for the chunk header

                    var chunkHeader = Encoding.ASCII.GetBytes("data").Concat(BitConverter.GetBytes((UInt32)length)).ToArray();

                    byte[] dataArray = new byte[length];
                    Array.Copy(currentChunk.AllBytes, cursor, dataArray, 0, length);
                    //Console.WriteLine($"Sending data from cursor: {cursor}");

                    cursor += length;

                    var arr = headerHead.Concat(headerBytes).Concat(chunkHeader).Concat(dataArray).ToArray();
                    var arrSeg = new ArraySegment<byte>(arr, 0, arr.Length);

                    await this.SocketAdapter.SendBinary(arrSeg);
                }
                //Move to the next RIFF chunk if there is one. 
                currentChunk = this.RiffChunker.Next();
            }
            /* #region Send Audio End  */
            {
                /* #region Prepare header */
                var outputBuilder = new StringBuilder();
                outputBuilder.Append("path:audio\r\n");
                outputBuilder.Append($"x-requestid:{requestId}\r\n");
                outputBuilder.Append($"x-timestamp:{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK")}\r\n");
                outputBuilder.Append($"content-type:audio/x-wav\r\n");

                var headerBytes = Encoding.ASCII.GetBytes(outputBuilder.ToString());
                var headerbuffer = new ArraySegment<byte>(headerBytes, 0, headerBytes.Length);
                var str = "0x" + (headerBytes.Length).ToString("X");
                var headerHeadBytes = BitConverter.GetBytes((UInt16)headerBytes.Length);
                var isBigEndian = !BitConverter.IsLittleEndian;
                var headerHead = !isBigEndian ? new byte[] { headerHeadBytes[1], headerHeadBytes[0] } : new byte[] { headerHeadBytes[0], headerHeadBytes[1] };
                /* #endregion*/

                var arr = headerHead.Concat(headerBytes).ToArray();
                var arrSeg = new ArraySegment<byte>(arr, 0, arr.Length);

                await this.SocketAdapter.SendBinary(arrSeg);
            }
            /* #endregion*/
            Console.WriteLine($"Finished sending data");
        }
        private async Task SendFirstAudioPart(string requestId)
        {

            /* #region Prepare header */
            var outputBuilder = new StringBuilder();
            outputBuilder.Append("path:audio\r\n");
            outputBuilder.Append($"x-requestid:{requestId}\r\n");
            outputBuilder.Append($"x-timestamp:{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK")}\r\n");
            outputBuilder.Append($"content-type:audio/x-wav\r\n");

            var headerBytes = Encoding.ASCII.GetBytes(outputBuilder.ToString());
            var headerbuffer = new ArraySegment<byte>(headerBytes, 0, headerBytes.Length);
            var str = "0x" + (headerBytes.Length).ToString("X");
            var headerHeadBytes = BitConverter.GetBytes((UInt16)headerBytes.Length);
            var isBigEndian = !BitConverter.IsLittleEndian;
            var headerHead = !isBigEndian ? new byte[] { headerHeadBytes[1], headerHeadBytes[0] } : new byte[] { headerHeadBytes[0], headerHeadBytes[1] };
            /* #endregion*/


            var riffHeaderBytes = this.RiffChunker.RiffHeader.Bytes;
            var arr = headerHead.Concat(headerBytes).Concat(riffHeaderBytes).ToArray();
            var arrSeg = new ArraySegment<byte>(arr, 0, arr.Length);
            await this.SocketAdapter.SendBinary(arrSeg);
        }
        private async Task SendSpeechConfig(string requestId)
        {
            var speechConfigJson = this.GetSpeechConfigJson();
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append("path: speech.config\r\n"); //Should this be \r\n
            outputBuilder.Append($"x-timestamp: {DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK")}\r\n");
            outputBuilder.Append($"x-requestid:{requestId}\r\n");
            outputBuilder.Append($"content-type: application/json; charset=utf-8\r\n");
            outputBuilder.Append("\r\n");
            outputBuilder.Append(speechConfigJson);
            var strh = outputBuilder.ToString();

            var encoded = Encoding.UTF8.GetBytes(strh);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            Console.WriteLine("Sending speech.config");
            await this.SocketAdapter.SendText(buffer);
            Console.WriteLine("Sent.");
        }
        private async Task Wait(double milliseconds)
        {
            {
                var startWait = DateTime.UtcNow;
                while ((DateTime.UtcNow - startWait).TotalMilliseconds < milliseconds)
                {
                    await Task.Delay(1);
                }
            }
        }
        private async Task<TurnEndMessage> WaitForTurnEnd(string requestId)
        {
            var startTime = DateTime.UtcNow;
            while ((DateTime.UtcNow - startTime).TotalSeconds < 120)
            {
                foreach (var p in this.ReceivedMessages.ToArray())
                {
                    var isTurnEnd = string.Equals(p.Value.RequestId, requestId, StringComparison.OrdinalIgnoreCase) && string.Equals(p.Value.Path, "turn.end", StringComparison.OrdinalIgnoreCase);
                    if (isTurnEnd)
                    {
                        this.ReceivedMessages.TryRemove(p.Key, out BingSocketTextMessage value);
                        return (TurnEndMessage)value.AsMessage();
                    }
                }
                await Task.Delay(100);
            }
            throw new TimeoutException($"Timed out waiting for 'turn.end' message for request '{requestId}'");
        }
        private async Task<TurnStartMessage> WaitForTurnStart(string requestId)
        {
            var startTime = DateTime.UtcNow;
            while ((DateTime.UtcNow - startTime).TotalSeconds < 30)
            {
                foreach (var p in this.ReceivedMessages.ToArray())
                {
                    var isTurnStart = string.Equals(p.Value.RequestId, requestId, StringComparison.OrdinalIgnoreCase) && string.Equals(p.Value.Path, "turn.start", StringComparison.OrdinalIgnoreCase);
                    if (isTurnStart)
                    {
                        this.ReceivedMessages.TryRemove(p.Key, out BingSocketTextMessage value);
                        return (TurnStartMessage)value.AsMessage();
                    }
                }
                await Task.Delay(100);
            }
            throw new TimeoutException($"Timed out waiting for 'turn.start' message for request '{requestId}'");

        }
        /* #endregion Private Methods */
        /* #region Public Properties */
        public bool IsConverting { get; private set; }
        public BingSpeechToTextServiceOptions Options { get; private set; }
        public RiffChunker RiffChunker { get; private set; }
        public ISocketAdapter SocketAdapter { get; }
        /* #endregion Public Properties */
        /* #region Public Constructors */
        public BingSpeechToTextSocketService(ISocketAdapter socketAdapter, IOptions<BingSpeechToTextServiceOptions> options)
        {
            this.Options = options.Value;

            this.SocketAdapter = socketAdapter;
            this.SocketAdapter.TextMessageReceived += (s, e) =>
            {
                try
                {
                    var bingSocketTextMessage = new BingSocketTextMessage(e.Message);
                    this.OnBingSocketTextMessage(bingSocketTextMessage);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            };
            this.SocketAdapter.Closed += (s, e) =>
            {

            };


            this.ReceivedMessages = new ConcurrentDictionary<long, BingSocketTextMessage>();
        }
        /* #endregion Public Constructors */
        /* #region Interface: 'Maptz.SpeechToText.Bing.Client.ISpeechToTextService' Methods */
        public async Task<IEnumerable<SpeechResult>> Convert(string riffFilePath)
        {
            //See here for implementation details: 
            //  https://docs.microsoft.com/en-us/azure/cognitive-services/speech/api-reference-rest/websocketprotocol

            /* #region Validate State */
            if (this.IsConverting) throw new InvalidOperationException();
            /* #endregion*/

            /* #region Initialize the state */
            this.IsConverting = true;

            var riffFileInfo = new FileInfo(riffFilePath);
            this.RiffChunker = new RiffChunker(riffFileInfo.FullName);
            /* #endregion*/

            /* #region Get Authentication Token */
            var authenticationKey = new BingSocketAuthentication(this.Options.AuthenticationKey);
            var token = authenticationKey.GetAccessToken();
            /* #endregion*/

            /* #region Connect */
            var connectionId = Guid.NewGuid().ToString("N");
            var lang = this.Options.LanguageCode;
            var url = $"wss://speech.platform.bing.com/speech/recognition/dictation/cognitiveservices/v1?format=detailed&language={lang}";  //See https://docs.microsoft.com/en-gb/azure/cognitive-services/speech/api-reference-rest/bingvoicerecognition#endpoints
            var headers = new Dictionary<string, string>();
            headers.Add("X-ConnectionId", connectionId);
            headers.Add("Authorization", "Bearer " + token);
            await this.SocketAdapter.Connect(url, headers);
            Console.WriteLine("Connected.");
            /* #endregion*/

            /* #region Perform conversion */
            var requestId = Guid.NewGuid().ToString("N");
            await this.SendSpeechConfig(requestId);
            await this.SendFirstAudioPart(requestId);
            await this.WaitForTurnStart(requestId);
            await this.SendAudioParts(requestId);
            await this.WaitForTurnEnd(requestId);
            /* #endregion*/

            /* #region Prepare return value */
            var retval = new List<SpeechResult>();
            var phrases = this.ReceivedMessages.Where(p => p.Value.Path?.ToLower() == "speech.phrase").Select(p => (SpeechPhraseMessage)p.Value.AsMessage());
            foreach (var phrase in phrases)
            {
                retval.Add(new SpeechResult()
                {
                    OffsetMs = phrase.Offset,
                    Text = phrase.NBest?.FirstOrDefault()?.Display
                });
            }
            /* #endregion*/

            /* #region Clean-up */
            this.RiffChunker.Dispose();
            this.RiffChunker = null;
            this.IsConverting = false;
            this.ReceivedMessages.Clear();
            /* #endregion*/

            return retval;
        }
        /* #endregion Interface: 'Maptz.SpeechToText.Bing.Client.ISpeechToTextService' Methods */
    }
}