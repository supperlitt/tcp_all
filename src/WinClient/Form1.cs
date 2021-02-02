using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace WinClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse("192.168.0.105"), 10000));

            byte[] data = Encoding.UTF8.GetBytes("hello i'm client");
            int len = data.Length;
            byte[] data_len = new byte[4];
            data_len[0] = (byte)((len >> 8 * 3) & 0xFF);
            data_len[1] = (byte)((len >> 8 * 2) & 0xFF);
            data_len[2] = (byte)((len >> 8 * 1) & 0xFF);
            data_len[3] = (byte)(len & 0xFF);

            byte[] data_all = new byte[len + 4];
            Array.Copy(data_len, 0, data_all, 0, data_len.Length);
            Array.Copy(data, 0, data_all, 4, data.Length);

            socket.Send(data_all);

            byte[] buffer = new byte[1024];
            int count = socket.Receive(buffer, SocketFlags.None);
            if (count > 0)
            {
                string msg = Encoding.UTF8.GetString(buffer, 0, count);

                this.Text = msg;
            }
        }
    }
}
