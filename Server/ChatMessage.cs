using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Server
{
    /// <summary>
    /// 聊天信息
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// 信息类别
        /// </summary>
        public MessageType type { get; set; }

        /// <summary>
        /// 对方的名字
        /// </summary>
        public string PeerName { get; set; }

        /// <summary>
        /// 我的名字
        /// </summary>
        public string MyName { get; set; }
        
        /// <summary>
        /// 聊天内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendDate { get; set; }


        public string Address { get; set; }

        public int Port { get; set; }
    }
}
