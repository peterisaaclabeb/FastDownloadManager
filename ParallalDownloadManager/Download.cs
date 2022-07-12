using ParallalDownloadManager.Worker;

namespace ParallalDownloadManager
{
	internal class Download
	{
		private readonly Queue<IDownloadWorker> _workeres;
		private DownloadChunk _head;

		public string Url { get; private set; }
		public string Filename { get; private set; }
		public long Filesize { get; private set; }


		public Download()
		{
			_workeres = new Queue<IDownloadWorker>();
		}

		public Download(string url, string name, long length, int threads = 4) : this()
		{
			Url = url;
			Filesize = length;
			Filename = name;
			_head = InitChunks(length, threads, name);
			for (int i = 0; i < threads; i++)
			{
				_workeres.Enqueue(new HTTPDownloadWorker(Url, i));
			}
		}

		public Task<string> StartAsync()
		{
			ScheduleChunks();
			var task = Task.Run(async () =>
			{
				var completed = CheckComplete();

				while (!completed)
				{
					LogStatus();
					await Task.Delay(TimeSpan.FromSeconds(1));
					completed = CheckComplete();
				}
				LogStatus();
				return SaveFile();
			});

			return task;
		}

		private bool CheckComplete()
		{
			var current = _head;
			while (current != null)
			{
				if (!current.Completed)
				{
					return false;
				}
				current = current.Next;
			}

			return true;
		}

		private void ScheduleChunks()
		{
			var current = _head;

			while (_workeres.Any() && current != null)
			{
				if (current.Completed || current.Downloading)
				{
					current = current.Next;
					continue;
				}

				var worker = _workeres.Dequeue();
				var task = current.StartAsync(worker).ContinueWith(t =>
				{
					_workeres.Enqueue(worker);
					ScheduleChunks();
				});

				current = current.Next;
			}
		}

		private string SaveFile()
		{
			const int chunkSize2 = 2 * 1024;
			var path = Path.Combine(@"C:\Users\Peter\OneDrive\Desktop", Filename);
			using var output = File.Create(path);

			var current = _head;
			while (current != null)
			{
				using var input = current.DataStream;
				var buffer = new byte[chunkSize2];
				int bytesRead;
				while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					output.Write(buffer, 0, bytesRead);
				}
				current = current.Next;
			}

			return path;
		}

		private void LogStatus()
		{
			var current = _head;
			int index = 0;
			while (current != null)
			{
				NotifyUpdate(index, current.Downloaded, current.TotalSize);
				current = current.Next;
				index++;
			}
		}

		private void NotifyUpdate(int index, long downloaded, long totalSize)
		{
			double percentage = downloaded * 100 / (double)totalSize;
			Console.SetCursorPosition(0, index);
			Console.WriteLine($"thread-{index} {downloaded} of {totalSize} {percentage:0.##}%\t");
		}

		private DownloadChunk InitChunks(long length, int threads, string name)
		{
			var chuckSize = length / threads;
			DownloadChunk prev = null;
			DownloadChunk head = null;

			for (int i = 0; i < threads; i++)
			{
				long from = i * chuckSize;
				long to = ((i + 1) * chuckSize) - 1;
				if (i == threads - 1)
				{
					to += length % threads;
				}
				var chunk = new DownloadChunk(from, to, name);

				if (i == 0)
				{
					head = chunk;
				}

				if (prev != null)
				{
					prev.Next = chunk;
				}
				prev = chunk;
			}

			return head;
		}
	}
}
