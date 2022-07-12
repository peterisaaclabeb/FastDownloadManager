
namespace ParallalDownloadManager.Worker
{
	internal interface IDownloadWorker
	{
		int Index { get; set; }

		Task<Stream> GetRangeAsync(long from, long to, CancellationToken token);
	}
}