using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Manager;
using System.IdentityModel.Policy;

namespace TimerService
{
    public class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/TimerService";
            string username = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            // Kreiramo ServiceHost
            ServiceHost host = new ServiceHost(typeof(TimerService));

            // Pre nego što pozovemo host.Open(), postavljamo autorizaciju
            host.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();
            host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;

            // Podesavamo spoljnu autorizacionu politiku
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add(new CustomAuthorizationPolicy());
            host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();

            // Dodajemo servisni endpoint
            host.AddServiceEndpoint(typeof(ITimerService), binding, address);

            // Otvaramo servis
            host.Open();

            Console.WriteLine("Korisnik koji je pokrenuo servera :" + username);
            Console.WriteLine("Servis je pokrenut.");
            Console.ReadLine();
        }
    }
}
