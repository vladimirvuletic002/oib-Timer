﻿using System;
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
            try
            {
                factory.ResetTimer();
            }
            catch
            {
                Console.WriteLine("Nemate dozvolu za akciju pokretanja tajmera!");
            }
        }

        public void SetTimer(string time)
        {
            try
            {
                factory.SetTimer(time);
            }
            catch 
            {
                Console.WriteLine("Nemate dozvolu za akciju setovanja tajmera!");
            }
        }

        public void StartTimer()
        {
            try
            {
                factory.StartTimer();
            }
            catch 
            {
                Console.WriteLine("Nemate dozvolu za akciju pokretanja tajmera!");
            }
        }


        public void StopTimer()
        {
            try
            {
                factory.StopTimer();
            }
            catch 
            {
                Console.WriteLine("Nemate dozvolu za akciju zaustavljanja tajmera!");
            }
        }

        public string AskForTime()
        {
            throw new NotImplementedException();

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
                // Pozivamo metodu na serveru da dobijemo preostalo vreme
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
    }
}
