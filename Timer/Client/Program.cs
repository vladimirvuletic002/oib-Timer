using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Manager;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/TimerService";
            string clientName = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            Console.WriteLine("Korisnik koji je pokrenuo klijenta je : " + clientName);

            using (ClientProxy proxy = new ClientProxy(binding, address))
            {
                proxy.AddUser("pera", "peric");
                //proxy.AddUser("pera", "peric");
            }

            Console.ReadLine();
        }
    }
}
