using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace task34k
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        StreamWriter Sender;
        StreamReader Receiver;
        TcpClient tcpServer;
        Thread talk;
        IPAddress serverip;
        IPAddress myip;
        string myname;
        bool isConnected;
        UdpClient udpclient;
        byte[] PingMsg;
        Thread listener;

        private void Form1_Load(object sender, EventArgs e)
        {
            udpclient = new UdpClient(100);
            PingMsg = Encoding.ASCII.GetBytes("Ping");
            myip = Dns.GetHostByName(Dns.GetHostName()).AddressList[1];
            myname = textBox2.Text;

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            socket.Bind(new IPEndPoint(myip, 100));
            IPEndPoint point = new IPEndPoint(IPAddress.Broadcast, 100);
            socket.SendTo(PingMsg, point);
            socket.Close();

            listener = new Thread(new ThreadStart(Listener));
            listener.Start();    
        }

        private void Listener()
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Any, 100);
            Byte[] temp;
            bool check = true;
            string s;

            while (check)
            {
                try
                {
                    temp = udpclient.Receive(ref point);
                    if ((s = Encoding.ASCII.GetString(temp)) != "Ping")
                    {
                        textBox1.Text = s;
                        check = false;
                    }
                }
                catch (Exception)
                {
                }
            }
            udpclient.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Abort();
            udpclient.Close();
            if (isConnected == true)
            {
                talk.Abort();
                isConnected = false;
                Sender.Close();
                Receiver.Close();
                tcpServer.Close();
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        private void Log(string str)
        {
            if (textBox4.InvokeRequired) textBox4.Invoke(new Action<string>((s) => textBox4.Text += str + "\r\n"), str);
            else textBox4.Text += str + "\r\n";
        }

        private void CloseConnection(string s)
        {
            Log(s);
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            isConnected = false;
            talk.Abort();
            Sender.Close();
            Receiver.Close();
            tcpServer.Close();
        }

        private void ReceiveMessages()
        {
            Receiver = new StreamReader(tcpServer.GetStream());
            string ConResponse = Receiver.ReadLine();
            if (ConResponse[0] == '1')
            {
                Log("=== Вы вошли в чат ===");
            }
            else 
            {
                CloseConnection("=== Не удалось войти в чат ===\r\n");
                return;
            }
            while (isConnected)
            {
                string rs = Receiver.ReadLine();
                Log(rs);
                string from = rs.Substring(0, rs.IndexOf(':'));
                if (listBox1.Items.IndexOf(from) == -1) listBox1.Items.Add(from);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tcpServer = new TcpClient();
            if (textBox1.Text.IndexOf(':') > 0)
            {
                string[] ss = textBox1.Text.Split(':');
                serverip = IPAddress.Parse(ss[0]);
                tcpServer.Connect(serverip, Convert.ToInt32(ss[1]));
            }
            else tcpServer.Connect(IPAddress.Parse(textBox1.Text), 1313);
            
            isConnected = true;
            myname = textBox2.Text;
            listBox1.Items.Add(myname);

            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;

            Sender = new StreamWriter(tcpServer.GetStream());
            Sender.WriteLine(myname);
            Sender.Flush();

            talk = new Thread(new ThreadStart(ReceiveMessages));
            talk.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseConnection("=== Вы покинули чат ===");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Length >= 1)
            {
                Sender.WriteLine(textBox3.Text);
                Sender.Flush();
                textBox3.Text = "";
            }
        }
    }
}
