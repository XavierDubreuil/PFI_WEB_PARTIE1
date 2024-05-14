using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace ChatManager.Models
{
    public class DB
    {
        private static readonly DB instance = new DB();

        public static UsersRepository Users { get; set; }
        public static LoginsRepository Logins { get; set; }
        public static Repository<Gender> Genders { get; set; }
        public static Repository<UserType> UserTypes { get; set; }
        public static Repository<UnverifiedEmail> UnverifiedEmails { get; set; }
        public static FriendshipsRepository Friendships { get; set; }
        public static Repository<ResetPasswordCommand> ResetPasswordCommands { get; set; }
        public static MessagesRepository Messages { get; set; }
       
        public DB()
        {
            Users = new UsersRepository();
            Logins = new LoginsRepository();
            Genders = new Repository<Gender>();
            UserTypes = new Repository<UserType>();
            UnverifiedEmails = new Repository<UnverifiedEmail>();
            Friendships = new FriendshipsRepository();
            ResetPasswordCommands = new Repository<ResetPasswordCommand>();
            Messages = new MessagesRepository();
           
            InitRepositories(this);
        }
        public static DB Instance
        {
            get { return instance; }
        }
        private static void InitRepositories(DB db)
        {
            string serverPath = HostingEnvironment.MapPath(@"~/App_Data/");
            PropertyInfo[] myPropertyInfo = db.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in myPropertyInfo)
            {
                if (propertyInfo.PropertyType.Name.Contains("Repository"))
                {
                    MethodInfo method = propertyInfo.PropertyType.GetMethod("Init");
                    method.Invoke(propertyInfo.GetValue(db), new object[] { serverPath + propertyInfo.Name + ".json" });
                }
            }
        }
    }
}