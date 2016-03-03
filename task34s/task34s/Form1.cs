using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace task34s
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Thread listener;
        Thread talk;
        IPAddress myip;
        UdpClient udpclient;
        private byte[] PingMsg;        
        Thread talkListener;
        TcpListener TCP;
        bool isRunning = false;
        StreamReader srReceiver;
        StreamWriter swSender;

        public static Hashtable Users = new Hashtable(20);        
        public static Hashtable Connections = new Hashtable(20);

        private void Form1_Load(object sender, EventArgs e)
        {
            myip = Dns.GetHostByName(Dns.GetHostName()).AddressList[1];
            textBox1.Text = myip.ToString() + ":1313";
            PingMsg = Encoding.ASCII.GetBytes(textBox1.Text);
            udpclient = new UdpClient(100);
            listener = new Thread(new ThreadStart(Listener));
            listener.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Abort();
            System.Diagnostics.Process.GetCurrentProcess().Kill();            
        }

        private void Log(string str)
        {
            if (textBox4.InvokeRequired) textBox4.Invoke(new Action<string>((s) => textBox4.Text += str + "\r\n"), str);
            else textBox4.Text += str + "\r\n";
        }

        private void Listener()
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Any, 100);
            Byte[] temp;
            string recstr;
            while (true)
            {
                try
                {
                    temp = udpclient.Receive(ref point);
                    recstr = Encoding.ASCII.GetString(temp);
                    if (recstr == "Ping")
                    {
                        udpclient.Send(PingMsg, PingMsg.Length, point);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;                
                TCP = new TcpListener(1313);
                TCP.Start();
                talkListener = new Thread(Listening);
                talkListener.Start();
                textBox4.AppendText("=== Сервер запущен ===\r\n");
                button1.Text = "Выключить";
                listBox1.Items.Add("Chat Master");
            }
            else
            {
                TCP.Stop();
                talkListener.Abort();
                textBox4.AppendText("=== Сервер остановлен ===\r\n");
                isRunning = false;
                listBox1.Items.Clear();
                button1.Text = "Включить";
            }
        }

        private void Listening()
        {
            while (isRunning)
            {
                TcpClient tcpClient = TCP.AcceptTcpClient();
                talk = new Thread(AcceptUser);
                talk.Start(tcpClient);
            }
        }

        private void AddUser(TcpClient tcpUser, string name)
        {
            Users.Add(name, tcpUser);
            Connections.Add(tcpUser, name);
            SendAdminMessage(name + " вошел в чат");
            if (listBox1.Items.IndexOf(name) == -1) listBox1.Items.Add(name);
        }

        private void RemoveUser(TcpClient tcpUser)
        {
            if (Connections[tcpUser] != null)
            {
                SendAdminMessage(Connections[tcpUser] + " покинул чат");
                listBox1.Items.Remove(Connections[tcpUser]);
                Users.Remove(Connections[tcpUser]);
                Connections.Remove(tcpUser);
            }
        }

        private void SendAdminMessage(string Message)
        {
            StreamWriter Sender;

            Log("Chat Master: " + Message);

            TcpClient[] tcpClients = new TcpClient[Users.Count];
            Users.Values.CopyTo(tcpClients, 0);
            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (Message.Trim() == "" || tcpClients[i] == null) continue;
                    Sender = new StreamWriter(tcpClients[i].GetStream());
                    Sender.WriteLine("Chat Master: " + Message);
                    Sender.Flush();
                    Sender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        private void SendMessage(string From, string Message)
        {
            StreamWriter Sender;

            Log(From + ": " + Message);

            TcpClient[] tcpClients = new TcpClient[Users.Count];
            Users.Values.CopyTo(tcpClients, 0);
            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (Message.Trim() == "" || tcpClients[i] == null) continue;
                    Sender = new StreamWriter(tcpClients[i].GetStream());
                    Sender.WriteLine(From + ": " + Message);
                    Sender.Flush();
                    Sender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        private void AcceptUser(object o)
        {
            TcpClient tcpClient = (TcpClient)o;

            srReceiver = new StreamReader(tcpClient.GetStream());
            swSender = new StreamWriter(tcpClient.GetStream());
            string newUser = srReceiver.ReadLine();
            string str;

            if (newUser == "") return;
            if (Users.Contains(newUser))
            {
                swSender.WriteLine("0|== Такое имя занято ===\r\n");
                swSender.Flush();
                tcpClient.Close();
                srReceiver.Close();
                swSender.Close();
                return;
            }
            else
            {
                swSender.WriteLine("1");
                swSender.Flush();
                AddUser(tcpClient, newUser);
            }

            try
            {
                while ((str = srReceiver.ReadLine()) != "")
                {
                    if (str == null) RemoveUser(tcpClient);
                    else SendMessage(newUser, str);
                }
            }
            catch
            {
                RemoveUser(tcpClient);
            }
        }
        
    }
}
