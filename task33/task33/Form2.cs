using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Message = OpenPop.Mime.Message;

namespace task33
{
    public partial class Form2 : Form
    {
        public Form2(Message m)
        {
            InitializeComponent();
            M = m;
        }

        private Message M;

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = "Content-Id: " + M.Headers.ContentId +
                "\r\nDate: " + M.Headers.Date +
                "\r\nDate: " + M.Headers.DateSent +
                "\r\nSender: " + M.Headers.Sender +
                "\r\nContent-Type: " + M.Headers.ContentType +
                "\r\nFrom: " + M.Headers.From +
                "\r\nSubject: " + M.Headers.Subject;
            textBox2.Text = M.FindFirstPlainTextVersion().GetBodyAsText();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.isanswer = false;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1.isanswer = true;
            this.Close();
        }
    }
}
