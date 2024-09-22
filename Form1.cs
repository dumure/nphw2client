using System.Net;
using System.Net.Sockets;
using System.Text;

namespace screenshotclient
{
    public partial class Form1 : Form
    {
        private Socket client;
        private EndPoint connEP;
        const int oneFragmentSize = 10000;
        public Form1()
        {
            InitializeComponent();

            var ip = IPAddress.Loopback;
            var port = 27001;

            client = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            connEP = new IPEndPoint(ip, port);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Thread.Sleep(333);
            byte[] buffer = new byte[sizeof(int)];
            client.SendTo(Encoding.UTF8.GetBytes("SCREENSHOT"), connEP);
            client.ReceiveFrom(buffer, ref connEP);
            int screenshotSize = BitConverter.ToInt32(buffer);
            byte[] screenshotBytes = new byte[screenshotSize];
            int fragmentsCount = (screenshotSize + oneFragmentSize - 1) / oneFragmentSize;
            int offset = 0;
            for (int i = 0; i < fragmentsCount; i++)
            {
                if (i == fragmentsCount - 1)
                {
                    byte[] fragmentBytes = new byte[screenshotSize - offset];
                    client.ReceiveFrom(fragmentBytes, ref connEP);
                    client.SendTo(Encoding.UTF8.GetBytes("Fragment received"), connEP);
                    Array.Copy(fragmentBytes, 0, screenshotBytes, offset, fragmentBytes.Length);
                }
                else
                {
                    byte[] fragmentBytes = new byte[oneFragmentSize];
                    client.ReceiveFrom(fragmentBytes, ref connEP);
                    client.SendTo(Encoding.UTF8.GetBytes("Fragment received"), connEP);
                    Array.Copy(fragmentBytes, 0, screenshotBytes, offset, fragmentBytes.Length);
                    offset += oneFragmentSize;
                }
            }
            pictureBox1.Image = ByteArrayToImage(screenshotBytes, screenshotSize);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            WindowState = FormWindowState.Normal;
        }

        private Image ByteArrayToImage(byte[] bytes, int length)
        {
            using (MemoryStream ms = new MemoryStream(bytes, 0, length))
            {
                return Image.FromStream(ms);
            }
        }
    }
}
