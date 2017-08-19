using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maptz.SpeechToText.Sockets
{

    public class BuiltInSocketAdapter : SocketAdapterBase, ISocketAdapter
    {
        /* #region Private Fields */
        private ClientWebSocket ClientWebSocket;
        /* #endregion Private Fields */
        /* #region Private Methods */
        private async Task Receiving(ClientWebSocket client)
        {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;
            while (true)
            {

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await client.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            var str = reader.ReadToEnd();
                            this.OnTextMessageReceived(str);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        throw new NotSupportedException();
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine($"Closing ... reason {client.CloseStatusDescription}");
                        var description = client.CloseStatusDescription;
                        await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        this.OnClosed();
                        break;
                    }
                }
            }
        }
        /* #endregion Private Methods */
        /* #region Interface: 'Maptz.SpeechToText.Sockets.ISocketAdapter' Methods */
        public async Task Connect(string uri, IEnumerable<KeyValuePair<string, string>> headers)
        {
            this.ClientWebSocket = new ClientWebSocket(); //cws.Options.AddSubProtocol

            foreach (var header in headers)
            {
                ClientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
            }

            Console.WriteLine("Connecting to web socket.");
            await ClientWebSocket.ConnectAsync(new Uri(uri), new CancellationToken());
            //Start Receiving Thread.
            var receiving = Receiving(ClientWebSocket);

        }
        public async Task SendBinary(ArraySegment<byte> buffer)
        {
            if (this.ClientWebSocket.State != WebSocketState.Open) throw new InvalidOperationException();
            await this.ClientWebSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, new CancellationToken());
        }
        public async Task SendText(ArraySegment<byte> buffer)
        {
            if (this.ClientWebSocket.State != WebSocketState.Open) throw new InvalidOperationException();
            await this.ClientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, new CancellationToken());
        }
        /* #endregion Interface: 'Maptz.SpeechToText.Sockets.ISocketAdapter' Methods */
    }
}