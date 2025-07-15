using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public enum SeverityLevel
    {
        Information,
        Warning,
        Critical
    }

    public static class IPSAudit
    {
        private static EventLog ipsLog = null;
        const string SourceName = "Manager.IPSAudit";
        const string LogName = "IPSLog";

        static IPSAudit()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                ipsLog = new EventLog(LogName, Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                ipsLog = null;
                Console.WriteLine("Error creating IPS log: " + e.Message);
            }
        }

        public static void LogIPSAlarm(string userName, string serviceName, string fileName, string fileLocation, SeverityLevel severity)
        {
            if (ipsLog != null)
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string message = $"ALARM: {now}\n" +
                                 $"User: {userName}\n" +
                                 $"Service: {serviceName}\n" +
                                 $"File: {fileName}\n" +
                                 $"Location: {fileLocation}\n" +
                                 $"Severity: {severity}";

                EventLogEntryType entryType = EventLogEntryType.Information;
                switch (severity)
                {
                    case SeverityLevel.Warning: entryType = EventLogEntryType.Warning; break;
                    case SeverityLevel.Critical: entryType = EventLogEntryType.Error; break;
                }
                ipsLog.WriteEntry(message, entryType);
            }
            else
            {
                Console.WriteLine("Error writing to IPS event log.");
            }
        }
    }
}
