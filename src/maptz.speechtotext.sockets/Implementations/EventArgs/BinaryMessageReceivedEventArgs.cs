using System;
namespace Maptz.SpeechToText.Sockets
{
    public class BinaryMessageReceivedEventArgs : EventArgs
    {
        /* #region Public Properties */
        public byte[] Bytes { get; set; }
        /* #endregion Public Properties */
        /* #region Public Constructors */
        public BinaryMessageReceivedEventArgs(byte[] bytes) { this.Bytes = bytes; }
        /* #endregion Public Constructors */
    }
}