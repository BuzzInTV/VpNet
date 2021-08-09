using System;
using System.Drawing;
using System.Numerics;
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
            SetNativeEvent(NativeEvent.Object, OnObjectNativeEvent);
            // SetNativeEvent(NativeEvent.ObjectChange, OnObjectChangeNativeEvent);
            SetNativeEvent(NativeEvent.ObjectDelete, OnObjectDeleteNativeEvent);
            SetNativeEvent(NativeEvent.ObjectClick, OnObjectClickNativeEvent);
            SetNativeEvent(NativeEvent.WorldList, OnWorldListNativeEvent);
            // SetNativeEvent(NativeEvent.WorldSetting, OnWorldSettingNativeEvent);
            // SetNativeEvent(NativeEvent.WorldSettingsChanged, OnWorldSettingsChangedNativeEvent);
            // SetNativeEvent(NativeEvent.Friend, OnFriendNativeEvent);
            SetNativeEvent(NativeEvent.WorldDisconnect, OnWorldDisconnectNativeEvent);
            SetNativeEvent(NativeEvent.UniverseDisconnect, OnUniverseDisconnectNativeEvent);
            SetNativeEvent(NativeEvent.UserAttributes, OnUserAttributesNativeEvent);
            SetNativeEvent(NativeEvent.QueryCellEnd, OnQueryCellEndNativeEvent);
            // SetNativeEvent(NativeEvent.TerrainNode, OnTerrainNodeNativeEvent);
            SetNativeEvent(NativeEvent.AvatarClick, OnAvatarClickNativeEvent);
            SetNativeEvent(NativeEvent.Teleport, OnTeleportNativeEvent);
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
            VirtualParadiseMessage message;

            lock (Lock)
            {
                int session = vp_int(sender, IntegerAttribute.AvatarSession);
                string name = vp_string(sender, StringAttribute.AvatarName);
                string content = vp_string(sender, StringAttribute.ChatMessage);

                int type = vp_int(sender, IntegerAttribute.ChatType);

                var color = Color.Black;
                var style = FontStyle.Regular;

                if (type == 1)
                {
                    int r = vp_int(sender, IntegerAttribute.ChatRolorRed);
                    int g = vp_int(sender, IntegerAttribute.ChatColorGreen);
                    int b = vp_int(sender, IntegerAttribute.ChatColorBlue);
                    color = Color.FromArgb(r, g, b);
                    style = (FontStyle)vp_int(sender, IntegerAttribute.ChatEffects);
                }

                var avatar = GetAvatar(session);
                message = new VirtualParadiseMessage((MessageType)type, name, content, avatar, style, color);
            }

            var args = new MessageReceivedEventArgs(message);
            RaiseEvent(MessageReceived, args);
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
            Vector3d position;
            Quaternion rotation;

            lock (Lock)
            {
                session = vp_int(sender, IntegerAttribute.AvatarSession);
                type = vp_int(sender, IntegerAttribute.AvatarType);

                double x = vp_double(sender, FloatAttribute.AvatarX);
                double y = vp_double(sender, FloatAttribute.AvatarY);
                double z = vp_double(sender, FloatAttribute.AvatarZ);
                position = new Vector3d(x, y, z);

                var pitch = (float)vp_double(sender, FloatAttribute.AvatarPitch);
                var yaw = (float)vp_double(sender, FloatAttribute.AvatarYaw);
                rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);
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

        private async void OnObjectNativeEvent(IntPtr sender)
        {
            int session;

            lock (Lock)
            {
                session = vp_int(sender, IntegerAttribute.AvatarSession);
            }

            var virtualParadiseObject = await ExtractObjectAsync(sender);
            var cell = virtualParadiseObject.Location.Cell;

            virtualParadiseObject = AddOrUpdateObject(virtualParadiseObject);

            if (session == 0)
            {
                if (_cellChannels.TryGetValue(cell, out var channel))
                    await channel.Writer.WriteAsync(virtualParadiseObject);
            }
            else
            {
                var avatar = GetAvatar(session);
                var args = new ObjectCreatedEventArgs(avatar, virtualParadiseObject);
                RaiseEvent(ObjectCreated, args);
            }
        }

        private async void OnObjectDeleteNativeEvent(IntPtr sender)
        {
            int objectId;
            int session;

            lock (Lock)
            {
                objectId = vp_int(sender, IntegerAttribute.ObjectId);
                session = vp_int(sender, IntegerAttribute.AvatarSession);
            }

            var avatar = GetAvatar(session);
            VirtualParadiseObject virtualParadiseObject;

            try
            {
                virtualParadiseObject = await GetObjectAsync(objectId);
            }
            catch // any exception: we don't care about GetObject failing. ID is always available
            {
                virtualParadiseObject = null;
            }

            _objects.TryRemove(objectId, out var _);

            var args = new ObjectDeletedEventArgs(avatar, objectId, virtualParadiseObject);
            RaiseEvent(ObjectDeleted, args);
        }

        private async void OnObjectClickNativeEvent(IntPtr sender)
        {
            Vector3d clickPoint;
            int objectId;
            int session;

            lock (Lock)
            {
                session = vp_int(sender, IntegerAttribute.AvatarSession);
                objectId = vp_int(sender, IntegerAttribute.ObjectId);

                double x = vp_double(sender, FloatAttribute.ClickHitX);
                double y = vp_double(sender, FloatAttribute.ClickHitY);
                double z = vp_double(sender, FloatAttribute.ClickHitZ);
                clickPoint = new Vector3d(x, y, z);
            }

            var avatar = GetAvatar(session);
            var virtualParadiseObject = await GetObjectAsync(objectId);
            var args = new ObjectClickedEventArgs(avatar, virtualParadiseObject, clickPoint);
            RaiseEvent(ObjectClicked, args);
        }

        private async void OnWorldListNativeEvent(IntPtr sender)
        {
            VirtualParadiseWorld world;

            lock (Lock)
            {
                string name = vp_string(sender, StringAttribute.WorldName);
                int avatarCount = vp_int(sender, IntegerAttribute.WorldUsers);
                var state = (WorldState)vp_int(sender, IntegerAttribute.WorldState);

                world = new VirtualParadiseWorld(name)
                {
                    AvatarCount = avatarCount,
                    State = state
                };
            }

            if (_worldListChannel is not null)
                await _worldListChannel.Writer.WriteAsync(world);
        }

        private void OnUniverseDisconnectNativeEvent(IntPtr sender)
        {
            DisconnectReason reason;
            lock (Lock) reason = (DisconnectReason)vp_int(sender, IntegerAttribute.DisconnectErrorCode);

            var args = new DisconnectedEventArgs(reason);
            RaiseEvent(UniverseServerDisconnected, args);
        }

        private void OnWorldDisconnectNativeEvent(IntPtr sender)
        {
            DisconnectReason reason;
            lock (Lock) reason = (DisconnectReason)vp_int(sender, IntegerAttribute.DisconnectErrorCode);

            var args = new DisconnectedEventArgs(reason);
            RaiseEvent(WorldServerDisconnected, args);
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

        private void OnQueryCellEndNativeEvent(IntPtr sender)
        {
            Cell cell;

            lock (Lock)
            {
                int x = vp_int(sender, IntegerAttribute.CellX);
                int z = vp_int(sender, IntegerAttribute.CellZ);

                cell = new Cell(x, z);
            }

            if (_cellChannels.TryRemove(cell, out var channel))
                channel.Writer.TryComplete();
        }

        private void OnAvatarClickNativeEvent(IntPtr sender)
        {
            int session, clickedSession;
            Vector3d clickPoint;

            lock (Lock)
            {
                session = vp_int(sender, IntegerAttribute.AvatarSession);
                clickedSession = vp_int(sender, IntegerAttribute.ClickedSession);

                double x = vp_double(sender, FloatAttribute.ClickHitX);
                double y = vp_double(sender, FloatAttribute.ClickHitY);
                double z = vp_double(sender, FloatAttribute.ClickHitZ);
                clickPoint = new Vector3d(x, y, z);
            }

            var avatar = GetAvatar(session);
            var clickedAvatar = GetAvatar(clickedSession);
            var args = new AvatarClickedEventArgs(avatar, clickedAvatar, clickPoint);
            RaiseEvent(AvatarClicked, args);
        }

        private async void OnTeleportNativeEvent(IntPtr sender)
        {
            int session;
            string worldName;
            Vector3d position;
            Quaternion rotation;

            lock (Lock)
            {
                session = vp_int(sender, IntegerAttribute.AvatarSession);

                double x = vp_double(sender, FloatAttribute.TeleportX);
                double y = vp_double(sender, FloatAttribute.TeleportY);
                double z = vp_double(sender, FloatAttribute.TeleportZ);
                position = new Vector3d(x, y, z);

                float yaw = vp_float(sender, FloatAttribute.TeleportYaw);
                float pitch = vp_float(sender, FloatAttribute.TeleportPitch);
                rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

                worldName = vp_string(sender, StringAttribute.TeleportWorld);
            }

            var world = string.IsNullOrWhiteSpace(worldName) ? CurrentWorld : await GetWorldAsync(worldName);
            var location = new Location(world, position, rotation);
            CurrentAvatar.Location = location;
            CurrentWorld = world;

            var avatar = GetAvatar(session);
            var args = new TeleportedEventArgs(avatar, location);
            RaiseEvent(Teleported, args);
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
            Quaternion rotation;
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

                var yaw = (float)vp_double(sender, FloatAttribute.InviteYaw);
                var pitch = (float)vp_double(sender, FloatAttribute.InvitePitch);

                position = new Vector3d(x, y, z);
                rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

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
