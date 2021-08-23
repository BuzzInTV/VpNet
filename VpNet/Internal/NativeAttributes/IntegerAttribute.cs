﻿using System;

namespace VpNet.Internal.NativeAttributes
{
    internal enum IntegerAttribute
    {
        AvatarSession,
        AvatarType,
        MyType,
        ObjectId,
        ObjectType,
        ObjectTime,
        ObjectUserId,
        WorldState,
        WorldUsers,
        ReferenceNumber,
        Callback,
        UserId,
        UserRegistrationTime,
        UserOnlineTime,
        UserLastLogin,
        [Obsolete] FriendId,
        FriendUserId,
        FriendOnline,
        MyUserId,
        ProxyType,
        ProxyPort,
        CellX,
        CellZ,
        TerrainTileX,
        TerrainTileZ,
        TerrainNodeX,
        TerrainNodeZ,
        TerrainNodeRevision,
        ClickedSession,
        ChatType,
        ChatRolorRed,
        ChatColorGreen,
        ChatColorBlue,
        ChatEffects,
        DisconnectErrorCode,
        UrlTarget,
        CurrentEvent,
        CurrentCallback,
        CellRevision,
        CellStatus,
        JoinId,
        InviteId,
        InviteUserId,
        WorldSize
    }
}