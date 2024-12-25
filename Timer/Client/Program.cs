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
                proxy.TestCommunication();
                Console.WriteLine("Unesite trajanje tajmera u formatu hh:mm:ss (npr. 00:02:00):");
                string timeInput = Console.ReadLine();

                proxy.SetTimer(timeInput);

                // Pokretanje tajmera
                Console.WriteLine("Pokrećemo tajmer...");
                proxy.StartTimer();

                // Čekanje korisničkog unosa za zaustavljanje
                bool timerExpired = false;

                Console.WriteLine("Pritisnite 'R' da resetujete tajmer ili enter da ga zaustavite u suprotnom tajmer se zaustavlja po isteku vremena");
                while (!timerExpired)
                {
                    // Proveravamo preostalo vreme svakih 1 sekundi
                    proxy.DisplayRemainingTime();

                    // Ispisujemo i proveravamo preostalo vreme
                    TimeSpan remainingTime = proxy.GetRemainingTime();

                    // Ako je preostalo vreme 00:00, zaustavljamo tajmer
                    if (remainingTime == TimeSpan.Zero)
                    {
                        Console.WriteLine("Tajmer je istekao");
                        proxy.StopTimer();
                        break; // Izađite iz petlje jer je tajmer istekao
                    }

                    // Provera korisničkog unosa za zaustavljanje tajmera
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Enter)
                        {
                            Console.WriteLine("Tajmer je zaustavljen");
                            proxy.StopTimer();
                            break;
                        }
                        else if (key == ConsoleKey.R) // Pritiskom na 'R' pozivamo reset
                        {
                            Console.WriteLine("Tajmer je resetovan");
                            proxy.ResetTimer();
                            break;
                        }
                    }

                    // Pauza od 1 sekunde
                    Task.Delay(1000).Wait();
                }



            }

            Console.WriteLine("Kraj programa");
            Console.ReadLine();
        }
    }
}
