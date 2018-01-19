using System;
using System.Configuration;
using Nancy.Hosting.Self;

namespace MeterReaderService.WebServer
{
	public class ServiceWebServer : NancyHost
	{
		private NancyHost _host;

		public void CreateServer()
		{
			var port = "1234";
			var foundValueInConfig = ConfigurationManager.AppSettings["port"];
			if (!string.IsNullOrEmpty(foundValueInConfig))
			{
				port = foundValueInConfig;
			}
			_host = new NancyHost(new Uri("http://localhost:" + port));
			_host.Start();

		}

		public void StopServer()
		{
			_host.Stop();
		}
	}
}