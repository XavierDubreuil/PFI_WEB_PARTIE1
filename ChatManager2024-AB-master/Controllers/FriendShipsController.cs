using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace ChatManager.Controllers
{
    public static class WildcardMatch
    {
        #region Public Methods
        public static bool IsLike(string pattern, string text, bool caseSensitive = false)
        {
            pattern = pattern.Replace(".", @"\.");
            pattern = pattern.Replace("?", ".");
            pattern = pattern.Replace("*", ".*?");
            pattern = pattern.Replace(@"\", @"\\");
            pattern = pattern.Replace(" ", @"\s");
            return new Regex(pattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase).IsMatch(text);
        }
        #endregion
    }

    [OnlineUsers.UserAccess]
    public class FriendshipsController : Controller
    {
        private string SearchText
        {
            get
            {
                if (Session["SearchText"] == null)
                    Session["SearchText"] = "";
                return (string)Session["SearchText"];
            }
            set { Session["SearchText"] = value; }
        }
        private bool FilterNotFriend
        {
            get
            {
                if (Session["FilterNotFriend"] == null)
                    Session["FilterNotFriend"] = true;
                return (bool)Session["FilterNotFriend"];
            }
            set { Session["FilterNotFriend"] = value; }
        }
        private bool FilterRequest
        {
            get
            {
                if (Session["FilterRequest"] == null)
                    Session["FilterRequest"] = true;
                return (bool)Session["FilterRequest"];
            }
            set { Session["FilterRequest"] = value; }
        }
        private bool FilterPending
        {
            get
            {
                if (Session["FilterPending"] == null)
                    Session["FilterPending"] = true;
                return (bool)Session["FilterPending"];
            }
            set { Session["FilterPending"] = value; }
        }
        private bool FilterFriend
        {
            get
            {
                if (Session["FilterFriend"] == null)
                    Session["FilterFriend"] = true;
                return (bool)Session["FilterFriend"];
            }
            set { Session["FilterFriend"] = value; }
        }
        private bool FilterRefused
        {
            get
            {
                if (Session["FilterRefused"] == null)
                    Session["FilterRefused"] = true;
                return (bool)Session["FilterRefused"];
            }
            set { Session["FilterRefused"] = value; }
        }
        private bool FilterBlocked
        {
            get
            {
                if (Session["FilterBlocked"] == null)
                    Session["FilterBlocked"] = true;
                return (bool)Session["FilterBlocked"];
            }
            set { Session["FilterBlocked"] = value; }
        }

        [OnlineUsers.UserAccess]
        public ActionResult Index()
        {
            ViewBag.SearchText = SearchText;
            ViewBag.FilterRequest = FilterRequest;
            ViewBag.FilterNotFriend = FilterNotFriend;
            ViewBag.FilterPending = FilterPending;
            ViewBag.FilterFriend = FilterFriend;
            ViewBag.FilterRefused = FilterRefused;
            ViewBag.FilterBlocked = FilterBlocked;
            Session["LastAction"] = "/Friendships/index";
            return View();
        }

        public ActionResult Search(string text)
        {
            SearchText = text;
            return null;
        }

        public Regex ToRegex(string text)
        {
            if (text == "")
                text = "*";
            else
                if (text.Length > 0 && text.IndexOf("*", StringComparison.OrdinalIgnoreCase) == -1)
            {
                text = "*" + text + "*";
            }
            return new Regex("^" + Regex.Escape(text.Trim().ToLower()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase);
        }

        public ActionResult SetFilterNotFriend(bool check)
        {
            FilterNotFriend = check;
            return null;
        }
        public ActionResult SetFilterRequest(bool check)
        {
            FilterRequest = check;
            return null;
        }
        public ActionResult SetFilterPending(bool check)
        {
            FilterPending = check;
            return null;
        }
        public ActionResult SetFilterFriend(bool check)
        {
            FilterFriend = check;
            return null;
        }
        public ActionResult SetFilterRefused(bool check)
        {
            FilterRefused = check;
            return null;
        }
        public ActionResult SetFilterBlocked(bool check)
        {
            FilterBlocked = check;
            return null;
        }
        private List<User> FilterUsers()
        {
            var loggedUser = OnlineUsers.GetSessionUser();
            List<User> filteredUsers = new List<User>();
            Regex regex = ToRegex(SearchText);
            List<User> users = SearchText != "" ?
                DB.Users.SortedUsers().Where(u => u.Verified && regex.IsMatch((u.FirstName + " " + u.LastName).ToLower())).ToList() :
                DB.Users.SortedUsers().ToList().Where(u => u.Verified).ToList();
            foreach (User user in users)
            {
                bool keep = true;
                switch (DB.Friendships.RelationStatus(loggedUser.Id, user.Id))
                {
                    case EnumRelationStatus.NotFriend: /* not friend relation*/
                        keep = FilterNotFriend;
                        break;
                    case EnumRelationStatus.Friend: /* friend*/
                        keep = FilterFriend;
                        break;
                    case EnumRelationStatus.Have_Been_Declined: /* friendship declined*/
                        keep = FilterRefused;
                        break;
                    case EnumRelationStatus.Request_Sender: /* friendship pending*/
                        keep = FilterPending;
                        break;
                    case EnumRelationStatus.Have_Decline: /* friendship declined*/
                        keep = FilterRefused;
                        break;
                    case EnumRelationStatus.Request_Receiver: /* friendship request*/
                        keep = FilterRequest;
                        break;
                    case EnumRelationStatus.Blocked: /* blocked*/
                        keep = FilterBlocked;
                        break;
                    default: break;
                }
                if (keep) filteredUsers.Add(user);
            }
            return filteredUsers;
        }

        public ActionResult GetFriendshipsStatus(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Friendships.HasChanged || OnlineUsers.HasChanged() || DB.Users.HasChanged)
                return PartialView(FilterUsers());

            return null;
        }
        public ActionResult SendFriendshipRequest(int id)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Friendships.Add_FiendshipRequest(currentUser.Id, id);
            return null;
        }
        public ActionResult RemoveFriendship(int id)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Friendships.Remove_Friendship(currentUser.Id, id);
            return null;
        }
        public ActionResult RemoveFriendshipRequest(int id)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Friendships.Remove_FriendshipRequest(currentUser.Id, id);
            return null;
        }
        public ActionResult AcceptFriendshipRequest(int id)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Friendships.Accept_Friendship(id, currentUser.Id);
            return null;
        }
        public ActionResult DeclineFriendshipRequest(int id)
        {
            User currentUser = OnlineUsers.GetSessionUser();
            DB.Friendships.Decline_Friendship(id, currentUser.Id);
            return null;
        }
    }

}