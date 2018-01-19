using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeterReaderService
{
	class Program
	{
		static void Main(string[] args)
		{
			var ServicesToRun = new ServiceBase[]
			{
				new MeterReaderFacade()
			};
			if (Environment.UserInteractive)
			{
				RunInteractive(ServicesToRun);
			}
			else
			{
				ServiceBase.Run(ServicesToRun);
			}
		}
		static void RunInteractive(ServiceBase[] servicesToRun)
		{
			Console.WriteLine("Service running in interactive mode.");
			Console.WriteLine();

			MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart",
				BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (ServiceBase service in servicesToRun)
			{
				Console.Write("Starting {0}...", service.ServiceName);
				onStartMethod.Invoke(service, new object[] { new string[] { } });
				Console.Write("Started");
			}

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(
				"Press any key to stop the services and end the process...");
			Console.ReadKey();
			Console.WriteLine();

			MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop",
				BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (ServiceBase service in servicesToRun)
			{
				Console.Write("Stopping {0}...", service.ServiceName);
				onStopMethod.Invoke(service, null);
				Console.WriteLine("Stopped");
			}

			Console.WriteLine("All services stopped.");
			// Keep the console alive for a second to allow the user to see the message.
			Thread.Sleep(1000);
		}
	}
}
