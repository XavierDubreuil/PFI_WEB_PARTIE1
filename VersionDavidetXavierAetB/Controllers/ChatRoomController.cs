using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return View();
        }

        public ActionResult Friends(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Relations.HasChanged || OnlineUsers.HasChanged() || DB.Users.HasChanged)
            {
                int usrID = OnlineUsers.GetSessionUser().Id;
                List<User> users = DB.Relations.GetFriends(usrID);
                return PartialView(users);
            }

            return null;
        }
        public ActionResult Message(int usrId = 0, bool forceRefresh = false) 
        {
            if (forceRefresh || DB.Relations.HasChanged || OnlineUsers.HasChanged() || DB.Users.HasChanged)
            {
                if(usrId == 0)
                {
                    return null;
                }
                //int usrID = OnlineUsers.GetSessionUser().Id;
                //List<User> users = DB.Relations.GetFriends(usrID);
                return PartialView(DB.Users.Get(usrId));
            }

            return null;
        }
    }
}