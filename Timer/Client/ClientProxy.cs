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
            factory = this.CreateChannel();
        }

        public void AddUser(string username, string password)
        {

            try
            {
                factory.AddUser(username, password);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
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
            throw new NotImplementedException();
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
