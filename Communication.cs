using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient {
    public class Communication {

        public interface ICommunicationEventListener {
            void OnConnected();
            void OnConnectionFailed();
            void OnDataReceived(byte[] buffer);
            void OnDisconnected();
        }

        public class Client {
            private ICommunicationEventListener eventListener = null;

            private bool stop = false;

            private Socket socket = null;
            private string ip = "127.0.0.1";
            private int port = 8762;

            private byte[] receiveBuffer = null;
            private string receiveString = null;

            private Queue<byte[]> queueForSend = null;
            private Queue<byte[]> queueForReceive = null;

            private Thread connectThread = null;
            private Thread dataSendThread = null;
            private Thread dataReceiveThread = null;

            public Client() {
                queueForSend = new Queue<byte[]>();
                queueForReceive = new Queue<byte[]>();
            }

            public void SetCommunicationEventListener(ICommunicationEventListener listener) {
                eventListener = listener;
            }

            public bool isRunning() {
                if (connectThread == null || dataSendThread == null || dataReceiveThread == null) {
                    return false;
                }

                return connectThread.IsAlive || dataSendThread.IsAlive || dataReceiveThread.IsAlive;
            }

            public void ConnectToServer(string ip, int port) {
                this.ip = ip;
                this.port = port;
                
                connectThread = new Thread(ConnectThread);
                connectThread.IsBackground = true;
                dataSendThread = new Thread(DataSendThread);
                dataSendThread.IsBackground = true;
                dataReceiveThread = new Thread(DataReceiveThread);
                dataReceiveThread.IsBackground = true;

                connectThread.Start();
            }

            public void Disconnect() {
                Thread disconnectThread = new Thread(DisconnectThread);
                disconnectThread.Start();

            }

            public void Send(string data) {
                queueForSend.Enqueue(Encoding.UTF8.GetBytes(data));
            }

            private void ConnectThread() {
                try {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                    Form1.form.Invoke(new Action(delegate () {
                        eventListener.OnConnected();
                    }));

                    dataSendThread.Start();
                    dataReceiveThread.Start();
                } catch (Exception e) {
                    Form1.form.Invoke(new Action(delegate () {
                        eventListener.OnConnectionFailed();
                    }));
                    Disconnect();
                }
            }

            private void DataSendThread() {
                while (true) {
                    try {
                        if (queueForSend.Count > 0) {
                            socket.Send(queueForSend.Dequeue());
                        }
                        if (stop) {
                            break;
                        }
                        Thread.Sleep(10);
                    } catch (Exception e) {
                        
                    }
                }
            }

            private void DataReceiveThread() {
                while (true) {
                    try {
                        if (socket.Available > 0) {
                            receiveBuffer = new byte[socket.Available];
                            socket.Receive(receiveBuffer);
                            receiveString = Encoding.UTF8.GetString(receiveBuffer);
                        
                            Form1.form.Invoke(new Action(delegate () {
                                eventListener.OnDataReceived(receiveBuffer);
                            }));
                        }
                        if (stop) {
                            break;
                        }
                        Thread.Sleep(10);
                    } catch (Exception e) {

                    }
                }
            }

            private void DisconnectThread() {

                stop = true;
                if (connectThread != null && connectThread.IsAlive)
                    connectThread.Join();
                if (dataSendThread != null && dataReceiveThread.IsAlive)
                    dataSendThread.Join();
                if (dataReceiveThread != null && dataReceiveThread.IsAlive)
                    dataReceiveThread.Join();
                stop = false;

                socket.Close();
                Form1.form.Invoke(new Action(delegate () {
                    eventListener.OnDisconnected();
                }));
            }
        }
    }
}
