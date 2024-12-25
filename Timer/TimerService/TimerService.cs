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

namespace TimerService
{
    public class TimerService : ITimerService
    {
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
            Console.WriteLine("testiranje uspesno");
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "StartStop")]
        public void StopTimer()
        {
            throw new NotImplementedException();
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Change")]
        public void ResetTimer()
        {
            throw new NotImplementedException();
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Change")]
        public void SetTimer(string encryptedTime)
        {
            throw new NotImplementedException();

        }

        [PrincipalPermission(SecurityAction.Demand, Role = "See")]
        public string AskForTime()
        {
            throw new NotImplementedException();

        }
    }
}
