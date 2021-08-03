﻿using System;
using VpNet.Internal;
using static VpNet.Internal.Native;

namespace VpNet
{
    public sealed partial class VirtualParadiseClient
    {
        private void SetNativeCallbacks()
        {
            // SetNativeCallback(NativeCallback.ObjectAdd, OnObjectAddNativeCallback);
            // SetNativeCallback(NativeCallback.ObjectChange, OnObjectChangeNativeCallback);
            // SetNativeCallback(NativeCallback.ObjectDelete, OnObjectDeleteNativeCallback);
            // SetNativeCallback(NativeCallback.GetFriends, OnGetFriendsNativeCallback);
            // SetNativeCallback(NativeCallback.FriendAdd, OnFriendAddNativeCallback);
            // SetNativeCallback(NativeCallback.FriendDelete, OnFriendDeleteNativeCallback);
            // SetNativeCallback(NativeCallback.TerrainQuery, OnTerrainQueryNativeCallback);
            // SetNativeCallback(NativeCallback.TerrainNodeSet, OnTerrainNodeSetNativeCallback);
            // SetNativeCallback(NativeCallback.ObjectGet, OnObjectGetNativeCallback);
            // SetNativeCallback(NativeCallback.ObjectLoad, OnObjectLoadNativeCallback);
            SetNativeCallback(NativeCallback.Login, OnLoginNativeCallback);
            SetNativeCallback(NativeCallback.Enter, OnEnterNativeCallback);
            // SetNativeCallback(NativeCallback.Join, OnJoinNativeCallback);
            SetNativeCallback(NativeCallback.ConnectUniverse, OnConnectUniverseNativeCallback);
            // SetNativeCallback(NativeCallback.WorldPermissionUserSet, OnWorldPermissionUserSetNativeCallback);
            // SetNativeCallback(NativeCallback.WorldPermissionSessionSet, OnWorldPermissionSessionSetNativeCallback);
            // SetNativeCallback(NativeCallback.WorldSettingSet, OnWorldSettingSetNativeCallback);
            // SetNativeCallback(NativeCallback.Invite, OnInviteNativeCallback);
            SetNativeCallback(NativeCallback.WorldList, OnWorldListNativeCallback);
        }

        private void SetNativeCallback(NativeCallback nativeCallback, NativeCallbackHandler handler)
        {
            _nativeCallbackHandlers.TryAdd(nativeCallback, handler);
            vp_callback_set(NativeInstanceHandle, nativeCallback, handler);
        }

        private void OnLoginNativeCallback(IntPtr sender, ReasonCode reason, int reference)
        {
            _loginCompletionSource.SetResult(reason);
        }

        private void OnEnterNativeCallback(IntPtr sender, ReasonCode reason, int reference)
        {
            _enterCompletionSource.SetResult(reason);
        }

        private void OnConnectUniverseNativeCallback(IntPtr sender, ReasonCode reason, int reference)
        {
            _connectCompletionSource?.SetResult(reason);
        }

        private void OnWorldListNativeCallback(IntPtr sender, ReasonCode reason, int reference)
        {
            _worldListChannel.Writer.Complete();
        }
    }
}