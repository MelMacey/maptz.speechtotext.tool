using System;
namespace Maptz.SpeechToText.Sockets
{

    public class SocketAdapterBase
    {
        /* #region Protected Methods */
        protected virtual void OnBinaryMessageReceived(byte[] buffer)
        {
            var bmr = this.BinaryMessageReceived;
            if (bmr != null) bmr(this, new BinaryMessageReceivedEventArgs(buffer));
        }
        protected virtual void OnClosed()
        {
            var closed = this.Closed;
            if (closed != null) closed(this, new EventArgs());
        }
        protected virtual void OnTextMessageReceived(string str)
        {
            var tmr = this.TextMessageReceived;
            if (tmr != null) tmr(this, new TextMessageReceivedEventArgs(str));
        }
        /* #endregion Protected Methods */
        /* #region Public Delegates */
        public event EventHandler<BinaryMessageReceivedEventArgs> BinaryMessageReceived;
        public event EventHandler<EventArgs> Closed;
        public event EventHandler<TextMessageReceivedEventArgs> TextMessageReceived;
        /* #endregion Public Delegates */
    }
}