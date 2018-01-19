using System;
using Nancy;

namespace MeterReaderService.WebServer
{
	public class MainModule : NancyModule
	{
		public MainModule()
		{
			Get["/"] = x =>
			{
				var dbStorage = new DataStorage.Repository();
				var lastResult = dbStorage.GetLatestImageData();
				//Use this if you want to return json
				//return new RotationData()
				//{
				//	Probabillity = lastResult.Probabillity,
				//	Rotation = lastResult.Rotation,
				//	FileChangedDate = lastResult.FileChangedDateTime,
				//	DeliveryDate = DateTime.Now
				//};
				var probabillity = (lastResult.Probabillity * 100).ToString("0.00");
				return "Probabillity=" + probabillity + ";Rotation=" + lastResult.Rotation + ";FileChangedDate=" + lastResult.FileChangedDateTime.ToString("yyyy-MM-dd HH:mm:ss") + ";DeliveryDate=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


			};
		}

	}
}