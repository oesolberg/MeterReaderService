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
				//};if(
				decimal probabillity = 0;
				var rotation = 0;
				var fileChangedDateTime = DateTime.MinValue;
				if (lastResult != null)
				{
					fileChangedDateTime = lastResult.FileChangedDateTime;
					probabillity = (decimal)(lastResult.Probabillity * 100);
					rotation = lastResult.Rotation;
				}
				
				return "Probabillity=" + probabillity.ToString("0.00")
						+ ";Rotation=" + rotation + ";FileChangedDate=" 
						+ fileChangedDateTime.ToString("yyyy-MM-dd HH:mm:ss") 
						+ ";DeliveryDate=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


			};
		}

	}
}