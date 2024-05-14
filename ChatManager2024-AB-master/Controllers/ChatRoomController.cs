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

        public ActionResult Friends(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Friendships.HasChanged || OnlineUsers.HasChanged() || DB.Users.HasChanged)
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
            if (forceRefresh || DB.Friendships.HasChanged || OnlineUsers.HasChanged() || DB.Users.HasChanged)
            {
                if (usrId == 0 && (int)Session["SelecID"] == 0)
                {
                    return null;
                }
                else if(usrId != 0)
                {
                    Session["SelecID"] = usrId;
                }
                usrId = (int)Session["SelecID"];
                var listMessages = DB.Messages.GetConv(OnlineUsers.GetSessionUser().Id, usrId);
                var usr = DB.Users.Get(usrId);
                return PartialView((listMessages, usr));
            }

            return null;
        }
    }
}