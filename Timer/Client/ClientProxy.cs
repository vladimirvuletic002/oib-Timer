using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Contracts;

namespace Client
{
    public class ClientProxy : ChannelFactory<ITimerService>, ITimerService, IDisposable
    {
        ITimerService factory;

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
            throw new NotImplementedException();
        }

        public void SetTimer(string encryptedTime)
        {
            throw new NotImplementedException();
        }

        public void StartTimer()
        {
            try
            {
                Console.WriteLine("Pokretanje");
                factory.StartTimer();
            }
            catch
            {
                Console.WriteLine("Nemate permisiju za pokretanje tajmera");
            }
        }


        public void StopTimer()
        {
            throw new NotImplementedException();
        }

        public string AskForTime()
        {
            throw new NotImplementedException();

        }
        public void Dispose()
        {
            if (factory != null)
            {
                factory = null;
            }

            this.Close();
        }

        
    }
}
