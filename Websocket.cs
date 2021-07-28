using Fleck;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Websocket
{
    public partial class Websocket : Form
    {
        WebSocketServer server;
        bool enabled = false;

        public Websocket()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            QRCodeGenerator QG = new QRCodeGenerator();
            var MyData = QG.CreateQrCode(GetLocalIPAddress(), QRCodeGenerator.ECCLevel.H);
            var code = new QRCode(MyData);
            picQR.Image = code.GetGraphic(50);

            btnNew_Click(btnNew, new EventArgs());
        }


        public Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            enabled = !enabled;
            btnNew.Text = enabled ? "DESATIVAR" : "ATIVAR";
            btnClear_Click(btnClear, new EventArgs());

            if (!enabled) {
                if(server != null) server.Dispose();
                return;
            }
            
            
            server = new WebSocketServer("ws://0.0.0.0:3000");
            server.Start(socket =>
            {
                //socket.OnOpen = () => Invoke((MethodInvoker)delegate { lblStatus.Text = "Open!\n"; });
                //socket.OnClose = () => Invoke((MethodInvoker)delegate { lblStatus.Text = "Close!\n"; });
                socket.OnMessage = message => Invoke((MethodInvoker)delegate {
                    var payload = message.Split(',')[1];
                    //lblStatus.Text = payload.Length.ToString();
                    picImg.Image = Base64ToImage(payload);
                    btnSave.Enabled = true;
                    socket.Close();
                    //server.Dispose();
                });
                //socket.OnError = error => Invoke((MethodInvoker)delegate { lblStatus.Text = error.Message; });
            });
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            picImg.Image = null;
            picImg.Invalidate();
            btnSave.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Image(*.png)|*.png|JPG Image(*.jpg)|*.jpg|BMP Image(*.bmp)|*.bmp";
            ImageFormat format = ImageFormat.Png;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }
                picImg.Image.Save(sfd.FileName, format);
            }
        }
    }
}
