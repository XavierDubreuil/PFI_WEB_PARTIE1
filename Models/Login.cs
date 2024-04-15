using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class Login
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Start {  get; set; }
        public DateTime? End { get; set; } = null;


        public Login(int userId)
        {          
            UserId = userId;
            Start = DateTime.Now;
        }
        public Login() { }
    }
}