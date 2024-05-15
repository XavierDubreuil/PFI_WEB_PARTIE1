using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace ChatManager.Controllers
{
    public class ChatRoomController : Controller
    {
        // GET: ChatRoom
        public ActionResult Index()
        {
            Session["LastAction"] = "/ChatRoom/Index";
            Session["SelecId"] = 0;
            return View();
        }
        public ActionResult Conv(bool forceRefresh = false)
        {
            if (forceRefresh || OnlineUsers.HasChanged() || DB.Friendships.HasChanged || DB.Messages.HasChanged )
            {
                var loggedUser = OnlineUsers.GetSessionUser();
                //user Sélectionné
                User userSelec;
                if ((int)Session["SelecId"] == 0)
                    userSelec = null;
                else
                    userSelec = DB.Users.Get((int)Session["SelecId"]);
                //liste messages
                IEnumerable<IEnumerable<Message>> listMessages;
                if (userSelec == null)
                    listMessages = null;
                else
                    listMessages = DB.Messages.GetConv(OnlineUsers.GetSessionUser().Id, (int)Session["SelecId"]);

                //liste d'amis
                List<User> filteredUsers = new List<User>(); 
                List<User> users =
                    DB.Users.SortedUsers().ToList().Where(u => u.Verified).ToList();
                foreach (User user in users)
                {
                    if (DB.Friendships.RelationStatus(loggedUser.Id, user.Id) == EnumRelationStatus.Friend)
                    {
                        filteredUsers.Add(user);
                    }

                }
                //Vérif si une amitié a changé
                if(userSelec != null && !DB.Friendships.AreFriends(OnlineUsers.GetSessionUser().Id, (int)Session["SelecId"]))
                {
                    userSelec = null;
                    listMessages = null;
                    Session["SelecId"] = 0;
                }
                (IEnumerable<User> amis, User user,IEnumerable<IEnumerable<Message>> messages) retour = (filteredUsers, userSelec, listMessages);
                return PartialView(retour);
            }

            return null;
        }
        public ActionResult Friends(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Friendships.HasChanged)
            {
                var loggedUser = OnlineUsers.GetSessionUser();
                List<User> filteredUsers = new List<User>();;
                List<User> users =
                    DB.Users.SortedUsers().ToList().Where(u => u.Verified).ToList();
                foreach (User user in users)
                {
                    if(DB.Friendships.RelationStatus(loggedUser.Id, user.Id) == EnumRelationStatus.Friend)
                    {
                        filteredUsers.Add(user);
                    }

                }
                return PartialView(filteredUsers);
            }

            return null;
        }
        public ActionResult Message(int usrId = 0, bool forceRefresh = false)
        {
            if (forceRefresh || DB.Friendships.HasChanged || DB.Messages.HasChanged)
            {


                if (usrId == 0 && (int)Session["SelecID"] == 0)
                {
                    return null;
                }
                else if(usrId != 0)
                {
                    Session["SelecID"] = usrId;
                }
                //if (!DB.Friendships.AreFriends(OnlineUsers.GetSessionUser().Id, (int)Session["SelecID"]))
                //{
                //    Session["SelecID"] = 0;
                //    return PartialView();
                //}
                usrId = (int)Session["SelecID"];
                var listMessages = DB.Messages.GetConv(OnlineUsers.GetSessionUser().Id, usrId);
                var usr = DB.Users.Get(usrId);
                return PartialView((listMessages, usr));
            }

            return null;
        }
        public void SendMessage(string message, bool forceRefresh = false)
        {
            if((int)Session["SelecID"] != 0 && Session["SelecID"] != null)
            {
                var objMessage = new Message(OnlineUsers.GetSessionUser().Id, (int)Session["SelecID"]);
                objMessage.content = message;
                DB.Messages.Add(objMessage);
            }
        }
        public void SetSelec(int usrID)
        {
            Session["SelecId"] = usrID;
        }
    }
}