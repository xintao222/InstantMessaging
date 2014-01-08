using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    /// <summary>
    /// 用于区别不同信息
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        LOGIN,

        /// <summary>
        /// 用户退出
        /// </summary>
        LOGOFF,

        /// <summary>
        /// 聊天
        /// </summary>
        CHAT
    }
}
