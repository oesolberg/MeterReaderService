using System;

namespace MeterReaderService.WebServer
{
	public class RotationData
	{
		public double Probabillity { get; set; }
		public int Rotation { get; set; }
		public DateTime FileChangedDate { get; set; }

		public DateTime DeliveryDate { get; set; }
	}
}