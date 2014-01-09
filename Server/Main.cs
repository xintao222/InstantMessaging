using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Server
{
    public partial class ServerForm : Form
    {
        public ServerForm()
        {
            InitializeComponent();

            richTextBoxUserRecords.Visible = false;
            btnStop.Enabled = false;
        }


        UdpClient localClient;
        IPEndPoint localIPE;
        IPEndPoint remoteIPE;
        List<UserRecords> userRecordsList;
        ChatMessage chatMessage;
        private void ReceiveMessage(object obj)
        {
            userRecordsList = new List<UserRecords>();
            byte[] receiveDgram;
            string msg = string.Empty;
            localIPE = new IPEndPoint(IPAddress.Parse(txtIPAddress.Text), Convert.ToInt32(txtPort.Text));
            localClient = new UdpClient(localIPE);
            remoteIPE = new IPEndPoint(IPAddress.Any, 0);  // receive any port and ipaddress

            while (true)
            {
                try
                {
                    receiveDgram = localClient.Receive(ref remoteIPE);
                    chatMessage = JsonConvert.DeserializeObject<ChatMessage>(Encoding.Unicode.GetString(receiveDgram));
                    chatMessage.Address = remoteIPE.Address.ToString();
                    chatMessage.Port = remoteIPE.Port;

                    this.Invoke(new MethodInvoker(() =>
                    {
                        richTextBoxUserRecords.AppendText(string.Format("{0}:{1}-{2}\r\n", chatMessage.type, chatMessage.MyName, remoteIPE));
                    }));
                }
                catch
                {
                    if (localClient.Client != null)
                    {
                        localClient.Close();
                    }
                    return;
                }

                Process(chatMessage.type);
                remoteIPE = new IPEndPoint(IPAddress.Any, 0);
            }
        }

        private void Process(MessageType type)
        {
            switch (type)
            {
                case MessageType.LOGIN:
                    ClientLogin(chatMessage);
                    break;
                case MessageType.LOGOFF:
                    ClientLogoff(chatMessage);
                    break;
                default:
                    throw new Exception("ERROR");
            }
        }

        /// <summary>
        /// Login
        /// 1. send this user info to others
        /// 2. send online users info to this user
        /// </summary>
        private void ClientLogin(ChatMessage chatMessage)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(chatMessage));

            // 1
            foreach (UserRecords ur in userRecordsList)
            {
                localClient.BeginSend(buffer, buffer.Length, ur.UserIPE, null, null);
            }

            // 2
            foreach (UserRecords ur in userRecordsList)
            {
                ChatMessage chat = new ChatMessage
                {
                    type = ur.Flag,
                    MyName = ur.userInfo.Name,
                    Address = ur.UserIPE.Address.ToString(),
                    Port = ur.UserIPE.Port
                };

                string json = JsonConvert.SerializeObject(chat);

                byte[] dgram = Encoding.Unicode.GetBytes(json);
                localClient.Send(dgram, dgram.Length, remoteIPE);
            }

            // Save userName and remoteIPE to List<UserRecords>
            UserRecords user = new UserRecords(chatMessage.MyName, remoteIPE);
            userRecordsList.Add(user);
        }

        /// <summary>
        /// Logoff
        /// notify other users
        /// </summary>
        private void ClientLogoff(ChatMessage chatMessage)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(chatMessage));

            UserRecords user = new UserRecords(chatMessage.MyName, remoteIPE);

            // remove current user info and notify other users
            var task = Task.Factory.StartNew(() =>
                {
                    userRecordsList.Remove(userRecordsList.Find(record => record.UserIPE.ToString().Trim() == user.UserIPE.ToString().Trim()));
                });
            task.ContinueWith(task1 => Parallel.ForEach(userRecordsList,
                                                        userRecord =>
                                                        localClient.BeginSend(buffer, buffer.Length, userRecord.UserIPE, null, null)));
        }


        #region ButtonClick

        /// <summary>
        /// Start listen
        /// </summary>
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            txtIPAddress.Enabled = false;
            txtPort.Enabled = false;
            ShowOnlineUsers();

            richTextBoxUserRecords.AppendText("Server is starting...\r\n");

            ThreadPool.QueueUserWorkItem(ReceiveMessage);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            txtIPAddress.Enabled = true;
            txtPort.Enabled = true;
            richTextBoxUserRecords.AppendText("Server is stoped...\r\n");

            if (localClient != null)
            {
                localClient.Client.Close();
            }
            //if (localClient.Client != null)
            //{
            //    localClient.Client.Dispose();
            //}
        }

        /// <summary>
        ///  prevent double-click maximum
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0xa3)
            {
                return;
            }
            base.WndProc(ref m);
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            btnConfig.Dock = DockStyle.Top;
            btnOnlineUser.Dock = DockStyle.Bottom;
            richTextBoxUserRecords.Visible = false;
            groupBox1.Visible = true;
        }

        private void btnOnlineUser_Click(object sender, EventArgs e)
        {
            ShowOnlineUsers();
        }

        /// <summary>
        /// Display online users
        /// </summary>
        private void ShowOnlineUsers()
        {
            btnConfig.Dock = DockStyle.Top;
            btnOnlineUser.BringToFront();
            btnOnlineUser.Dock = DockStyle.Top;
            groupBox1.Visible = false;
            richTextBoxUserRecords.Visible = true;
        }
        #endregion
    }
}
