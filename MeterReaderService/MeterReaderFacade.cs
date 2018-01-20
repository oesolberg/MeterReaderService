using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using MeterReaderService.Common;
using MeterReaderService.WebServer;

namespace MeterReaderService
{
	public class MeterReaderFacade : ServiceBase
	{
		private int _sleepInterval;
		private string _folderToWatch;
		private string _filefilter;
		private FileSystemWatcher fileSystemWatcher;
		private ServiceWebServer _server;

		public MeterReaderFacade()
		{
			ServiceName = StaticVars.ServiceName;
			InitializeComponent();
			fileSystemWatcher.Created += fileSystemWatcher_Changed;
		}

		protected override void OnStart(string[] args)
		{
			MeterReaderEventLog.SaveToEventLog("Starting MeterReaderService", EventLogEntryType.Information);
			_filefilter = ConfigurationManager.AppSettings["filter"];
			fileSystemWatcher.Filter = _filefilter;
			_folderToWatch = ConfigurationManager.AppSettings["folder"];
			fileSystemWatcher.Path = _folderToWatch;
			_sleepInterval = 10000;
			var sleepIntervalFromConfig = ConfigurationManager.AppSettings["sleepinterval"];
			int sleepIntervalFromConfigConvertedToInt;
			if (!string.IsNullOrWhiteSpace(sleepIntervalFromConfig) && int.TryParse(sleepIntervalFromConfig, out sleepIntervalFromConfigConvertedToInt))
			{
				_sleepInterval = sleepIntervalFromConfigConvertedToInt;
			}
			_server = new ServiceWebServer();
			_server.CreateServer();
		}

		protected override void OnStop()
		{
			_server.StopServer();
			MeterReaderEventLog.SaveToEventLog("MeterReaderService stopped", EventLogEntryType.Information);
		}

		private void fileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
		{
			try
			{
				//Added 1-2 seconds sleep to avoid trying to take a file that is beeing written
				Thread.Sleep(_sleepInterval);
				if ((e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed) && FileExists(e.FullPath))
				{
					var logString = "MeterReaderService sensed a change and started processing file " + e.FullPath;
					MeterReaderEventLog.SaveToEventLog(logString,EventLogEntryType.Information);
					
					var stopWatch = new Stopwatch();
					//Some file has changed.
					stopWatch.Start();

					var imageProcessor = new ImageProcessing.ProcessImage();
					imageProcessor.Execute(e.FullPath);
					if (stopWatch != null)
					{
						stopWatch.Stop();
						var stringToLog = "Processed image in  " + stopWatch.ElapsedMilliseconds.ToString("N1") + " ms.";
						MeterReaderEventLog.SaveToEventLog(stringToLog,EventLogEntryType.Information);
					}
				}
			}
			catch (Exception ex)
			{
				MeterReaderEventLog.SaveToEventLog(ex);
				throw;
			}
		}
		
		private bool FileExists(string fullPath)
		{
			return System.IO.File.Exists(fullPath);
		}

		private void InitializeComponent()
		{
			this.fileSystemWatcher = new System.IO.FileSystemWatcher();
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
			// 
			// fileSystemWatcher
			// 
			this.fileSystemWatcher.EnableRaisingEvents = true;
			this.fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcher_Changed);
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();

		}
	}
}