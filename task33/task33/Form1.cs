using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Data;
using System.Xml.Serialization;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenPop.Pop3;
using OpenPop.Mime;
using System.IO;
using Message = OpenPop.Mime.Message;

namespace task33
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<Message> myLetters;

        private void Log (string s)
        {
            label5.Text = s;
            label5.Refresh();
        }

        private List<Message> GetMessages(string hostname, int port, string username, string password)
        {
            using (Pop3Client client = new Pop3Client())
            {
                client.Connect(hostname, port, true);
                client.Authenticate(username, password);
                int messageCount = client.GetMessageCount();
                List<Message> allMessages = new List<Message>(messageCount);
                for (int i = messageCount; i > 0; i--)
                {
                    allMessages.Add(client.GetMessage(i));
                }

                client.Disconnect();
                return allMessages;
            }
        }

        private void LoadMessages()
        {
            myLetters.Clear();

        }

        private void SaveMessages()
        {
            Letters data = new Letters();
            data.Data = myLetters;
            /*
            FileStream stream = new FileStream(textBox6.Text, FileMode.Create);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
            stream.Close();*/
/*
            XmlSerializer ser = new XmlSerializer(data.GetType());
            FileStream fs = new FileStream(textBox6.Text, FileMode.Open);
            ser.Serialize(fs, data);
            fs.Close();*/
        }

        private void LoadTable()
        {
            dataGridView1.Rows.Clear();
            foreach (Message M in myLetters)
            {
                dataGridView1.Rows.Add(new object[] { M.Headers.DateSent, M.Headers.ReturnPath, M.Headers.Subject });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox6.Text.Length == 0 || textBox5.Text.Length == 0 || textBox2.Text.Length == 0 || textBox1.Text.Length == 0)
            {
                MessageBox.Show(this, "Заполнены не все поля");
            }
            else
            {
                Log("Получение писем ...");
                label5.Refresh();
                myLetters = GetMessages(textBox1.Text, Convert.ToInt32(textBox2.Text), textBox6.Text, textBox5.Text);
                Log("Получено писем: " + myLetters.Count);
                LoadTable();
                SaveMessages();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox6.Text.Length == 0 || textBox5.Text.Length == 0 || textBox4.Text.Length == 0 || textBox3.Text.Length == 0)
            {
                MessageBox.Show(this, "Заполнены не все поля");
            }
            else
            {
                Form3 sendform = new Form3(textBox6.Text, textBox5.Text, textBox4.Text, textBox3.Text, null);
                sendform.ShowDialog();
            }        
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadMessages();
            LoadTable();
        }

        public static bool isanswer;

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            Form2 letter = new Form2(myLetters[dataGridView1.CurrentRow.Index]);
            letter.ShowDialog();
            if (isanswer)
            {
                string to = myLetters[dataGridView1.CurrentRow.Index].Headers.ReturnPath.ToString();
                Form3 sendform = new Form3(textBox6.Text, textBox5.Text, textBox4.Text, textBox3.Text, to);
                sendform.ShowDialog();
            }
        }
    }

    public class Letters
    {
        public Letters()
        {
        }
/*
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("props", mL, typeof(List<Message>));
        }

        public Letters(SerializationInfo info, StreamingContext context)
        {
            mL = (List<Message>)info.GetValue("props", typeof(List<Message>));
        }*/

        private List<Message> mL;

        public List<Message> Data
        {
            get { return mL; }
            set { mL = value; }
        }
        

    }
}
