using System;
using VpNet.Entities;

namespace VpNet.EventData
{
    /// <summary>
    ///     Provides event arguments for <see cref="VirtualParadiseClient.MessageReceived" />.
    /// </summary>
    public sealed class MessageReceivedEventArgs : EventArgs
    {
        /// <inheritdoc />
        public MessageReceivedEventArgs(VirtualParadiseMessage message)
        {
            Message = message;
        }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public VirtualParadiseMessage Message { get; }
    }
}
