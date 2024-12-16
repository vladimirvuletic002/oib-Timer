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
        public void TestCommunication()
        {
            Console.WriteLine("Communication established.");
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
