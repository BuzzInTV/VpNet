using System;

namespace VpNet.EventData
{
    /// <summary>
    ///     Provides event arguments for <see cref="VirtualParadiseClient.InviteRequestReceived" />.
    /// </summary>
    public sealed class InviteRequestReceivedEventArgs : EventArgs
    {
        /// <inheritdoc />
        public InviteRequestReceivedEventArgs(InviteRequest inviteRequest)
        {
            InviteRequest = inviteRequest;
        }

        /// <summary>
        ///     Gets the invite request.
        /// </summary>
        /// <value>The invite request.</value>
        public InviteRequest InviteRequest { get; }
    }
}
