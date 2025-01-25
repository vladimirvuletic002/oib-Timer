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
            string clientGroup = null;

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            Console.WriteLine("Korisnik koji je pokrenuo klijenta je : " + clientName);

            using (ClientProxy proxy = new ClientProxy(binding, address))
            {
                proxy.TestCommunication();
                WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(windowsIdentity);

                foreach (IdentityReference group in windowsIdentity.Groups)
                {
                    SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                    string name = (sid.Translate(typeof(NTAccount))).ToString();
                    string[] nameParts = name.Split('\\');
                    string groupName = nameParts.Length > 1 ? nameParts[1] : nameParts[0];

                    if (groupName == "Admin" || groupName == "Modifier" || groupName == "Reader")
                    {
                        clientGroup = groupName;
                        break;
                    }
                }

                while (true)
                {
                    string option;
                    bool validOption = false;
                    string timeInput;

                    switch (clientGroup)
                    {
                        case "Admin":
                            Console.WriteLine();
                            Console.WriteLine("1. Startuj tajmer");
                            Console.WriteLine("2. Postavi tajmer");
                            Console.WriteLine("3. Poništi tajmer");
                            Console.WriteLine("4. Očitaj stanje tajmera");
                            Console.WriteLine("X. Izlaz");

                            do
                            {
                                Console.WriteLine("\nIzaberite opciju: ");
                                option = Console.ReadLine();
                                option = option.ToUpper();

                                if (option == "1" || option == "2" || option == "3" || option == "4" || option == "X")
                                {
                                    validOption = true;
                                    switch (option)
                                    {
                                        case "1":
                                            // traziti od korisnika da startuje tajmer ako je postavljen
                                            break;
                                        case "2":
                                            // traziti od korisnika da postavi tajmer
                                            Console.WriteLine("Unesite trajanje tajmera u formatu hh:mm:ss (npr. 00:02:00):");
                                            timeInput = Console.ReadLine();

                                            // postavljanje tajmera
                                            proxy.SetTimer(timeInput);
                                            break;
                                        case "3":
                                            // pitati korisnika da li zeli da ponisti tajmer (y/n)
                                            break;
                                        case "4":
                                            // prikazati trenutno vreme na tajmeru u momentu ocitavanja
                                            Console.WriteLine($"Preostalo vreme: {proxy.AskForTime()}");
                                            break;
                                        case "X":
                                            Console.WriteLine("Izlaz iz programa...");
                                            Environment.Exit(0);
                                            break;
                                    }
                                }
                                else
                                {
                                    validOption = false;
                                    Console.WriteLine("\nNepoznata opcija. Pokušajte ponovo.");
                                }

                            } while (!validOption);

                            break;


                        case "Modifier":
                            Console.WriteLine();
                            Console.WriteLine("1. Postavi tajmer");
                            Console.WriteLine("2. Poništi tajmer");
                            Console.WriteLine("3. Očitaj stanje tajmera");
                            Console.WriteLine("X. Izlaz");

                            do
                            {
                                Console.WriteLine("\nIzaberite opciju: ");
                                option = Console.ReadLine();
                                option = option.ToUpper();

                                if (option == "1" || option == "2" || option == "3" || option == "X")
                                {
                                    validOption = true;
                                    switch (option)
                                    {
                                        case "1":
                                            // traziti od korisnika da postavi tajmer
                                            Console.WriteLine("Unesite trajanje tajmera u formatu hh:mm:ss (npr. 00:02:00):");
                                            timeInput = Console.ReadLine();

                                            // postavljanje tajmera
                                            proxy.SetTimer(timeInput);
                                            break;
                                        case "2":
                                            // pitati korisnika da li zeli da ponisti tajmer (y/n)
                                            break;
                                        case "3":
                                            // prikazati trenutno vreme na tajmeru u momentu ocitavanja
                                            Console.WriteLine($"Preostalo vreme: {proxy.AskForTime()}");
                                            break;
                                        case "X":
                                            Console.WriteLine("Izlaz iz programa...");
                                            Environment.Exit(0);
                                            break;
                                    }
                                }
                                else
                                {
                                    validOption = false;
                                    Console.WriteLine("\nNepoznata opcija. Pokušajte ponovo.");
                                }

                            } while (!validOption);

                            break;

                        case "Reader":
                            Console.WriteLine();
                            Console.WriteLine("1. Očitaj stanje tajmera");
                            Console.WriteLine("X. Izlaz");

                            do
                            {
                                Console.WriteLine("\nIzaberite opciju: ");
                                option = Console.ReadLine();
                                option = option.ToUpper();

                                if (option == "1" || option == "X")
                                {
                                    validOption = true;
                                    switch (option)
                                    {
                                        case "1":
                                            // prikazati trenutno vreme na tajmeru u momentu ocitavanja
                                            Console.WriteLine($"Preostalo vreme: {proxy.AskForTime()}");
                                            break;
                                        case "X":
                                            Console.WriteLine("Izlaz iz programa...");
                                            Environment.Exit(0);
                                            break;
                                    }
                                }
                                else
                                {
                                    validOption = false;
                                    Console.WriteLine("\nNepoznata opcija. Pokušajte ponovo.");
                                }

                            } while (!validOption);

                            break;

                    }
                }
                    

                /*Console.WriteLine("Unesite trajanje tajmera u formatu hh:mm:ss (npr. 00:02:00):");
                timeInput = Console.ReadLine();

                // Postavljanje tajmera
                proxy.SetTimer(timeInput);

                // Pokretanje tajmera
                Console.WriteLine("Pokrećemo tajmer...");
                proxy.StartTimer();

                // Logika za upravljanje tajmerom
                ManageTimer(proxy);*/
            }

            //Console.WriteLine("Kraj programa");
            //Console.ReadLine();
        }
    }
}
