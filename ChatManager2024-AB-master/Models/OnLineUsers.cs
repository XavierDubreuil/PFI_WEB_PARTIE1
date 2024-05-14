using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace ChatManager.Models
{
    public static class OnlineUsers
    {
        public static List<int> ConnectedUsersId
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsers"] == null)
                    HttpRuntime.Cache["OnLineUsers"] = new List<int>();
                return (List<int>)HttpRuntime.Cache["OnLineUsers"];
            }
        }
        #region Status change management
        private static string SerialNumber
        {
            get
            {
                if (HttpRuntime.Cache["OnLineUsersSerialNumber"] == null)
                    SetHasChanged();
                return (string)HttpRuntime.Cache["OnLineUsersSerialNumber"];
            }
            set
            {
                HttpRuntime.Cache["OnLineUsersSerialNumber"] = value;
            }
        }
        public static bool HasChanged()
        {
            if (HttpContext.Current.Session["SerialNumber"] == null)
            {
                HttpContext.Current.Session["SerialNumber"] = SerialNumber;
                return true;
            }
            string sessionSerialNumber = (string)HttpContext.Current.Session["SerialNumber"];
            HttpContext.Current.Session["SerialNumber"] = SerialNumber;
            return SerialNumber != sessionSerialNumber;
        }
        public static void SetHasChanged()
        {
            SerialNumber = Guid.NewGuid().ToString();
        }
        #endregion
        #region Session management
        public static void AddSessionUser(int userId)
        {
            ConnectedUsersId.Add(userId);
            HttpContext.Current.Session["CurrentLoginId"] = DB.Logins.Add(userId).Id;
            HttpContext.Current.Session["UserId"] = userId;
            SetHasChanged();
        }
        public static void RemoveSessionUser(bool updateLoginsJournal = false)
        {
            User user = GetSessionUser();
            if (user != null)
            {
                if (updateLoginsJournal)
                    DB.Logins.UpdateLogout((int)HttpContext.Current.Session["CurrentLoginId"]);
                ConnectedUsersId.Remove(user.Id);
            }
            HttpContext.Current?.Session.Abandon();
            SetHasChanged();
        }
        public static bool IsOnLine(int userId)
        {
            return ConnectedUsersId.Contains(userId);
        }
        public static User GetSessionUser()
        {
            if (HttpContext.Current.Session["UserId"] != null)
            {
                User currentUser = DB.Users.Get((int)HttpContext.Current.Session["UserId"]);
                return currentUser;
            }
            return null;
        }
        public static bool Write_Access()
        {
            User sessionUser = OnlineUsers.GetSessionUser();
            if (sessionUser != null)
            {
                return sessionUser.IsPowerUser || sessionUser.IsAdmin;
            }
            return false;
        }
        #endregion
        #region Notifications handling
        private static List<Notification> Notifications
        {
            get
            {
                if (HttpRuntime.Cache["Notifications"] == null)
                    HttpRuntime.Cache["Notifications"] = new List<Notification>();
                return (List<Notification>)HttpRuntime.Cache["Notifications"];
            }
        }
        public static List<string> LastNotifications
        {
            get
            {
                if (HttpContext.Current.Session["LastNotifications"] == null)
                    HttpContext.Current.Session["LastNotifications"] = new List<string>();
                return (List<string>)HttpContext.Current.Session["LastNotifications"];
            }
        }
        public static List<string> PopLastNotifications()
        {
            List<string> messages = new List<string>(); 
            foreach(string message in LastNotifications)
            {
                messages.Add(message);
            }
            LastNotifications.Clear();
            return messages;
        }
        public static void AddNotification(int TargetUserId, string Message)
        {
            User user = DB.Users.Get(TargetUserId);
            if (user != null && IsOnLine(user.Id) && !user.NotificationsDisabled)
            {
                Notifications.Add(new Notification() { TargetUserId = TargetUserId, Message = Message });
            }
        }
        public static bool CurrentUserHasNotifications()
        {
            return LastNotifications.Count > 0;
        }
        public static void GetCurrentUserNotifications()
        {
            User currentUser = GetSessionUser();
            if (currentUser != null)
            {
                List<string> notificationMessages = new List<string>();
                List<Notification> notifications = Notifications.Where(n => n.TargetUserId == currentUser.Id).OrderBy(n => n.Created).ToList();
                if (!currentUser.NotificationsDisabled)
                    foreach (Notification notification in notifications)
                    {
                        LastNotifications.Add(notification.Message);
                    }
                else
                {
                    LastNotifications.Clear();
                }
            }
        }
        public static List<string> PopNotifications(int TargetUserId)
        {
            GetCurrentUserNotifications();
            List<string> notificationMessages = new List<string>();
            if (IsOnLine(TargetUserId))
            {
                List<Notification> notifications = Notifications.Where(n => n.TargetUserId == TargetUserId).OrderBy(n => n.Created).ToList();
                foreach (Notification notification in notifications)
                {
                    notificationMessages.Add(notification.Message);
                    Notifications.Remove(notification);
                }
            }
            return notificationMessages;
        }
        #endregion
        #region Access control
        public class UserAccess : AuthorizeAttribute
        {
            private bool ServerSideResponseHandling { get; set; }
            public UserAccess(bool serverSideResponseHandling = true)
            {
                ServerSideResponseHandling = serverSideResponseHandling;
            }
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                User sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null)
                {
                    if (sessionUser.Blocked)
                    {
                        RemoveSessionUser();
                        if (ServerSideResponseHandling)
                        {
                            httpContext.Response.Redirect("~/Accounts/Login?message=Compte bloqué!");
                            return false;
                        }
                        else
                        {
                            httpContext.Response.StatusCode = 403; // Forbiden status
                        }
                    }
                    return true;
                }
                httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
                return false;

            }
        }
        public class PowerUserAccess : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                User sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null && (sessionUser.IsPowerUser || sessionUser.IsAdmin))
                    return true;
                else
                {
                    RemoveSessionUser();
                    httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!", true);
                }
                return false;
            }
        }
        public class AdminAccess : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                User sessionUser = OnlineUsers.GetSessionUser();
                if (sessionUser != null && sessionUser.IsAdmin)
                    return true;
                else
                {
                    RemoveSessionUser();
                    httpContext.Response.Redirect("~/Accounts/Login?message=Accès non autorisé!");
                }
                return true;
            }
        }
        #endregion
    }
}