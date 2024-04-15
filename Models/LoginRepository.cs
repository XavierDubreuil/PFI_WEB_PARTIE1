using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class LoginRepository : Repository<Login>
    {
        public void EndLastOccurence()
        {
            var liste = DB.Logins.ToList();
            User user = OnlineUsers.GetSessionUser();
            if (user != null)
            {
                for (int i = liste.Count - 1; i >= 0; i--)
                {
                    if (liste[i].UserId == user.Id)
                    {
                        liste[i].End = DateTime.Now;
                        DB.Logins.Update(liste[i]);
                        break;
                    }
                }
            }
        }
    }
}