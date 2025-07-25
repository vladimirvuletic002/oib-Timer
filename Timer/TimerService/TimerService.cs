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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TimerService : ITimerService
    {

        //private DispatcherTimer dispatcherTimer; // Tajmer za praćenje intervala
        private System.Timers.Timer timer;
        private DateTime endTime; // Vreme završetka tajmera
        private bool isTimerSet; // Oznaka da li je tajmer postavljen
        private TimeSpan timerDuration;
        private bool isActive;

        private static readonly byte[] key = Encoding.UTF8.GetBytes("OvoJeVrloTajniKljuc12345");
        private readonly object timerLock = new object();

        public TimerService()
        {
            //dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Tick += OnTimerTick; 
            timer = new System.Timers.Timer(1000); // Tick svakih 1s
            timer.Elapsed += OnTimerTick; // Event handler za Tick događaj
            timer.AutoReset = true;
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
                lock(timerLock)
                {
                    if (!isTimerSet)
                    {
                        Console.WriteLine("Tajmer nije postavljen. Koristite SetTimer pre StartTimer.");
                        return;
                    }

                    if (timer.Enabled)
                    {
                        Console.WriteLine("Tajmer je već pokrenut.");
                        return;
                    }

                    //dispatcherTimer.Interval = TimeSpan.FromSeconds(1); // Tick interval
                    //dispatcherTimer.Start();
                    timer.Start();
                    isActive = true;

                    endTime = DateTime.Now.Add(timerDuration);

                    if (timer.Enabled)
                    {
                        Console.WriteLine($"Tajmer je pokrenut. Završava se u {endTime}.");
                    }
                    else
                    {
                        Console.WriteLine("Greška prilikom pokretanja tajmera.");
                    }
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
                lock(timerLock)
                {
                    if (!timer.Enabled)
                    {
                        Console.WriteLine("Tajmer nije aktivan.");
                        return;
                    }


                    timer.Stop();

                    //isTimerSet = false; // Resetujemo postavku tajmera
                    timerDuration = GetRemainingTime();

                    string duration = timerDuration.ToString(@"hh\:mm\:ss");

                    //byte[] key = Encoding.UTF8.GetBytes("OvoJeVrloTajniKljuc1234");

                    //if (key.Length != 24)
                    //{
                    //Array.Resize(ref key, 24);
                    //}

                    string encryptedTime = Encrypt3DES(duration);

                    //SetTimer(duration);
                    SetTimer(encryptedTime);
                    isActive = false;
                    Console.WriteLine("Tajmer je zaustavljen.");
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
                lock (timerLock)
                {
                    if (!isTimerSet)
                    {
                        Console.WriteLine("Tajmer nije postavljen.");
                        return;
                    }

                    // Zaustavljamo tajmer
                    //dispatcherTimer.Stop();
                    timer.Stop();
                    isTimerSet = false; // Resetujemo status da nije postavljen
                    endTime = DateTime.MinValue; // Postavljamo "nulu" za vreme završetka

                    Console.WriteLine($"Tajmer je resetovan. Trenutno vreme završetka: {endTime}");
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

        public static string Decrypt3DES(string encryptedText)
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
                lock (timerLock)
                {
                    try
                    {

                        string decryptedTime = Decrypt3DES(encryptedTime);

                        if (TimeSpan.TryParse(decryptedTime, out TimeSpan duration) && duration > TimeSpan.Zero)
                        {
                            //endTime = DateTime.Now.Add(duration);
                            timerDuration = duration;
                            isTimerSet = true;
                            Console.WriteLine($"Enkodovano vreme: {encryptedTime}");
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

                lock (timerLock)
                {
                    return GetRemainingTime().ToString();
                }
                
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
            Console.WriteLine($"Preostalo vreme: {GetRemainingTime().ToString(@"hh\:mm\:ss")}");

            if (DateTime.Now >= endTime)
            {
                //dispatcherTimer.Stop();
                timer.Stop();
                isTimerSet = false; // Resetujemo postavku tajmera kada istekne
                isActive = false;
                Console.WriteLine("Tajmer je istekao.");
            }
        }

        public TimeSpan GetRemainingTime()
        {
            lock (timerLock)
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
        }

        public bool IsTimerExpired()
        {
            return !timer.Enabled && !isTimerSet;
        }

        public bool IsTimerActive()
        {
            return timer.Enabled;
        }

        public bool IsTimerSet()
        {
            return isTimerSet;
        }
    }
}
