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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace task31s
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private byte[] PingMsg;
        private IPAddress myip;
        private UdpClient udpclient, udpc;
        private TcpListener tcpl;
        private Thread listener, receiver;
        private Stream stream;
        private NetworkStream nstream;
        private delegate void UpdateProgressCallback(Int64 BytesRead, Int64 TotalBytes);

        private void Form1_Load(object sender, EventArgs e)
        {         
            myip = Dns.GetHostByName(Dns.GetHostName()).AddressList[1];            
            textBox2.Text = myip.ToString() + ":11000";
            PingMsg = Encoding.ASCII.GetBytes(textBox2.Text);
            udpclient = new UdpClient(100);                        

            listener = new Thread(new ThreadStart(Listener));
            listener.Start();
            receiver = new Thread(new ThreadStart(Receiver));
            receiver.Start();
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
        private string CheckName(string name)
        {
            bool checkname;
            int addname;
            string path;
            if (textBox3.Text.Length > 0) path = textBox3.Text;
            else path = Path.GetFullPath(@".\");
            string[] files = Directory.GetFiles(path);
            addname = 1;
            checkname = false;
            while (!checkname)
            {
                checkname = true;
                foreach (string s in files)
                {
                    if (new FileInfo(s).Name == name)
                    {
                        if (name.IndexOf('_') < 0) name = Path.GetFileNameWithoutExtension(name) + "_" + addname + Path.GetExtension(name);
                        else name = name.Substring(0, name.LastIndexOf('_') + 1) + addname + Path.GetExtension(name);
                        addname++;
                        checkname = false;
                        break;
                    }
                }
            }

            return name;
        }


        private void Receiver()
        {
            try
            {
                tcpl = new TcpListener(myip, 11000);
                tcpl.Start();
                udpc = new UdpClient(11000);
                IPEndPoint endp = new IPEndPoint(IPAddress.Any, 11000);

                TcpClient tcpk = tcpl.AcceptTcpClient();
                nstream = tcpk.GetStream();
                byte[] Buffer = new byte[2048];

                Buffer = udpc.Receive(ref endp);
                int bytesSize = Buffer.Length;

                string fi = System.Text.Encoding.ASCII.GetString(Buffer, 0, bytesSize);
                string FileName = CheckName(fi.Split(' ')[0]);
                long FileSize = Convert.ToInt64(fi.Split(' ')[1]);

                stream = new FileStream(FileName, FileMode.Create);
                   
                while ((bytesSize = nstream.Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    stream.Write(Buffer, 0, bytesSize);
                    this.Invoke(new UpdateProgressCallback(this.UpdateProgress), new object[] { stream.Length, FileSize });
                }

                textBox1.Text += DateTime.Now.ToString() + " .... " + endp.ToString() + " .... " + FileName + " .... " + (new FileInfo(FileName)).Length.ToString() + "б\r\n";
            }
            finally
            {
                if (nstream != null) nstream.Close();
                if (stream != null) stream.Close();
                udpc.Close();
                tcpl.Stop();
                Receiver(); 
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            udpclient.Close();
            if (nstream != null) nstream.Close();
            if (stream != null) stream.Close();
            receiver.Abort();
            listener.Abort();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) textBox3.Text = folderBrowserDialog1.SelectedPath;
        }

        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            if (TotalBytes > 0)
            {
                progressBar1.Value = Convert.ToInt32((BytesRead * 100) / TotalBytes);
            }
        }
    }
}
