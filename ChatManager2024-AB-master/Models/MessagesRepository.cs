using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class MessagesRepository : Repository<Message>
    {
        public IEnumerable<Message> GetConv(int loggedUsrID, int targetUsrId)
        {

            var liste = ToList().Where(m => (m.usrSource == loggedUsrID && m.usrTarget == targetUsrId) || (m.usrTarget == loggedUsrID && m.usrSource == targetUsrId));
            liste.OrderBy(m => m.Date);
            return liste;
        }
    }
}