using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimerService
{
    public class TimerService : ITimerService
    {
        public static Dictionary<string, User> UserAccountsDB = new Dictionary<string, User>();

        /// <summary>
        /// Add new user to UserAccountsDB. Dictionary Key is "username"
        /// </summary>
        public void AddUser(string username, string password)
        {
            if (!UserAccountsDB.ContainsKey(username))
            {
                UserAccountsDB.Add(username, new User(username, password));
            }
            else
            {
                Console.WriteLine($"Korisnik sa korisnickim imenom {username} vec postoji u bazi");
            }

            IIdentity identity = Thread.CurrentPrincipal.Identity;

            Console.WriteLine("Tip autentifikacije : " + identity.AuthenticationType);

            WindowsIdentity windowsIdentity = identity as WindowsIdentity;

            Console.WriteLine("Ime klijenta koji je pozvao metodu : " + windowsIdentity.Name);
            Console.WriteLine("Jedinstveni identifikator : " + windowsIdentity.User);

            Console.WriteLine("Grupe korisnika:");
            foreach (IdentityReference group in windowsIdentity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                string name = (sid.Translate(typeof(NTAccount))).ToString();
                Console.WriteLine(name);
            }
        }


        public void StartTimer()
        {
            throw new NotImplementedException();
        }

        public void StopTimer()
        {
            throw new NotImplementedException();
        }

        public void ResetTimer()
        {
            throw new NotImplementedException();
        }

        public void SetTimer(string encryptedTime)
        {
            throw new NotImplementedException();
        }

        public string AskForTime()
        {
            throw new NotImplementedException();
        }
    }
}
