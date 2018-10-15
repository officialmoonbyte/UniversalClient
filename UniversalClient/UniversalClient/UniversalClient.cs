
using IndieGoat.Cryptography.UniversalTCPEncryption;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IndieGoat.Net.Tcp
{

    /// <summary>
    /// Used to connect to a server through a object
    /// </summary>
    public class UniversalConnectionObject
    {
        public int port; public string IP;
        public UniversalConnectionObject(string IPADDRESS, int PORT)
        {
            IP = IPADDRESS; port = PORT;
        }
    }

    /// <summary>
    /// A TCP client used to connect to Universal Server
    /// </summary>
    public class UniversalClient
    {

        #region Vars

        public TcpClient Client;
        bool IsConnected { get
            { try { if (Client.Connected) { return true; }
                else { return false; } } catch { return false; } }}
        public prvClientSender ClientSender;
        public Encryption encryption;

        #endregion

        #region OnStartup

        /// <summary>
        /// Initializes all of the local vars
        /// </summary>
        public UniversalClient()
        {
            Client = new TcpClient();
            ClientSender = new prvClientSender();
        }

        #endregion

        #region Connection

        /// <summary>
        /// Connects to a universal server using IP and PORT number
        /// </summary>
        public void ConnectToRemoteServer(string InternetProtocolAddress, int Port)
        {
            ConnectToRemoteServer(new UniversalConnectionObject(InternetProtocolAddress, Port));
        }
        /// <summary>
        /// Connect to a universal server using IPAddress object and PortNumber
        /// </summary>
        public void ConnectToRemoteServer(IPAddress InternetProtocolAddress, int Port)
        {
            ConnectToRemoteServer(new UniversalConnectionObject(InternetProtocolAddress.ToString(), Port));
        }
        /// <summary>
        /// Connects to a universal server using serverObject.
        /// </summary>
        public void ConnectToRemoteServer(UniversalConnectionObject serverObject)
        {
            encryption = new Encryption(false);
            ClientSender.encryption = encryption;

            //Checks if client is null then connect to the server
            if (Client == null) Console.WriteLine("[UniversalClient] TCP Client is currently set to null!");
            Client.Connect(serverObject.IP, serverObject.port);

            //Sets the client sender tcp client
            ClientSender.client = Client;

            ClientSender.SendMessage("READYYY", false);

            string ServerPublicKey = ClientSender.WaitForResult(false);
            encryption.SetServerPublicKey(ServerPublicKey);

            string EncryptedPublicKey = encryption.Encrypt(encryption.GetClientPublicKey(), encryption.GetServerPublicKey());
            string EncryptedPrivateKey = encryption.Encrypt(encryption.GetClientPrivateKey(), encryption.GetServerPublicKey());

            ClientSender.SendMessage(EncryptedPublicKey, false);

            ClientSender.WaitForResult(false); //Gets the ready command

            ClientSender.SendMessage(EncryptedPrivateKey, false);

            string RawServerPrivateKey = ClientSender.WaitForResult(false);
            string ServerPrivateKey = encryption.Decrypt(RawServerPrivateKey, encryption.GetClientPrivateKey());
            encryption.SetServerPrivateKey(ServerPrivateKey);

            ClientSender.SendMessage("READY", false);
            ClientSender.WaitForResult(false); //Gets the ready command
        }

        #endregion

        #region Disconnect

        public void Disconnect()
        {
            if (Client.Connected) Client.Close();
        }

        #endregion

        #region Client Sender

        /// <summary>
        /// Used to simplify sending information to a client
        /// </summary>
        public class prvClientSender
        {
            #region Vars

            public TcpClient client;
            public Encryption encryption;

            #endregion

            #region Send information

            /// <summary>
            /// Sends a message to the server with formating for a command
            /// </summary>
            public string SendCommand(string Command, string[] args)
            {
                for(int i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Replace(" ", "%40%");
                }
                string ArgsSend = string.Join(" ", args);
                Console.WriteLine("Args Send : " + ArgsSend);
                string valueToSend = "CLNT|" + Command + " " + ArgsSend;
                SendMessage(valueToSend);
                return WaitForResult();
            }

            /// <summary>
            /// Sends a message to the server without formating for a command
            /// </summary>
            public void SendMessage(string Value, bool UseEncryption = true)
            {
                //Sends the message to the client
                string stringToSend = Value.Replace(" ", "%20%");
                if (UseEncryption) stringToSend = encryption.Encrypt(stringToSend, encryption.GetClientPrivateKey());
                Console.WriteLine("Sending " + stringToSend);
                byte[] BytesToSend = Encoding.UTF8.GetBytes(stringToSend);
                client.Client.BeginSend(BytesToSend, 0, BytesToSend.Length, 0, new AsyncCallback(SendCallBack), client);
            }

            private void SendCallBack(IAsyncResult ar)
            {
                if (ar.IsCompleted)
                {
                    Console.WriteLine("Data sent sucessfully!");
                }
                else
                {
                    Console.WriteLine("Data was not sucessfully!");
                }
            }

            #endregion

            #region WaitForResult

            /// <summary>
            /// Wait for the server to respond
            /// </summary>
            /// <returns>the string the server responds with</returns>
            public string WaitForResult(bool UseEncryption = true)
            {
                Console.WriteLine("Receiving Server Data! Please wait...");
                byte[] data = new byte[client.Client.ReceiveBufferSize];
                int receivedDataLength = client.Client.Receive(data);
                string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
                Console.WriteLine("Server response: " + stringData);
                string Final = stringData.Replace("%20%", " ");
                if (UseEncryption)
                {
                    Final = encryption.Decrypt(Final, encryption.GetClientPrivateKey());
                }
                return Final;
            }

            #endregion

        }

        #endregion

    }
}
