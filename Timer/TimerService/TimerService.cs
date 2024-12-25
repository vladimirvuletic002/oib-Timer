using Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
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

        public TimerService()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += OnTimerTick; // Event handler za Tick događaj
            isTimerSet = false; // Tajmer nije postavljen prilikom inicijalizacije
        }
        public void TestCommunication()
        {
            Console.WriteLine("Communication established.");

            IIdentity identity = Thread.CurrentPrincipal.Identity;

            Console.WriteLine("Tip autentifikacije : " + identity.AuthenticationType);

            WindowsIdentity windowsIdentity = identity as WindowsIdentity;

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
        [PrincipalPermission(SecurityAction.Demand, Role = "StartStop")]
        public void StartTimer()
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

            if (dispatcherTimer.IsEnabled)
            {
                Console.WriteLine($"Tajmer je pokrenut. Završava se u {endTime}.");
            }
            else
            {
                Console.WriteLine("Greška prilikom pokretanja tajmera.");
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "StartStop")]
        public void StopTimer()
        {
            if (!dispatcherTimer.IsEnabled)
            {
                Console.WriteLine("Tajmer nije aktivan.");
                return;
            }

            dispatcherTimer.Stop();
            isTimerSet = false; // Resetujemo postavku tajmera
            Console.WriteLine("Tajmer je zaustavljen.");
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Change")]
        public void ResetTimer()
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
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Change")]
        public void SetTimer(string time)
        {
            try
            {
                if (TimeSpan.TryParse(time, out TimeSpan duration) && duration > TimeSpan.Zero)
                {
                    endTime = DateTime.Now.Add(duration);
                    isTimerSet = true; // Obeležavamo da je tajmer validno postavljen
                    Console.WriteLine($"Tajmer je postavljen na {duration.TotalMinutes} minuta. Završava se u {endTime}.");
                }
                else
                {
                    isTimerSet= false;
                    Console.WriteLine("Uneta vrednost za tajmer nije validna.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri postavljanju tajmera: {ex.Message}");
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "See")]
        public string AskForTime()
        {
            throw new NotImplementedException();

        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            Console.WriteLine($"Tick: {DateTime.Now} - Kraj: {endTime}");
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

            return endTime - DateTime.Now; // Vraća preostalo vreme
        }

        public bool IsTimerExpired()
        {
            return !dispatcherTimer.IsEnabled && !isTimerSet;
        }
    }
}
