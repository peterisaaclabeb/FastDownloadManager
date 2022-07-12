namespace FastDownloadManager
{
	internal class DownloadChunk
	{
		private string _filename;
		private string _filepath;
		private CancellationTokenSource _cts;

		public bool Active { get; private set; }
		public bool Completed => Pointer > To;


		private long From { get; }
		private long Pointer { get; set; }

		private FileStream _filestream;
		public Stream DataStream => File.OpenRead(_filepath);

		private long To { get; }
		private long TotalSize => To - From + 1;

		public DownloadChunk Next { get; set; }

		public DownloadChunk(long from, long to, string name)
		{
			_filename = $"{name}-{from}";
			From = from;
			Pointer = from;
			To = to;
			var baseDirectory = Path.Combine(Path.GetTempPath(), "download-manager");
			Directory.CreateDirectory(baseDirectory);

			_filepath = Path.Combine(baseDirectory, _filename);
			var fileInfo = new FileInfo(_filepath);
			Pointer = From + fileInfo.Length;
		}

		public async Task StartAsync(DownloadWorker downloadWorker)
		{
			Active = true;
			_cts = new CancellationTokenSource();
			var token = _cts.Token;
			_filestream = new FileStream(_filepath, FileMode.Append, FileAccess.Write);

			try
			{
				var buffer = new byte[1024 * 16];
				var stream = await downloadWorker.GetRangeAsync(Pointer, To, token);

				var bytesRead = 0;
				do
				{
					bytesRead = await stream.ReadAsync(buffer, token);
					BytesDownloaded(buffer, 0, bytesRead);
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
			Active = false;
			_filestream.Close();

			return Task.CompletedTask;
		}

		private void InitDataStore()
		{
			
		}

		private void BytesDownloaded(byte[] buffer, int offset, int bytesCount)
		{
			_filestream.Write(buffer, offset, bytesCount);
			_filestream.Flush();
			Pointer += bytesCount;
		}

		private void NotifyUpdate(int index)
		{
			Console.SetCursorPosition(0, index);
			long v = (Pointer - From);
			double percentage = v * 100 / (double)TotalSize;
			Console.WriteLine($"thread-{index} {v} of {TotalSize} {percentage:0.##}%");
		}
	}
}
