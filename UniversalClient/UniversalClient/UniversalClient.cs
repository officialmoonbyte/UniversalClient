using Indiegoat.Encryption;
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
        public Encryption localEncryption;

        #endregion

        #region OnStartup

        /// <summary>
        /// Initializes all of the local vars
        /// </summary>
        public UniversalClient()
        {
            Client = new TcpClient();
            ClientSender = new prvClientSender();
            localEncryption = new Encryption();
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
            //Checks if client is null then connect to the server
            if (Client == null) Console.WriteLine("[UniversalClient] TCP Client is currently set to null!");
            Client.Connect(serverObject.IP, serverObject.port);

            //Sets the client sender tcp client
            ClientSender.client = Client;
            ClientSender._localEncryption = localEncryption;

            Console.WriteLine("Getting server encryption key...");

            string ServerPublicKey = ClientSender.WaitForResult(false);
            localEncryption.SetKey(ServerPublicKey);

            Console.WriteLine("Got server encryption key.");

            Console.WriteLine(localEncryption.GetKey());

            Console.WriteLine("Sending client key");

            string KeyPackage = localEncryption.GetClientEncryptionKey();
            ClientSender.SendMessage(localEncryption.Encrypt(KeyPackage));

            Console.WriteLine("Sent client key.. Waiting for server to be ready");
            string readyMessage = ClientSender.WaitForResult(false);

            if (readyMessage == "ready") Console.WriteLine("Server is ready!");
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
            public Encryption _localEncryption;

            #endregion

            #region Send information

            /// <summary>
            /// Sends a message to the server with formating for a command
            /// </summary>
            public string SendCommand(string Command, string[] args)
            {
                string valueToSend = "CLNT|" + Command + " " + string.Join(" ", args);
                SendMessage(valueToSend);
                return WaitForResult();
            }

            /// <summary>
            /// Sends a message to the server without formating for a command
            /// </summary>
            public void SendMessage(string Value)
            {
                //Sends the message to the client
                string stringToSend = _localEncryption.Encrypt(Value.Replace(" ", "%20%"), true);
                client.Client.Send(Encoding.UTF8.GetBytes(stringToSend));
            }
            #endregion

            #region WaitForResult

            /// <summary>
            /// Wait for the server to respond
            /// </summary>
            /// <returns>the string the server responds with</returns>
            public string WaitForResult(bool Decypher = true)
            {
                Console.WriteLine("Receiving Server Data! Please wait...");
                byte[] data = new byte[client.Client.ReceiveBufferSize];
                int receivedDataLength = client.Client.Receive(data);
                string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
                if (Decypher)
                {
                    stringData = _localEncryption.Decrypt(stringData);
                }
                Console.WriteLine("Server response: " + stringData);
                return stringData.Replace("%20%", " ");
            }

            #endregion

        }

        #endregion

    }
}
