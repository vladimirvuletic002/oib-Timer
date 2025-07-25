using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.ServiceModel;
using Contracts;

namespace Client
{
    public class ClientProxy : ChannelFactory<ITimerService>, ITimerService, IDisposable
    {
        ITimerService factory;

        private static readonly byte[] key = Encoding.UTF8.GetBytes("OvoJeVrloTajniKljuc12345");

        public ClientProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            Console.WriteLine("Creating channel...");
            factory = this.CreateChannel();
            Console.WriteLine("Channel created.");
        }

        public void TestCommunication()
        {
            try
            {
                Console.WriteLine("Calling TestCommunication...");
                factory.TestCommunication();
            }
            catch (Exception e)
            {
                Console.WriteLine("[TestCommunication] ERROR = {0}", e.Message);
            }
        }

        public void ResetTimer()
        {
            try
            {
                factory.ResetTimer();
                Console.WriteLine("Timer je uspešno resetovan!");
            }
            catch
            {
                Console.WriteLine("Pristup Odbijen: Nemate dozvolu za akciju pokretanja tajmera!");
            }
        }


        public void SetTimer(string time)
        {
            try
            {
                //byte[] key = Encoding.UTF8.GetBytes("OvoJeVrloTajniKljuc12345");

                //if (key.Length != 24)
                //{
                    //Array.Resize(ref key, 24);
                //}

                string encryptedTime = Encrypt3DES(time);

                factory.SetTimer(encryptedTime);
                if (TimeSpan.TryParse(time, out TimeSpan duration) && duration > TimeSpan.Zero)
                {
                    DateTime endTime = DateTime.Now.Add(duration);
                    Console.WriteLine($"Tajmer je postavljen na {duration}.");
                }
            }
            catch 
            {
                Console.WriteLine("Pristup Odbijen: Nemate dozvolu za akciju setovanja tajmera!");
            }
        }

        public void StartTimer()
        {
            try
            {
                if (!IsTimerActive())
                {
                    factory.StartTimer();
                    if (!IsTimerSet())
                    {
                        Console.WriteLine("Tajmer nije postavljen. Koristite SetTimer pre StartTimer.");
                    }
                    else
                    {
                        Console.WriteLine("Timer je uspešno pokrenut!");
                    }

                }
                else
                {
                    Console.WriteLine("Timer je vec pokrenut!");
                }
            }
            catch 
            {
                Console.WriteLine("Pristup Odbijen: Nemate dozvolu za akciju pokretanja tajmera!");
            }
        }


        public void StopTimer()
        {
            try
            {
                factory.StopTimer();
                Console.WriteLine("Timer je zaustavljen!");
            }
            catch 
            {
                Console.WriteLine("Pristup Odbijen: Nemate dozvolu za akciju zaustavljanja tajmera!");
            }
        }

        public string AskForTime()
        {
            try
            {
                string remainingTime = factory.GetRemainingTime().ToString(@"hh\:mm\:ss");
                return remainingTime;
            }
            catch
            {
                return "Timer is not started!";
            }

        }
        public bool IsTimerExpired()
        {
            try
            {
                return factory.IsTimerExpired();
            }
            catch
            {
                return false;
            }
        }
        public void DisplayRemainingTime()
        {
            try
            {
                TimeSpan remainingTime = factory.GetRemainingTime();

                if (remainingTime > TimeSpan.Zero)
                {
                    Console.WriteLine($"Preostalo vreme: {remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}");
                }
                else
                {
                    Console.WriteLine("Tajmer je istekao.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Greška pri dobijanju preostalog vremena: " + e.Message);
            }
        }

        public static string Encrypt3DES(string plainText)
        {
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = tdes.CreateEncryptor();
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public void Dispose()
        {
            if (factory != null)
            {
                factory = null;
            }

            this.Close();
        }

        public TimeSpan GetRemainingTime()
        {
            return factory.GetRemainingTime();
        }

        public bool IsTimerActive()
        {
            return factory.IsTimerActive();
        }

        public bool IsTimerSet()
        {
            return factory.IsTimerSet();
        }
    }
}
