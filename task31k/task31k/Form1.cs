using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace task31k
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        byte[] PingMsg;
        IPAddress myip;
        UdpClient udpclient;
        Thread listener;
        TcpClient client;
        NetworkStream nstream;
        FileStream fs;
        Socket _socket;
        IPEndPoint _point;

        private void Form1_Load(object sender, EventArgs e)
        {
            PingMsg = Encoding.ASCII.GetBytes("Ping");
            myip = Dns.GetHostByName(Dns.GetHostName()).AddressList[1];
            udpclient = new UdpClient(100);
        }

        private void button1_Click(object sender, EventArgs e)
        {      
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _socket.Bind(new IPEndPoint(myip, 100));
            _point = new IPEndPoint(IPAddress.Broadcast, 100);
            _socket.SendTo(PingMsg, _point);
            _socket.Close();

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
                        textBox4.Text = s;
                        check = false;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("123");
                }
            }
            udpclient.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpclient.Close();
            if (listener != null) listener.Abort();
        }

        private void textBox4_DoubleClick(object sender, EventArgs e)
        {            
            string[] s = textBox4.Text.Split(':');
            textBox1.Text = s[0];
            textBox2.Text = s[1];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Length == 0) return;

            client = new TcpClient();
            try
            {                
                client.Connect(textBox1.Text, Convert.ToInt32(textBox2.Text));
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Сервер недоступен");
                return;
            }

            nstream = client.GetStream();
            fs = new FileStream(textBox3.Text, FileMode.Open, FileAccess.Read);
            FileInfo finfo = new FileInfo(textBox3.Text);

            udpclient = new UdpClient(Convert.ToInt32(textBox2.Text));
            _point = new IPEndPoint(IPAddress.Parse(textBox1.Text), Convert.ToInt32(textBox2.Text));
            string FileName = finfo.Name;
            long FileSize = finfo.Length;
            byte[] fi = new byte[2048];
            fi = System.Text.Encoding.ASCII.GetBytes((FileName + " " + FileSize.ToString()).ToCharArray());
            udpclient.Send(fi, fi.Length, _point);

            int bytesSize = 0;
            byte[] Buffer = new byte[2048];
            while ((bytesSize = fs.Read(Buffer, 0, Buffer.Length)) > 0)
            {
                nstream.Write(Buffer, 0, bytesSize);
            }

            udpclient.Close();
            client.Close();
            nstream.Close();            
            fs.Close();
        }
    }
}
