using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public enum EnumFriendshipStatus { 
        Pending, 
        Accepted, 
        Declined 
    }
    
    public class Friendship
    {
        public int Id { get; set; }
        public int SourceUserId { get; set; }
        public int TargetUserId { get; set; }
        public DateTime CreationDate { get; set; }
        public EnumFriendshipStatus FriendshipStatus { get; set; }
    }
}