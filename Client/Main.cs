using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Media;
using System.Threading;

namespace Client
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        List<ChatForm> chatFormList = new List<ChatForm>();
        UserInfo userInfo;
        IPEndPoint remoteIPE;
        UdpClient localClient;
        private bool isReceive = true;
        private const string note = "Client is starting...";

        /// <summary>
        /// login
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userName = txtUserName.Text.Trim();
            string password = txtPassword.Text.Trim();
            if (userName.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace())
            {
                this.ShakeForm();
                txtUserName.Text = txtPassword.Text = "";
                txtUserName.Focus();
                SystemSounds.Exclamation.Play();
                return;
            }

            lblShowConnMsg.Visible = pictureBox1.Visible = true;
            userInfo = new UserInfo { Name = userName, Password = password };
            Func<int> myFun = () =>
            {
                // validate input user info
                OperateUserInfo operate = new OperateUserInfo();
                Thread.Sleep(2000);
                return operate.ValidateUserInfo(userInfo);
            };
            myFun.BeginInvoke(CallBackMethod, myFun);
        }

        private void CallBackMethod(IAsyncResult target)
        {
            Func<int> myFun = (Func<int>)target.AsyncState;
            int record = myFun.EndInvoke(target);
            if (record > 0)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    panelLogin.Visible = false;  // hide panelLogin
                    this.Text = string.Format("{0}", userInfo.Name);
                }));
            }
            else
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    this.ShakeForm();
                    txtUserName.Text = txtPassword.Text = "";
                    txtUserName.Focus();
                    SystemSounds.Exclamation.Play();
                    lblShowConnMsg.Visible = pictureBox1.Visible = false;
                }));
            }
        }

        /// <summary>
        /// register
        /// </summary>
        private void btnRegister_Click(object sender, EventArgs e)
        {
            UserRegister register = new UserRegister();
            register.ShowDialog();
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        private void btnConnection_Click(object sender, EventArgs e)
        {
            btnConnection.Enabled = txtIPAddress.Enabled = txtPort.Enabled = false;
            localClient = new UdpClient();

            ChatMessage chatMessage = new ChatMessage { type = MessageType.LOGIN, MyName = this.Text };
            byte[] dgram = Encoding.Unicode.GetBytes(chatMessage.JsonSerialize());
            remoteIPE = new IPEndPoint(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtPort.Text));
            
            // send login request
            localClient.BeginSend(dgram, dgram.Length, remoteIPE, ReceiveLoginMessage, localClient);
            this.Invoke(new MethodInvoker(() => { listViewOnlineUsers.Items.Add(note + "\r\n"); }));
        }

        /// <summary>
        /// receive response from server
        /// </summary>
        /// <param name="asySend"></param>
        private void ReceiveLoginMessage(IAsyncResult asySend)
        {
            UdpClient localClient = (UdpClient)asySend.AsyncState;
            byte[] dgram = null;

            try
            {
                while (isReceive)
                {
                    dgram = localClient.Receive(ref remoteIPE);
                    string receiveMessge = Encoding.Unicode.GetString(dgram);
                    ChatMessage chatMessage = receiveMessge.JsonDeserialize<ChatMessage>();
                    ProcessMessage(chatMessage);
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// process receive response message
        /// </summary>
        private void ProcessMessage(ChatMessage showUserInfo)
        {
            switch (showUserInfo.type)
            {
                case MessageType.LOGIN:
                    this.Invoke(new MethodInvoker(() =>
                    {
                        listViewOnlineUsers.Items.Add(string.Format("{0}-{1}:{2}", showUserInfo.MyName, showUserInfo.Address, showUserInfo.Port));
                    }));
                    break;
                case MessageType.LOGOFF:
                    this.Invoke(new MethodInvoker(() =>
                    {
                        for (int i = 0; i < listViewOnlineUsers.Items.Count; i++)
                        {
                            if (listViewOnlineUsers.Items[i].Text.Trim() == string.Format("{0}-{1}:{2}", showUserInfo.MyName, showUserInfo.Address, showUserInfo.Port))
                            {
                                listViewOnlineUsers.Items.RemoveAt(i);
                                break;
                            }
                        }
                    }));
                    break;
                case MessageType.CHAT:
                    string message = string.Format("{0}:{1}", showUserInfo.PeerName, showUserInfo.MyName);
                    ChatForm form = chatFormList.Find(chatForm => chatForm.Text.Trim() == message.Trim());
                    if (form != null)
                    {
                        form.ShowNewMsg(showUserInfo);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// close form
        /// </summary>
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            ChatMessage chatMessage = new ChatMessage { type = MessageType.LOGOFF, MyName = this.Text };
            byte[] dgram = Encoding.Unicode.GetBytes(chatMessage.JsonSerialize());
            try
            {
                if (localClient != null)
                {
                    remoteIPE = new IPEndPoint(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtPort.Text));
                    localClient.BeginSend(dgram, dgram.Length,
                        remoteIPE, asyncResult =>
                            {
                                isReceive = false;
                                localClient.Close();
                            }, null);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// double-click the user info to chat
        /// </summary>
        private void listViewOnlineUsers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string otherInfo = listViewOnlineUsers.SelectedItems[0].Text.Trim();
            string[] other = otherInfo.Split('-');
            string otherIPAddress = other[1].Trim();  // get ip and port
            string[] ipAndPort = otherIPAddress.Split(':');
            ChatMessage otherProfile = new ChatMessage { MyName = other[0].Trim(), Address = ipAndPort[0], Port = Convert.ToInt32(ipAndPort[1]) };

            if (otherInfo != note)
            {
                string localPoint = localClient.Client.LocalEndPoint.ToString();
                string[] arr = localPoint.Split(':');
                ChatMessage myProfile = new ChatMessage { MyName = this.Text, Address = arr[0], Port = Convert.ToInt32(arr[1]) };

                ChatForm form = chatFormList.Find(f => f.Text == string.Format("{0}:{1}", myProfile.MyName, otherProfile.MyName));
                if (form != null)
                {
                    form.Activate();
                }
                else
                {
                    ChatForm chat = new ChatForm(otherProfile, myProfile);
                    chatFormList.Add(chat);
                    chat.Show();
                }
            }
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
    }
}
