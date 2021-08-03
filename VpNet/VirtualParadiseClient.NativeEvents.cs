using System;
using VpNet.Entities;
using VpNet.EventData;
using VpNet.Internal;
using VpNet.NativeApi;
using static VpNet.Internal.Native;

namespace VpNet
{
    public sealed partial class VirtualParadiseClient
    {
        private void SetNativeEvents()
        {
            SetNativeEvent(NativeEvent.Chat, OnChatNativeEvent);
            SetNativeEvent(NativeEvent.AvatarAdd, OnAvatarAddNativeEvent);
            SetNativeEvent(NativeEvent.AvatarChange, OnAvatarChangeNativeEvent);
            SetNativeEvent(NativeEvent.AvatarDelete, OnAvatarDeleteNativeEvent);
            // SetNativeEvent(NativeEvent.Object, OnObjectNativeEvent);
            // SetNativeEvent(NativeEvent.ObjectChange, OnObjectChangeNativeEvent);
            // SetNativeEvent(NativeEvent.ObjectDelete, OnObjectDeleteNativeEvent);
            // SetNativeEvent(NativeEvent.ObjectClick, OnObjectClickNativeEvent);
            SetNativeEvent(NativeEvent.WorldList, OnWorldListNativeEvent);
            // SetNativeEvent(NativeEvent.WorldSetting, OnWorldSettingNativeEvent);
            // SetNativeEvent(NativeEvent.WorldSettingsChanged, OnWorldSettingsChangedNativeEvent);
            // SetNativeEvent(NativeEvent.Friend, OnFriendNativeEvent);
            // SetNativeEvent(NativeEvent.WorldDisconnect, OnWorldDisconnectNativeEvent);
            // SetNativeEvent(NativeEvent.UniverseDisconnect, OnUniverseDisconnectNativeEvent);
            SetNativeEvent(NativeEvent.UserAttributes, OnUserAttributesNativeEvent);
            // SetNativeEvent(NativeEvent.QueryCellEnd, OnQueryCellEndNativeEvent);
            // SetNativeEvent(NativeEvent.TerrainNode, OnTerrainNodeNativeEvent);
            // SetNativeEvent(NativeEvent.AvatarClick, OnAvatarClickNativeEvent);
            // SetNativeEvent(NativeEvent.Teleport, OnTeleportNativeEvent);
            // SetNativeEvent(NativeEvent.Url, OnUrlNativeEvent);
            // SetNativeEvent(NativeEvent.ObjectBumpBegin, OnObjectBumpBeginNativeEvent);
            // SetNativeEvent(NativeEvent.ObjectBumpEnd, OnObjectBumpEndNativeEvent);
            // SetNativeEvent(NativeEvent.TerrainNodeChanged, OnTerrainNodeChangedNativeEvent);
            SetNativeEvent(NativeEvent.Join, OnJoinNativeEvent);
            SetNativeEvent(NativeEvent.Invite, OnInviteNativeEvent);
        }

        private void SetNativeEvent(NativeEvent nativeEvent, NativeEventHandler handler)
        {
            _nativeEventHandlers.TryAdd(nativeEvent, handler);
            vp_event_set(NativeInstanceHandle, nativeEvent, handler);
        }

        private void OnChatNativeEvent(IntPtr sender)
        {
            throw new NotImplementedException();
        }

        private void OnAvatarAddNativeEvent(IntPtr sender)
        {
            VirtualParadiseAvatar avatar = ExtractAvatar(sender);
            avatar = AddOrUpdateAvatar(avatar);
            var args = new AvatarJoinedEventArgs(avatar);
            RaiseEvent(AvatarJoined, args);
        }

        private void OnAvatarChangeNativeEvent(IntPtr sender)
        {
            int session;
            int type;
            Vector3d position, rotation;

            lock (Lock)
            {
                session = vp_int(sender, IntegerAttribute.AvatarSession);
                type = vp_int(sender, IntegerAttribute.AvatarType);

                double x = vp_double(sender, FloatAttribute.AvatarX);
                double y = vp_double(sender, FloatAttribute.AvatarY);
                double z = vp_double(sender, FloatAttribute.AvatarZ);
                position = new Vector3d(x, y, z);

                double pitch = vp_double(sender, FloatAttribute.AvatarPitch);
                double yaw = vp_double(sender, FloatAttribute.AvatarYaw);
                rotation = new Vector3d(pitch, yaw, 0);
            }

            var avatar = GetAvatar(session);
            if (type != avatar.Type)
            {
                int oldType = avatar.Type;
                avatar.Type = type;

                var args = new AvatarTypeChangedEventArgs(avatar, type, oldType);
                RaiseEvent(AvatarTypeChanged, args);
            }

            var oldLocation = avatar.Location;
            var newLocation = new Location(oldLocation.World, position, rotation);
            avatar.Location = newLocation;

            if (position != oldLocation.Position || rotation != oldLocation.Rotation)
            {
                var args = new AvatarMovedEventArgs(avatar, newLocation, oldLocation);
                RaiseEvent(AvatarMoved, args);
            }
        }

        private void OnAvatarDeleteNativeEvent(IntPtr sender)
        {
            int session;

            lock (Lock)
            {
                session = vp_int(sender, IntegerAttribute.AvatarSession);
            }

            var avatar = GetAvatar(session);
            _avatars.TryRemove(session, out var _);

            var args = new AvatarLeftEventArgs(avatar);
            RaiseEvent(AvatarLeft, args);
        }

        private async void OnWorldListNativeEvent(IntPtr sender)
        {
            VirtualParadiseWorld world;

            lock (Lock)
            {
                string name = vp_string(sender, StringAttribute.WorldName);
                int avatarCount = vp_int(sender, IntegerAttribute.WorldUsers);
                var state = (WorldState) vp_int(sender, IntegerAttribute.WorldState);

                world = new VirtualParadiseWorld(name)
                {
                    AvatarCount = avatarCount,
                    State = state
                };
            }

            if (_worldListChannel is not null)
                await _worldListChannel.Writer.WriteAsync(world);
        }

        private void OnUserAttributesNativeEvent(IntPtr sender)
        {
            int userId;
            VirtualParadiseUser user;

            lock (Lock)
            {
                userId = vp_int(sender, IntegerAttribute.UserId);
                string name = vp_string(sender, StringAttribute.UserName);
                string email = vp_string(sender, StringAttribute.UserEmail);

                int lastLogin = vp_int(sender, IntegerAttribute.UserLastLogin);
                int onlineTime = vp_int(sender, IntegerAttribute.UserOnlineTime);
                int registered = vp_int(sender, IntegerAttribute.UserRegistrationTime);

                user = new VirtualParadiseUser(this, userId)
                {
                    Name = name,
                    EmailAddress = email,
                    LastLogin = DateTimeOffset.FromUnixTimeSeconds(lastLogin),
                    OnlineTime = TimeSpan.FromSeconds(onlineTime),
                    RegistrationTime = DateTimeOffset.FromUnixTimeSeconds(registered)
                };
            }

            if (_usersCompletionSources.TryGetValue(userId, out var taskCompletionSource))
                taskCompletionSource.SetResult(user);
        }

        private async void OnJoinNativeEvent(IntPtr sender)
        {
            int requestId;
            int userId;
            string name;

            lock (Lock)
            {
                requestId = vp_int(NativeInstanceHandle, IntegerAttribute.JoinId);
                userId = vp_int(NativeInstanceHandle, IntegerAttribute.UserId);
                name = vp_string(NativeInstanceHandle, StringAttribute.JoinName);
            }

            var user = await GetUserAsync(userId);
            var joinRequest = new JoinRequest(this, requestId, name, user);
            var args = new JoinRequestReceivedEventArgs(joinRequest);
            RaiseEvent(JoinRequestReceived, args);
        }

        private async void OnInviteNativeEvent(IntPtr sender)
        {
            Vector3d position;
            Vector3d rotation;
            int requestId;
            int userId;
            string worldName;
            string avatarName;

            lock (Lock)
            {
                requestId = vp_int(sender, IntegerAttribute.InviteId);
                userId = vp_int(sender, IntegerAttribute.InviteUserId);
                avatarName = vp_string(sender, StringAttribute.InviteName);

                double x = vp_double(sender, FloatAttribute.InviteX);
                double y = vp_double(sender, FloatAttribute.InviteY);
                double z = vp_double(sender, FloatAttribute.InviteZ);

                double yaw = vp_double(sender, FloatAttribute.InviteYaw);
                double pitch = vp_double(sender, FloatAttribute.InvitePitch);

                position = new Vector3d(x, y, z);
                rotation = new Vector3d(yaw, pitch, 0);

                worldName = vp_string(sender, StringAttribute.InviteWorld);
            }

            var world = await GetWorldAsync(worldName);
            var user = await GetUserAsync(userId);

            var location = new Location(world, position, rotation);
            var request = new InviteRequest(this, requestId, avatarName, user, location);
            var args = new InviteRequestReceivedEventArgs(request);
            RaiseEvent(InviteRequestReceived, args);
        }
    }
}
