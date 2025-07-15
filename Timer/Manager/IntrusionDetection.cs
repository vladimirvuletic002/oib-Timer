using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public static class IntrusionDetection
    {
        private static readonly Dictionary<string, int> failedAttempts = new Dictionary<string, int>();

        // Kljuc: userName|serviceName
        public static int RegisterFailedAttempt(string userName, string serviceName)
        {
            string key = $"{userName}|{serviceName}";
            if (!failedAttempts.ContainsKey(key))
                failedAttempts[key] = 1;
            else
                failedAttempts[key]++;

            return failedAttempts[key];
        }
    }
}
