using System;
using System.Collections.Specialized;
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

namespace task23
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private byte[] StartMsg, EndMsg;
        private Thread listener;
        private Socket _socket;
        private IPEndPoint _point;
        private IPAddress myip;
        private List<IPAddress> list;

        private static UdpClient udpclient;
        private static Dictionary<IPAddress, int> data;

        private void Form1_Load(object sender, EventArgs e)
        {
            StartMsg = Encoding.ASCII.GetBytes("Hello");
            EndMsg = Encoding.ASCII.GetBytes("Bye");
            data = new Dictionary<IPAddress, int>();
            list = new List<IPAddress>();

            myip = Dns.GetHostByName(Dns.GetHostName()).AddressList[1];
            listBox1.Items.Add(myip);

            udpclient = new UdpClient(11000);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _socket.Bind(new IPEndPoint(myip, 11000));
            _point = new IPEndPoint(IPAddress.Broadcast, 11000);
            _socket.SendTo(StartMsg, _point);

            timer1.Interval = Convert.ToInt32(textBox2.Text);

            listener = new Thread(new ThreadStart(Listener));
            listener.Start();
            timer1.Start();
        }

        private void SetIP(IPAddress ip)
        {
            if (listBox1.InvokeRequired) listBox1.Invoke(new Action<IPAddress>((i) => listBox1.Items.Add(i)), ip);
            else listBox1.Items.Add(ip);
        }

        private void Listener()
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Any, 11000);
            Byte[] temp;
            string pstr, recstr;
            IPAddress ip;

            while (true)
            {
                try
                {
                    temp = udpclient.Receive(ref point);
                    recstr = Encoding.ASCII.GetString(temp);
                    pstr = point.Address.ToString();
                    if (pstr == myip.ToString()) continue;                    
                    ip = IPAddress.Parse(pstr);

                    if (recstr == "Bye") data[ip] = -1;
                    else
                        if (data.ContainsKey(ip)) data[ip] = 6;
                        else
                        {
                            data.Add(ip, 6);
                            SetIP(ip);
                        }
                }
                catch (Exception)
                {
                    continue;
                }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _socket.SendTo(StartMsg, _point);

            for (int i = 1; i < listBox1.Items.Count; i++)
            {
                IPAddress tip = IPAddress.Parse(listBox1.Items[i].ToString());
                if (--data[tip] <= 0)
                {
                    data.Remove(tip);
                    list.Add(tip);
                }
            }

            foreach (IPAddress i in list) listBox1.Items.Remove(i);
            list.Clear();

            textBox1.Text = " Копий запущенно: " + listBox1.Items.Count;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Abort();            
            udpclient.Close();

            foreach (IPAddress ip in listBox1.Items) _socket.SendTo(EndMsg, new IPEndPoint(ip, 11000));
            _socket.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            timer1.Interval = Convert.ToInt32(textBox2.Text);
        }
    }
}
