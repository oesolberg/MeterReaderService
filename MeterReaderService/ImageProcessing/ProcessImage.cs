using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MeterReaderService.Common;

namespace MeterReaderService.ImageProcessing
{
	public class ProcessImage
	{
		private int _alottedTime= 60000;

		public void Execute(string filePath)
		{
			CutAfterAllottedTimeAndThreeRetries(filePath);
		}

		private void CutAfterAllottedTimeAndThreeRetries(string fileToProcess)
		{
			var numberOfRetries = 0;
			ImageData lastResult = null;

			var dbStore = new DataStorage.Repository();
			do
			{
				lastResult = RunFileToProcess(fileToProcess);
				numberOfRetries++;
			} while (numberOfRetries < 3  && 
				lastResult.ProcessingResult == ProcessingResultType.Cancelled);

			if ( lastResult.ProcessingResult == ProcessingResultType.Ok)
			{
				MeterReaderEventLog.SaveToEventLog(string.Format("File processed with rotation {0} and probabillity {1}%",lastResult.Rotation,(lastResult.Probabillity*100).ToString("N1")),EventLogEntryType.Information);
				dbStore.SaveData(lastResult);
			}
		}

		private ImageData RunFileToProcess(string fileToProcess)
		{

			CancellationTokenSource cts = new CancellationTokenSource();
			Task<ImageData> processImageAsTask = Task.Factory.StartNew<ImageData>(() => DoProcessingAsTask(fileToProcess, cts.Token));
			ImageData resultData = null;

			// wait for max 1 minutes(60000 milliseks). Abort process if the imageprocessing fails
			if (processImageAsTask.Wait(_alottedTime, cts.Token))
			{
				resultData = processImageAsTask.Result;
				return resultData;

			}
			else
			{
				// it did not finish within allotted time
				cts.Cancel();
				return new ImageData() { ProcessingResult = ProcessingResultType.Cancelled };
			}

		}

		private ImageData DoProcessingAsTask(string fileToProcess, CancellationToken ct)
		{
			if (ct.IsCancellationRequested == true)
			{
				ct.ThrowIfCancellationRequested();
			}
			var imageHandler = new MeterReaderImageHandler();
			return imageHandler.DoImageProcessing(fileToProcess);

		}
	}
}