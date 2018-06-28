using IndieGoat.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            UniversalClient client = new UniversalClient();
            client.ConnectToRemoteServer("localhost", 7777);
            client.ClientSender.SendCommand("test", client.encryption, new string[] { "test" });
            Console.Read();
        }
    }
}
