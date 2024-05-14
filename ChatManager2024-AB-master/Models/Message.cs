using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class Message
    {

        public int Id { get; set; }
        public int usrSource { get; set; }
        public int usrTarget { get; set; }
        public string content { get; set; }
        public DateTime Date { get; set; }
        public Message(int userId, int otherUserId)
        {
            usrSource = userId;
            usrTarget = otherUserId;
            Date = DateTime.Now;
        }
        public Message() { }

    }
}