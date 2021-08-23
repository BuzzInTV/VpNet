﻿using VpNet.Entities;

namespace VpNet
{
    /// <summary>
    ///     An enumeration of URI targets.
    /// </summary>
    /// <remarks>
    ///     When used with <see cref="VirtualParadiseAvatar.SendUriAsync" />, <see cref="Overlay" /> indicates that that the URI
    ///     will be displayed as a 2D overlay over the 3D world view. This currently uses CEF (Chromium Embedded Framework).
    /// </remarks>
    public enum UriTarget
    {
        /// <summary>
        ///     Opens the URI in an external browser.
        /// </summary>
        Browser,

        /// <summary>
        ///     Opens the URI in an internal browser.
        /// </summary>
        Overlay
    }
}