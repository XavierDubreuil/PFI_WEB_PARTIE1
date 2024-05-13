using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class Message
    {

            public int Id { get; set; }
            public (int usrEnvoyer, int usrRecu) UsersId { get; set; }
            public string Contenu { get; set; }
            public DateTime Date { get; set; }
            public Message(int userId, int otherUserId)
            {
                UsersId = (userId, otherUserId);
                Date = DateTime.Now;
            }
            public Message() { }
        
    }
}