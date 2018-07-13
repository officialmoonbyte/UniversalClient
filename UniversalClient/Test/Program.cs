using IndieGoat.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            UniversalClient client = new UniversalClient();
            client.ConnectToRemoteServer("localhost", 7777);
            Console.WriteLine("Response : " + client.ClientSender.SendCommand("userdatabase", new string[] { "adduser", "braydel", "ritter"}));
            Console.Read();
        }
    }
}
