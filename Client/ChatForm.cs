using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Client
{
    public partial class ChatForm : Form
    {
        private ChatMessage OtherInfo;
        private ChatMessage PersonalInfo;
        StringBuilder chatContent = new StringBuilder();
        public ChatForm(ChatMessage otherInfo, ChatMessage personalInfo)
        {
            InitializeComponent();

            OtherInfo = otherInfo;
            PersonalInfo = personalInfo;

            ShowUserInfo();
        }

        /// <summary>
        /// Display User Information
        /// </summary>
        public void ShowUserInfo()
        {
            #region  display other's information
            lblPeerName.Text = OtherInfo.MyName;
            lblPeerIP.Text = OtherInfo.Address;
            lblPeerPort.Text = OtherInfo.Port.ToString();
            #endregion


            # region display my information
            lblMyName.Text = PersonalInfo.MyName;
            lblMyIP.Text = PersonalInfo.Address;
            lblMyPort.Text = PersonalInfo.Port.ToString();
            #endregion

            this.Text = string.Format("{0}:{1}", PersonalInfo.MyName, OtherInfo.MyName);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text.Trim() == "")
            {
                SystemSounds.Exclamation.Play();
                this.ShakeForm();
            }
            else
            {
                sendMessage(txtMessage.Text);
            }
        }

        /// <summary>
        /// Send message to others
        /// </summary>
        /// <param name="msg"></param>
        public void sendMessage(string msg)
        {
            string sendMsg = string.Format("{0}:{1}\r\n", lblMyName.Text, msg);
            richTextBoxChat.AppendText(sendMsg);
            richTextBoxChat.ScrollToCaret();
            chatContent.Append(sendMsg);

            msg = msg.Encrypt();    // Encrypt Message

            ChatMessage chatMessage = new ChatMessage
            {
                type = MessageType.CHAT,
                PeerName = lblPeerName.Text.Trim(),
                MyName = lblMyName.Text.Trim(),
                Content = msg,
                SendDate = DateTime.Now
            };

            // use jsonserialize
            byte[] sendDgram = Encoding.Unicode.GetBytes(chatMessage.JsonSerialize());

            UdpClient localClient = new UdpClient();
            IPEndPoint remoteIPE = new IPEndPoint(IPAddress.Parse(lblPeerIP.Text.Trim()), int.Parse(lblPeerPort.Text.Trim()));
            AsyncCallback callBack = new AsyncCallback(AfterSendMessage);
            localClient.BeginSend(sendDgram, sendDgram.Length, remoteIPE, callBack, localClient);
        }

        /// <summary>
        /// callback method
        /// </summary>
        public void AfterSendMessage(IAsyncResult asyncResult)
        {
            UdpClient localClient = (UdpClient)asyncResult.AsyncState;
            localClient.Close();
        }

        /// <summary>
        /// Clear the input box
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtMessage.Text = "";
            txtMessage.Focus();
        }


        /// <summary>
        /// Receive and display message
        /// </summary>
        public void ShowNewMsg(ChatMessage receiveMsg)
        {
            // 对消息内容进行解密
            receiveMsg.Content = receiveMsg.Content.Decrypt();
            string data = string.Format("{0}:{1}\r\n", receiveMsg.PeerName, receiveMsg.Content);

            this.Invoke(new MethodInvoker(() =>
            {
                richTextBoxChat.SelectionColor = Color.Purple;
                richTextBoxChat.AppendText(data);
                richTextBoxChat.ScrollToCaret();
                this.Activate();
            }));
            chatContent.Append(data);
        }


        /// <summary>
        /// prevent double-click maximum form
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0xa3)
            {
                return;
            }
            base.WndProc(ref m);
        }


        /// <summary>
        /// save chat content
        /// </summary>
        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StreamWriter streamWrite;
            if (!File.Exists("Records.txt"))
            {
                using (FileStream fileStream = new FileStream("Records.txt", FileMode.Create, FileAccess.Write))
                {
                    streamWrite = new StreamWriter(fileStream, Encoding.Default);
                    streamWrite.WriteLine(chatContent);
                    streamWrite.Close();
                }
            }
            else
            {
                using (FileStream fileStream = new FileStream("Records.txt", FileMode.Append, FileAccess.Write))
                {
                    streamWrite = new StreamWriter(fileStream, Encoding.Default);
                    streamWrite.WriteLine(chatContent);
                    streamWrite.Close();
                }
            }
        }
    }
}
