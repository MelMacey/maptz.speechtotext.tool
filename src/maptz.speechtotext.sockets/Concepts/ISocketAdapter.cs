using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Maptz.SpeechToText.Sockets
{
    public interface ISocketAdapter
    {
        /* #region Public Events */
        event EventHandler<BinaryMessageReceivedEventArgs> BinaryMessageReceived;
        event EventHandler<EventArgs> Closed;
        event EventHandler<TextMessageReceivedEventArgs> TextMessageReceived;
        /* #endregion Public Delegates */
        /* #region Public Methods */
        Task Connect(string uri, IEnumerable<KeyValuePair<string, string>> headers);
        Task SendBinary(ArraySegment<byte> buffer);
        Task SendText(ArraySegment<byte> buffer);
        /* #endregion Public Methods */
    }
}