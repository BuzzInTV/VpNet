using System;

namespace VpNet.EventData
{
    /// <summary>
    ///     Provides event arguments for <see cref="VirtualParadiseClient.JoinRequestReceived" />.
    /// </summary>
    public sealed class JoinRequestReceivedEventArgs : EventArgs
    {
        /// <inheritdoc />
        public JoinRequestReceivedEventArgs(JoinRequest joinRequest)
        {
            JoinRequest = joinRequest;
        }

        /// <summary>
        ///     Gets the join request.
        /// </summary>
        /// <value>The join request.</value>
        public JoinRequest JoinRequest { get; }
    }
}
