using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class Audit : IDisposable
    {

        private static EventLog customLog = null;
        const string SourceName = "Manager.Audit";
        const string LogName = "AuthLog";

        //Audit
        static Audit()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                customLog = new EventLog(LogName,
                    Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                customLog = null;
                Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
            }
        }


        public static void AuthenticationSuccess(string userName)
        {

            if (customLog != null)
            {
                string UserAuthenticationSuccess =
                    AuditEvents.AuthenticationSuccess;
                string message = String.Format(UserAuthenticationSuccess,
                    userName);
                customLog.WriteEntry(message, EventLogEntryType.SuccessAudit, (int)AuditEventTypes.AuthenticationSuccess);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.AuthenticationSuccess));
            }
        }

        public static void AuthorizationSuccess(string userName, string serviceName)
        {
            //TO DO
            if (customLog != null)
            {
                string AuthorizationSuccess =
                    AuditEvents.AuthorizationSuccess;
                string message = String.Format(AuthorizationSuccess,
                    userName, serviceName);
                customLog.WriteEntry(message, EventLogEntryType.SuccessAudit, (int)AuditEventTypes.AuthorizationSuccess);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.AuthorizationSuccess));
            }
        }

        public static void AuthorizationFailed(string userName, string serviceName, string reason)
        {
            if (customLog != null)
            {
                string AuthorizationFailed =
                    AuditEvents.AuthorizationFailed;
                string message = String.Format(AuthorizationFailed,
                    userName, serviceName, reason);
                customLog.WriteEntry(message, EventLogEntryType.FailureAudit, (int)AuditEventTypes.AuthorizationFailed);

                int failedCount = IntrusionDetection.RegisterFailedAttempt(userName, serviceName);
                SeverityLevel severity = SeverityLevel.Information;
                if (failedCount == 2)
                    severity = SeverityLevel.Warning;
                else if (failedCount >= 3)
                    severity = SeverityLevel.Critical;

                SendAlarmToIPS(userName, serviceName, severity); // IPSAudit poziva EventLog!
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.AuthorizationFailed));
            }
        }

        public static void SendAlarmToIPS(string userName, string serviceName, SeverityLevel severity)
        {
            string fileLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string fileName = Path.GetFileName(fileLocation);

            IPSAudit.LogIPSAlarm(userName, serviceName, fileName, fileLocation, severity);
        }

        public void Dispose()
        {
            if (customLog != null)
            {
                customLog.Dispose();
                customLog = null;
            }
        }
    }
}
