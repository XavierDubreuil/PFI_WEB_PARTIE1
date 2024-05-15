using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class MessagesRepository : Repository<Message>
    {
        public IEnumerable<IEnumerable<Message>> GetConv(int loggedUsrID, int targetUsrId)
        {

            var liste = DB.Messages.ToList().Where(m => (m.usrSource == loggedUsrID && m.usrTarget == targetUsrId) || (m.usrTarget == loggedUsrID && m.usrSource == targetUsrId));
            liste.OrderBy(m => m.Date);
            List<List<Message>> sortedList = new List<List<Message>>();
            List<Message> tempList = new List<Message>();
            if(liste.Count() == 1) 
            {
                tempList.Add(liste.ElementAt(0));
                sortedList.Add(tempList);
                return sortedList;
            }
            for (int i = 0; i < liste.Count(); i++)
            {
                if(tempList.Count() == 0)
                {
                    tempList.Add(liste.ElementAt(i));
                }
                else
                {
                    if((liste.ElementAt(i).Date - tempList.ElementAt(0).Date).TotalMinutes < 30)
                    {
                        tempList.Add(liste.ElementAt(i));
                    }
                    else
                    {
                        sortedList.Add(tempList);
                        tempList = new List<Message>
                        {
                            liste.ElementAt(i)
                        };
                    }
                }
            }
            sortedList.Add(tempList);
            return sortedList;
        }
    }
}