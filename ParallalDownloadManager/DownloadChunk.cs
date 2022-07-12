using ParallalDownloadManager.DataStore;
using ParallalDownloadManager.Worker;

namespace ParallalDownloadManager
{
	internal class DownloadChunk : IDisposable
	{
		private CancellationTokenSource _cts;

		public bool Downloading { get; private set; }
		public bool Completed => _pointer > _to;
		public long TotalSize => _to - _from + 1;
		public Stream DataStream => _dataStore.DataStream;

		private long _from;
		private long _pointer;
		private long _to;
		private IDataStore _dataStore;

		public DownloadChunk? Next { get; set; }

		public DownloadChunk(long from, long to, string filename)
		{
			var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
			var extension = Path.GetExtension(filename);
			var name = $"{nameWithoutExtension}-{from}.{extension}";

			_dataStore = new FileDataStore(name);
			_from = from;
			_pointer = from;
			_to = to;
			_pointer = _from + _dataStore.Length;
		}

		public async Task StartAsync(IDownloadWorker downloadWorker)
		{
			Downloading = true;
			_cts = new CancellationTokenSource();
			var token = _cts.Token;
			_dataStore.Open();
			try
			{
				var buffer = new byte[1024 * 16];
				var stream = await downloadWorker.GetRangeAsync(_pointer, _to, token);

				var bytesRead = 0;
				do
				{
					bytesRead = await stream.ReadAsync(buffer, token);
					_dataStore.Write(buffer, 0, bytesRead);
					_pointer += bytesRead;
					NotifyUpdate(downloadWorker.Index);

				} while (!token.IsCancellationRequested && stream.CanRead && bytesRead > 0);
			}
			catch (Exception ex)
			{

			}
			finally
			{
				await StopAsync();
			}
		}

		public Task StopAsync()
		{
			_cts?.Cancel();
			Downloading = false;

			return Task.CompletedTask;
		}

		private void NotifyUpdate(int index)
		{
			long downloaded = _pointer - _from;
			double percentage = downloaded * 100 / (double)TotalSize;
			Console.SetCursorPosition(0, index);
			Console.WriteLine($"thread-{index} {downloaded} of {TotalSize} {percentage:0.##}%\t");
		}

		public void Dispose()
		{
			_dataStore.Dispose();
		}
	}
}
