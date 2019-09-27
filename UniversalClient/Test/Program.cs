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
            string Command = "getTime";
            string[] Args = new string[] { "yyyyMMddHHmmss" };
            universalClient client = new universalClient();
            client.ConnectToRemoteServer("localhost", 7777);
            Console.WriteLine("Response : " + client.ClientSender.SendCommand(Command, Args ));
            Console.Read();
        }
    }
}
