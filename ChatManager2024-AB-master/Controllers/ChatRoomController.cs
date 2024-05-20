using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace ChatManager.Controllers
{
    public class ChatRoomController : Controller
    {
        // GET: ChatRoom
        [OnlineUsers.UserAccess]
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
                IEnumerable<IEnumerable<Models.Message>> listMessages;
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
                (IEnumerable<User> amis, User user,IEnumerable<IEnumerable<Models.Message>> messages) retour = (filteredUsers, userSelec, listMessages);
                return PartialView(retour);
            }

            return null;
        }

        public void SendMessage(string message, bool forceRefresh = false)
        {
            if((int)Session["SelecID"] != 0 && Session["SelecID"] != null)
            {
                var objMessage = new Models.Message(OnlineUsers.GetSessionUser().Id, (int)Session["SelecID"]);
                objMessage.content = message;
                DB.Messages.Add(objMessage);
            }
        }
        public void SetSelec(int usrID)
        {
            Session["SelecId"] = usrID;
        }
        public void UpdateMessage(int messageID, string message)
        {
            var messageInit = DB.Messages.Get(messageID);
            if(messageInit != null && messageInit.usrSource == OnlineUsers.GetSessionUser().Id)
            {
                messageInit.content = message;
                DB.Messages.Update(messageInit);
            }
        }
        public void DeleteMessage(int messageID)
        {
            var messageInit = DB.Messages.Get(messageID);
            if (messageInit != null && messageInit.usrSource == OnlineUsers.GetSessionUser().Id)
            {
                DB.Messages.Delete(messageID);
            }
        }
        [OnlineUsers.AdminAccess]
        public ActionResult Admin()
        {
            return View();
        }
        public ActionResult AdminMessages(bool forceRefresh = false)
        {
            if (forceRefresh || OnlineUsers.HasChanged()  || DB.Messages.HasChanged)
            {
                var listeMessage = DB.Messages.ToList().OrderByDescending(m => m.Id).ToList();
                return PartialView(listeMessage);
            }
            return null;
        }
    }
}