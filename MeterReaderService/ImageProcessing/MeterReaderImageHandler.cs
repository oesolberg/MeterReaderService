using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using MeterReaderService.Common;

namespace MeterReaderService.ImageProcessing
{
	public class MeterReaderImageHandler
	{
		public ImageData DoImageProcessing(string fileToProcess)
		{
			MeterReaderEventLog.SaveToEventLog("starting doimageproc", EventLogEntryType.Information);

			var foundImage = ExtractMeterImage(fileToProcess);
			ShowImage(foundImage);
			if (foundImage == null) return new ImageData() { ProcessingResult = ProcessingResultType.NoImageFound };
			return TestRotation(foundImage, fileToProcess);
		}

		private Image<Bgr, byte> ExtractMeterImage(string fileToProcess)
		{
			Image<Bgr, byte> imageToReturn = null;
			var filepathToTemplate = GetImageWithFullPath("MaalerMal");
			try
			{
				Image<Bgr, byte> source = new Image<Bgr, byte>(fileToProcess);
				Image<Bgr, byte> template = new Image<Bgr, byte>(filepathToTemplate);
				ShowImage(source);
				ShowImage(template);
				using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.Ccoeff))
				{
					double[] minValues, maxValues;
					Point[] minLocations, maxLocations;
					result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

					// If we have a value higher than 0.9(90%) we think it is a match
					if (maxValues[0] > 0.9)
					{
						// This is a match. Copy the small image part to a new image
						Rectangle match = new Rectangle(maxLocations[0], template.Size);
						var smallImage = CreateSmallImage(match, source, template.Size);

						//Try to export the inside image without the borders
						imageToReturn = smallImage.Copy();
					}
				}

			}
			catch (Exception ex)
			{
				MeterReaderEventLog.SaveToEventLog(ex);
				
				return null;
			}
			ShowImage(imageToReturn);
			return imageToReturn;
		}

		private static void ShowImage(Image<Bgr, byte> imageToReturn)
		{
			return;
			//Remove the return if you want to se the images used when debugging
			CvInvoke.Imshow("extract", imageToReturn);
			CvInvoke.WaitKey(0);
		}

		private Image<Bgr, byte> CreateSmallImage(Rectangle match, Image<Bgr, byte> imageToShow, Size size)
		{
			imageToShow.ROI = match;
			return imageToShow.Copy();
		}


		private string GetImageWithFullPath(string appsettingKey)
		{
			var foundAppsetting = ConfigurationManager.AppSettings[appsettingKey];
			if (string.IsNullOrEmpty(foundAppsetting)) throw new ConfigurationErrorsException($"Key not found ({appsettingKey})");

			var applicationPath = System.AppDomain.CurrentDomain.BaseDirectory;
			return Path.Combine(applicationPath, "Templates", foundAppsetting);
		}

		private ImageData TestRotation(Image<Bgr, byte> imageToProcess, string fileToProcess)
		{
			var pathToTemplate = GetImageWithFullPath("NaalMal");
			var templateImage = new Image<Bgr, byte>(pathToTemplate);
			var listOfResults = new List<ImageResult>();
			var totalRotation = 360;
			var rotation = 0;
			while (rotation < totalRotation)
			{
				var rotationToUse = 0;
				if (rotation > 180)
				{
					rotationToUse = 180 - rotation;
				}
				else
				{
					rotationToUse = rotation;
				}
				var rotatedtemplateImage = templateImage.Rotate(rotationToUse, new Bgr(Color.Gray));
				var imgResult = TryToMatchRotation(imageToProcess,
					rotatedtemplateImage, rotationToUse);
				listOfResults.Add(imgResult);
				rotation++;
			}
			return FindHighestProbabillity(listOfResults, fileToProcess);

		}

		private ImageData FindHighestProbabillity(List<ImageResult> listOfResults, string fullFilePath)
		{
			var ioInfo = new FileInfo(fullFilePath);
			var highestProb = listOfResults.OrderByDescending(x => x.ProbabillityOfCorrect).First();
			return new ImageData()
			{
				Probabillity = highestProb.ProbabillityOfCorrect,
				Rotation = highestProb.Rotation,
				ProcessingResult = ProcessingResultType.Ok,
				FileChangedDateTime = ioInfo.LastWriteTime
			};
		}

		public ImageResult TryToMatchRotation(Image<Bgr, byte> source, Image<Bgr, byte> templateImage, int degreesOfRotation)
		{
			var imgResult = new ImageResult() { Rotation = degreesOfRotation };
			Image<Bgr, byte> template = null;
			if (templateImage != null) template = templateImage;

			Image<Bgr, byte> imageToShow = source.Copy();

			using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
			{
				double[] minValues, maxValues;
				Point[] minLocations, maxLocations;
				result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
				imgResult.ProbabillityOfCorrect = maxValues[0];
			}
			return imgResult;
		}
	}
}