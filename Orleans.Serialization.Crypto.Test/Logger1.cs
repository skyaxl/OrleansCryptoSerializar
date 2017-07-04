using System;
using System.Collections.Generic;
using Orleans.Runtime;
using System.Diagnostics;

namespace Orleans.Serialization.Crypto.Test
{
    internal class Logger1 : Logger
    {
        public override Severity SeverityLevel { get; } = Severity.Error;

        public override string Name { get; } = "test";

        public override void DecrementMetric(string name)
        {
            Debug.Write(name);
        }

        public override void DecrementMetric(string name, double value)
        {
          
        }

        public override Logger GetLogger(string loggerName)
        {
            return this;
        }

        public override void IncrementMetric(string name)
        {
            Debug.Write(name);

        }

        public override void IncrementMetric(string name, double value)
        {
            
        }

        public override void Log(int errorCode, Severity sev, string format, object[] args, Exception exception)
        {
            Debug.Write($"{errorCode} {sev} {format}");

        }

        public override void TrackDependency(string name, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
        }

        public override void TrackEvent(string name, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public override void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public override void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
        }

        public override void TrackMetric(string name, TimeSpan value, IDictionary<string, string> properties = null)
        {
        }

        public override void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
        }

        public override void TrackTrace(string message)
        {
        }

        public override void TrackTrace(string message, Severity severityLevel)
        {
        }

        public override void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
        {
        }

        public override void TrackTrace(string message, IDictionary<string, string> properties)
        {
        }
    }
}