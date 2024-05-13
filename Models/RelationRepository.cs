using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class RelationRepository : Repository<Relation>
    {
        public void InstatiateStart()
        {

            foreach (var user in DB.Users.ToList())
            {
                foreach (var user2 in DB.Users.ToList())
                {
                    if(user.Id != user2.Id) 
                    {
                        DB.Relations.Add(new Relation(user.Id, user2.Id));
                    }
                }
            }

        }
        public (Relation rel1, Relation rel2) FindRelation (int IDusr1, int IDusr2)
        {
            Relation relation1 = null;
            Relation relation2 = null;
            foreach(var relation in DB.Relations.ToList())
            {
                if(relation1 == null && relation.UsersId.usr1 == IDusr1 && relation.UsersId.usr2 == IDusr2)
                {
                    relation1 = relation;
                }
                else if(relation2 == null && relation.UsersId.usr1 == IDusr2 && relation.UsersId.usr2 == IDusr1)
                {
                    relation2 = relation;
                }
            }
                return(relation1, relation2);
        }
        public List<User> GetFriends (int usrId)
        {
            List<User> users = new List<User>();
            foreach(var relation in DB.Relations.ToList())
            {
                if(relation.UsersId.usr1 == usrId)
                {
                    users.Add(DB.Users.Get(relation.UsersId.usr2));
                }
            }
            return users;
        }
    }
}