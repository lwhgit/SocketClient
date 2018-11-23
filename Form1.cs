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
                logView.Items.Add(">" + input.Text);

                if (!cmdHistoryListView.Items.Contains(input.Text)) {
                    cmdHistoryListView.Items.Add(input.Text);
                }

                input.Text = "";
            }
        }

        private void connectButton_Click(object sender, EventArgs e) {
            if (client.isRunning()) {
                eventLogView.Items.Add("Cannot connect. Please disconnect.");
            } else {
                client.ConnectToServer(ipInput.Text, int.Parse(portInput.Text));
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e) {
            client.Disconnect();
        }

        private void cmdHistoryListView_MouseDoubleClick(object sender, MouseEventArgs e) {
            client.Send((string) cmdHistoryListView.SelectedItem);
        }

        public void OnConnected() {
            eventLogView.Items.Add("Connected.");
        }

        public void OnConnectionFailed() {
            eventLogView.Items.Add("Connection Failed.");
        }

        public void OnDataReceived(byte[] buffer) {
            logView.Items.Add(Encoding.UTF8.GetString(buffer));
        }

        public void OnDisconnected() {
            eventLogView.Items.Add("Disconnected.");
        }
    }
}
