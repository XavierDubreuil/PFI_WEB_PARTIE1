using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public enum EnumRelationStatus
    {
        NotFriend,
        Friend,
        Request_Sender,
        Request_Receiver,
        Have_Decline,
        Have_Been_Declined,
        Blocked
    }

    public class FriendshipsRepository : Repository<Friendship>
    {
        public Friendship Add_FiendshipRequest(int sourceUserId, int targetUserId)
        {
            Remove_Friendship(sourceUserId, targetUserId, false);
            User sourceUser = DB.Users.Get(sourceUserId);
            User targetUser = DB.Users.Get(targetUserId);
            if (sourceUser != null && targetUser != null && !targetUser.Blocked)
            {
                Friendship friendship = new Friendship()
                {
                    SourceUserId = sourceUser.Id,
                    TargetUserId = targetUser.Id,
                    FriendshipStatus = EnumFriendshipStatus.Pending,
                    CreationDate = DateTime.Now
                };
                friendship.Id = base.Add(friendship);
                OnlineUsers.AddNotification(targetUserId, $"Vous avez reçu une demande d'amitié de {sourceUser.GetFullName()}");
                return friendship;
            }
            return null;
        }
        public bool AreFriends(int userId_X, int userId_Y)
        {
            User user_X = DB.Users.Get(userId_X);
            if (user_X != null)
            {
                if (user_X.Blocked)
                    return false;
            }
            else
                return false; 
            
            User user_Y = DB.Users.Get(userId_Y);
            if (user_Y != null)
            {
                if (user_Y.Blocked)
                    return false;
            }
            else
                return false;

            Friendship friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == userId_X && f.TargetUserId == userId_Y)).FirstOrDefault();
            if (friendship != null)
            {
                return friendship.FriendshipStatus == EnumFriendshipStatus.Accepted;
            }
            friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == userId_Y && f.TargetUserId == userId_X)).FirstOrDefault();
            if (friendship != null)
            {
                return friendship.FriendshipStatus == EnumFriendshipStatus.Accepted;
            }
            return false;
        }
        public bool HaveFriendshipRelation(int userId_X, int userid_Y)
        {
            Friendship friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == userId_X && f.TargetUserId == userid_Y)).FirstOrDefault();
            if (friendship != null) return true;
            else
            {
                friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == userid_Y && f.TargetUserId == userId_X)).FirstOrDefault();
                if (friendship != null) return true;
            }
            return false;
        }
        
        public bool Remove_FriendshipRequest(int userId_X, int userId_Y)
        {
            User user_X = DB.Users.Get(userId_X);
            User user_Y = DB.Users.Get(userId_Y);
            if (user_X != null && user_Y != null)
            {
                Friendship friendship = DB.Friendships.ToList().Where(f => f.SourceUserId == userId_X && f.TargetUserId == userId_Y).FirstOrDefault();
                if (friendship != null)
                {
                    DB.Friendships.Delete(friendship.Id);
                }
                else
                {
                    friendship = DB.Friendships.ToList().Where(f => f.SourceUserId == userId_Y && f.TargetUserId == userId_X).FirstOrDefault();
                    if (friendship != null)
                    {
                        DB.Friendships.Delete(friendship.Id);
                    }
                }
                OnlineUsers.AddNotification(userId_Y, $"{user_X.GetFullName()} a retiré sa demande amitié");
            }
            return true;
        }
        public bool Remove_Friendship(int userId_X, int userId_Y, bool notify = true)
        {
            User user_X = DB.Users.Get(userId_X);
            User user_Y = DB.Users.Get(userId_Y);
            if (user_X != null && user_Y != null)
            {
                Friendship friendship = DB.Friendships.ToList().Where(f => f.SourceUserId == userId_X && f.TargetUserId == userId_Y).FirstOrDefault();
                if (friendship != null)
                {
                    DB.Friendships.Delete(friendship.Id);
                }
                else
                {
                    friendship = DB.Friendships.ToList().Where(f => f.SourceUserId == userId_Y && f.TargetUserId == userId_X).FirstOrDefault();
                    if (friendship != null)
                    {
                        DB.Friendships.Delete(friendship.Id);
                    }
                }
                if (notify)
                    OnlineUsers.AddNotification(userId_Y, $"{user_X.GetFullName()} a retiré de votre amitié");
            }
            return true;
        }
        public bool Accept_Friendship(int targetUserId, int sourceUserId)
        {
            Friendship friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == targetUserId && f.TargetUserId == sourceUserId)).FirstOrDefault();
            if (friendship != null)
            {
                friendship.FriendshipStatus = EnumFriendshipStatus.Accepted;
                User sourceUser = DB.Users.Get(sourceUserId);
                User targetUser = DB.Users.Get(targetUserId);
                OnlineUsers.AddNotification(targetUserId, $"{sourceUser.GetFullName()} a accepté votre demande d'amitié");
                return base.Update(friendship);
            }
            return false;
        }
        public bool Decline_Friendship(int targetUserId, int sourceUserId)
        {
            Friendship friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == targetUserId && f.TargetUserId == sourceUserId)).FirstOrDefault();
            if (friendship != null)
            {
                friendship.FriendshipStatus = EnumFriendshipStatus.Declined;
                User sourceUser = DB.Users.Get(sourceUserId);
                User targetUser = DB.Users.Get(targetUserId);
                OnlineUsers.AddNotification(targetUserId, $"{sourceUser.GetFullName()} a décliné votre demande d'amitié");
                return base.Update(friendship);
            }
            return false;
        }
        public bool FriendshipAccepted(int sourceUserId, int targetUserId)
        {
            User targetUser = DB.Users.Get(targetUserId);
            if (targetUser != null)
            {
                if (targetUser.Blocked)
                    return false;
            }
            else
                return false;
            User sourceUser = DB.Users.Get(sourceUserId);
            if (sourceUser != null)
            {
                if (sourceUser.Blocked)
                    return false;
            }
            else
                return false;

            Friendship friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == sourceUserId && f.TargetUserId == targetUserId)).FirstOrDefault();
            if (friendship != null)
            {
                return friendship.FriendshipStatus == EnumFriendshipStatus.Accepted;
            }
            friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == targetUserId && f.TargetUserId == sourceUserId)).FirstOrDefault();
            if (friendship != null)
            {
                return friendship.FriendshipStatus == EnumFriendshipStatus.Accepted;
            }
            return false;
        }
        public bool FriendshipDeclined(int sourceUserId, int targetUserId)
        {
            Friendship friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == sourceUserId && f.TargetUserId == targetUserId)).FirstOrDefault();
            if (friendship != null)
            {
                return friendship.FriendshipStatus == EnumFriendshipStatus.Declined;
            }
            friendship = DB.Friendships.ToList().Where(f => (f.SourceUserId == targetUserId && f.TargetUserId == sourceUserId)).FirstOrDefault();
            if (friendship != null)
            {
                return friendship.FriendshipStatus == EnumFriendshipStatus.Declined;
            }
            return false;
        }
        public EnumRelationStatus RelationStatus(int sourceUserId, int targetUserId)
        {
            User targetUser = DB.Users.Get(targetUserId);
            if (targetUser != null)
            {
                if (targetUser.Blocked)
                    return EnumRelationStatus.Blocked;
            }
            Friendship friendShipOfSourceUser = DB.Friendships.ToList().Where(f => (f.SourceUserId == sourceUserId && f.TargetUserId == targetUserId)).FirstOrDefault();
            if (friendShipOfSourceUser != null)
            {
                if (friendShipOfSourceUser.FriendshipStatus == EnumFriendshipStatus.Accepted)
                    return EnumRelationStatus.Friend;
                if (friendShipOfSourceUser.FriendshipStatus == EnumFriendshipStatus.Declined)
                    return EnumRelationStatus.Have_Been_Declined;
                return EnumRelationStatus.Request_Sender;
            }

            Friendship friendShipOfTargetUser = DB.Friendships.ToList().Where(f => (f.SourceUserId == targetUserId && f.TargetUserId == sourceUserId)).FirstOrDefault();
            if (friendShipOfTargetUser != null)
            {
                if (friendShipOfTargetUser.FriendshipStatus == EnumFriendshipStatus.Accepted)
                    return EnumRelationStatus.Friend;
                if (friendShipOfTargetUser.FriendshipStatus == EnumFriendshipStatus.Declined)
                    return EnumRelationStatus.Have_Decline;
                return EnumRelationStatus.Request_Receiver;
            }
            return EnumRelationStatus.NotFriend;
        }
    }

}