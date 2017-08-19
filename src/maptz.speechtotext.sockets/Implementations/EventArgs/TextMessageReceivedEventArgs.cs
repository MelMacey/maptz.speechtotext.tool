using System;
namespace Maptz.SpeechToText.Sockets
{

    public class TextMessageReceivedEventArgs : EventArgs
    {
        /* #region Public Properties */
        public string Message { get; set; }
        /* #endregion Public Properties */
        /* #region Public Constructors */
        public TextMessageReceivedEventArgs(string message) { this.Message = message; }
        /* #endregion Public Constructors */
    }
}