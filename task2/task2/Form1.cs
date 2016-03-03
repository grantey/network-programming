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

namespace task2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private byte[] StartMsg, EndMsg;        
        private Thread listener;
        private static Socket _socket;
        private static EndPoint _point;
        private static byte[] temp;
        private static int delay;

        private void Form1_Load(object sender, EventArgs e)
        {
            StartMsg = Encoding.ASCII.GetBytes("Hello");
            EndMsg = Encoding.ASCII.GetBytes("Bye");
            temp = new Byte[10];
            
            IPAddress myip = Dns.GetHostByName(Dns.GetHostName()).AddressList[0];
            listBox1.Items.Add(myip);
           
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            socket.Bind(new IPEndPoint(myip, 11000));
            IPAddress p = IPAddress.Broadcast;
            socket.SendTo(StartMsg, new IPEndPoint(IPAddress.Broadcast, 11000));
            socket.Close();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.ReceiveTimeout = 500;

            IPEndPoint endp = new IPEndPoint(IPAddress.Any, 11000);
            _socket.Bind(endp);
            _point = (EndPoint)endp;

            listener = new Thread(new ThreadStart(Listener));
            listener.Start();
        }

        private void SetIP(IPAddress ip)
        {
            if (listBox1.InvokeRequired) listBox1.Invoke(new Action<IPAddress>((i) => listBox1.Items.Add(i)), ip);
            else listBox1.Items.Add(ip);
        }

        private void RemoveIP(IPAddress ip)
        {
            if (listBox1.InvokeRequired) listBox1.Invoke(new Action<IPAddress>((i) => listBox1.Items.Remove(i)), ip);
            else listBox1.Items.Remove(ip);
        }

        private void Listener()
        {
            while (true)
            {
                int length;

                try
                {                    
                    length = _socket.ReceiveFrom(temp, ref _point);
                }
                catch (Exception)
                {
                    continue;
                }

                string pointstr = _point.ToString();
                IPAddress ip = IPAddress.Parse(pointstr.Substring(0, pointstr.IndexOf(':')));

                if (length == 3) RemoveIP(ip);
                else
                {
                    SetIP(ip);

                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.SendTo(StartMsg, _point);
                    socket.Close();
                }

                textBox1.Text = " Копий запущенно: " + listBox1.Items.Count;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Abort();
            _socket.Close();
            
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            foreach (IPAddress ip in listBox1.Items) _socket.SendTo(EndMsg, new IPEndPoint(ip, 11000));

            _socket.Close();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
