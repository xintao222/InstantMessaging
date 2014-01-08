using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    /// <summary>
    /// save user info
    /// </summary>
    class UserInfo
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public UserInfo() { }
        public UserInfo(string name,string password)
        {
            Name = name;
            Password = password;
        }
    }
}
