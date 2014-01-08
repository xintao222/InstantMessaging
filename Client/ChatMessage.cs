using System;

namespace Client
{
    /// <summary>
    /// Chat Message
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Message Type: LOGIN,LOGOUT,CHAT
        /// </summary>
        public MessageType type { get; set; }

        /// <summary>
        /// Other's name
        /// </summary>
        public string PeerName { get; set; }

        /// <summary>
        /// My Name
        /// </summary>
        public string MyName { get; set; }
        
        /// <summary>
        /// Chat Content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// SendDate
        /// </summary>
        public DateTime SendDate { get; set; }

        /// <summary>
        /// IPAddress
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }
    }
}
