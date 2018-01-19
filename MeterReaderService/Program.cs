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
	}
}
