using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketClient {
    public partial class Form1 : Form, Communication.ICommunicationEventListener {
        public static Form1 form = null;

        private Communication.Client client = null;

        public Form1() {
            InitializeComponent();

            Form1.form = this;
            client = new Communication.Client();
            client.SetCommunicationEventListener(this);
        }

        private void input_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                client.Send(input.Text);
                input.Text = "";
            }
        }

        private void connectButton_Click(object sender, EventArgs e) {
            client.ConnectToServer(ipInput.Text, int.Parse(portInput.Text));
        }

        public void OnConnected() {

        }

        public void OnDataReceived(byte[] buffer) {
            logView.Items.Add(Encoding.UTF8.GetString(buffer));
        }
    }
}
