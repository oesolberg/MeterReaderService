using System;
using System.Diagnostics;

namespace MeterReaderService.Common
{
	public class MeterReaderEventLog
	{
		public static void SaveToEventLog(Exception exception)
		{
			SaveToEventLog(exception.Message +
			                    Environment.NewLine +
			                    exception.Source +
			                    Environment.NewLine +
			                    exception.StackTrace, EventLogEntryType.Error);
		}

		public static void SaveToEventLog(string message, EventLogEntryType eventLogEntryType)
		{
			CreateEventLogIfMissing();
			using (EventLog eventLog = new EventLog(StaticVars.EventLog))
			{
				eventLog.Source = StaticVars.EventSource;
				eventLog.WriteEntry(message, eventLogEntryType);
				LogToConsoleIfPossible(message);
			}
		}

		private static void LogToConsoleIfPossible(string stringToWrite)
		{
			if (Environment.UserInteractive)
			{
				Console.WriteLine(stringToWrite);
			}
		}

		private static void CreateEventLogIfMissing()
		{
			if (!EventLog.SourceExists(StaticVars.EventLog))
				EventLog.CreateEventSource(StaticVars.EventSource, StaticVars.EventLog);
		}

	}
}