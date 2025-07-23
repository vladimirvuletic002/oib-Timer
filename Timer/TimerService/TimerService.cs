using Contracts;
using Manager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace TimerService
{
    public class TimerService : ITimerService
    {

        private DispatcherTimer dispatcherTimer; // Tajmer za praćenje intervala
        private DateTime endTime; // Vreme završetka tajmera
        private bool isTimerSet; // Oznaka da li je tajmer postavljen
        private TimeSpan timerDuration;
        private bool isActive;

        public TimerService()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += OnTimerTick; // Event handler za Tick događaj
            isTimerSet = false; // Tajmer nije postavljen prilikom inicijalizacije
            isActive = false;
        }

        public void TestCommunication()
        {
            Console.WriteLine("Communication established.");

            IIdentity identity = Thread.CurrentPrincipal.Identity;

            Console.WriteLine("Tip autentifikacije : " + identity.AuthenticationType);

            WindowsIdentity windowsIdentity = identity as WindowsIdentity;

            try
            {
                Audit.AuthenticationSuccess(windowsIdentity.Name);   // user name
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Ime klijenta koji je je uspostavio komunikaciju : " + windowsIdentity.Name);
            Console.WriteLine("Jedinstveni identifikator : " + windowsIdentity.User);

            Console.WriteLine("Grupe korisnika:");
            foreach (IdentityReference group in windowsIdentity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                string name = (sid.Translate(typeof(NTAccount))).ToString();
                Console.WriteLine(name);
            }
        }

        //[PrincipalPermission(SecurityAction.Demand, Role = "StartStop")]
        public void StartTimer()
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            string userName = Formatter.ParseName(principal.Identity.Name);

            if (Thread.CurrentPrincipal.IsInRole("StartStop"))
            {
                if (!isTimerSet)
                {
                    Console.WriteLine("Tajmer nije postavljen. Koristite SetTimer pre StartTimer.");
                    return;
                }

                if (dispatcherTimer.IsEnabled)
                {
                    Console.WriteLine("Tajmer je već pokrenut.");
                    return;
                }

                dispatcherTimer.Interval = TimeSpan.FromSeconds(1); // Tick interval
                dispatcherTimer.Start();

                isActive = true;

                endTime = DateTime.Now.Add(timerDuration);

                if (dispatcherTimer.IsEnabled)
                {
                    Console.WriteLine($"Tajmer je pokrenut. Završava se u {endTime}.");
                }
                else
                {
                    Console.WriteLine("Greška prilikom pokretanja tajmera.");
                }

                try
                {
                    Audit.AuthorizationSuccess(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action, "Start Timer method need StartStop permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                throw new FaultException("User " + userName +
                    " try to call Start Timer method. Start Timer method need StartStop permission.");
            }
                
        }

        //[PrincipalPermission(SecurityAction.Demand, Role = "StartStop")]
        public void StopTimer()
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            string userName = Formatter.ParseName(principal.Identity.Name);


            if (Thread.CurrentPrincipal.IsInRole("StartStop"))
            { 
                if (!dispatcherTimer.IsEnabled)
                {
                    Console.WriteLine("Tajmer nije aktivan.");
                    return;
                }

            
                dispatcherTimer.Stop();
            
                //isTimerSet = false; // Resetujemo postavku tajmera
                timerDuration = GetRemainingTime();

                string duration = timerDuration.ToString(@"hh\:mm\:ss");

                byte[] key = Encoding.UTF8.GetBytes("OvoJeVrloTajniKljuc1234");

                if (key.Length != 24)
                {
                    Array.Resize(ref key, 24);
                }

                string encryptedTime = Encrypt3DES(duration, key);

                //SetTimer(duration);
                SetTimer(encryptedTime);
                isActive = false;
                Console.WriteLine("Tajmer je zaustavljen.");

                try
                {
                    Audit.AuthorizationSuccess(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action, "Stop Timer method need StartStop permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                throw new FaultException("User " + userName +
                    " try to call Stop Timer method. Stop Timer method need StartStop permission.");
            }
        }

        //[PrincipalPermission(SecurityAction.Demand, Role = "Change")]
        public void ResetTimer()
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            string userName = Formatter.ParseName(principal.Identity.Name);


            if (Thread.CurrentPrincipal.IsInRole("Change"))
            {
                if (!isTimerSet)
                {
                    Console.WriteLine("Tajmer nije postavljen.");
                    return;
                }

                // Zaustavljamo tajmer
                dispatcherTimer.Stop();
                isTimerSet = false; // Resetujemo status da nije postavljen
                endTime = DateTime.MinValue; // Postavljamo "nulu" za vreme završetka

                Console.WriteLine($"Tajmer je resetovan. Trenutno vreme završetka: {endTime}");

                try
                {
                    Audit.AuthorizationSuccess(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action, "Reset Timer method need Change permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                throw new FaultException("User " + userName +
                    " try to call Reset Timer method. Reset Timer method need Change permission.");
            }
        }

        public static string Encrypt3DES(string plainText, byte[] key)
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

        public static string Decrypt3DES(string encryptedText, byte[] key)
        {
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = tdes.CreateDecryptor();
                byte[] inputBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        //[PrincipalPermission(SecurityAction.Demand, Role = "Change")]
        public void SetTimer(string encryptedTime)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            string userName = Formatter.ParseName(principal.Identity.Name);


            if (Thread.CurrentPrincipal.IsInRole("Change"))
            {
                try
                {
                    byte[] key = Encoding.UTF8.GetBytes("OvoJeVrloTajniKljuc1234");

                    if (key.Length != 24)
                    {
                        Array.Resize(ref key, 24);
                    }

                    //string encryptedTime = Encrypt3DES(inputTime, key);
                    Console.WriteLine($"Enkodovano vreme: {encryptedTime}");

                    string decryptedTime = Decrypt3DES(encryptedTime, key);

                    if (TimeSpan.TryParse(decryptedTime, out TimeSpan duration) && duration > TimeSpan.Zero)
                    {
                        //endTime = DateTime.Now.Add(duration);
                        timerDuration = duration;
                        isTimerSet = true;
                        Console.WriteLine($"Tajmer je postavljen na {duration}.");
                    }
                    else
                    {
                        isTimerSet = false;
                        Console.WriteLine("Uneta vrednost za tajmer nije validna.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška pri postavljanju tajmera: {ex.Message}");
                }

                try
                {
                    Audit.AuthorizationSuccess(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action, "Set Timer method need Change permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                throw new FaultException("User " + userName +
                    " try to call Set Timer method. Set Timer method need Change permission.");
            }
        }



        //[PrincipalPermission(SecurityAction.Demand, Role = "See")]
        public string AskForTime()
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            string userName = Formatter.ParseName(principal.Identity.Name);


            if (Thread.CurrentPrincipal.IsInRole("See"))
            {
                try
                {
                    Audit.AuthorizationSuccess(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return GetRemainingTime().ToString();
            }
            else
            {
                try
                {
                    Audit.AuthorizationFailed(userName,
                        OperationContext.Current.IncomingMessageHeaders.Action, "AskForTime method need See permission.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                throw new FaultException("User " + userName +
                    " try to call AskForTime method. AskForTime method need See permission.");
            }
                
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            Console.Clear();

            // Ispisivanje preostalog vremena
            Console.WriteLine($"Preostalo vreme: {GetRemainingTime()}");

            if (DateTime.Now >= endTime)
            {
                dispatcherTimer.Stop();
                isTimerSet = false; // Resetujemo postavku tajmera kada istekne
                Console.WriteLine("Tajmer je istekao.");
            }
        }

        public TimeSpan GetRemainingTime()
        {
            if (!isTimerSet || DateTime.Now >= endTime)
            {
                return TimeSpan.Zero;
            }

            if (!isActive)
            {
                return timerDuration;
            }

            return endTime - DateTime.Now; // Vraća preostalo vreme
        }

        public bool IsTimerExpired()
        {
            return !dispatcherTimer.IsEnabled && !isTimerSet;
        }

        public bool IsTimerActive()
        {
            return dispatcherTimer.IsEnabled;
        }

        public bool IsTimerSet()
        {
            return isTimerSet;
        }
    }
}
