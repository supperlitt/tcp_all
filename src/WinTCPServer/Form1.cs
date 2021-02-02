using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WinTCPServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, 10000));
            server.Listen(1000);

            this.txtResult.AppendText("服务已经启动\r\n");
            Thread t = new Thread(Execute);
            t.IsBackground = true;
            t.Start(server);
        }

        private void Execute(Object obj)
        {
            Socket server = obj as Socket;
            while (true)
            {
                Socket client = server.Accept();

                Thread t = new Thread(SingleExecute);
                t.IsBackground = true;
                t.Start(client);
            }
        }

        private void SingleExecute(object obj)
        {
            Socket client = obj as Socket;
            while (true)
            {
                byte[] buffer_len = new byte[4];
                int current_len = 0;
                do
                {
                    byte[] temp = new byte[buffer_len.Length - current_len];
                    int count = client.Receive(temp);
                    if (count > 0)
                    {
                        Array.Copy(temp, 0, buffer_len, current_len, temp.Length);
                        current_len += count;
                    }
                } while (current_len < buffer_len.Length);

                int len = 0;
                len += (buffer_len[0] << 24);
                len += (buffer_len[1] << 16);
                len += (buffer_len[2] << 8);
                len += (buffer_len[3]);

                byte[] data = new byte[len];
                current_len = 0;
                do
                {
                    byte[] temp = new byte[data.Length - current_len];
                    int count = client.Receive(temp);
                    if (count > 0)
                    {
                        Array.Copy(temp, 0, data, current_len, temp.Length);
                        current_len += count;
                    }
                } while (current_len < data.Length);

                string msg = Encoding.UTF8.GetString(data);
                this.txtResult.AppendText(msg + "\r\n");

                client.Send(Encoding.UTF8.GetBytes(msg + " hahaha"));
            }
        }
    }
}
