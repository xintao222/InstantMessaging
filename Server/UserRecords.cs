using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Server
{
    /// <summary>
    /// save user info and endpoint
    /// </summary>
    class UserRecords
    {
        public MessageType Flag { get; set; }
        public IPEndPoint UserIPE { get; set; }

        public UserInfo userInfo;
        public UserRecords(string name, IPEndPoint userIPE)
        {
            userInfo = new UserInfo();
            userInfo.Name = name;
            UserIPE = userIPE;
        }
    }
}
