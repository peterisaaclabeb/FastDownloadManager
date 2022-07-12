namespace ParallalDownloadManager.DataStore
{
	internal class FileDataStore : IDataStore
	{
		private string _filepath;
		private FileStream _filestream;

		public FileDataStore(string name)
		{
			var baseDirectory = Path.Combine(Path.GetTempPath(), "download-manager");
			Directory.CreateDirectory(baseDirectory);
			_filepath = Path.Combine(baseDirectory, name);
		}

		public long Length
		{
			get
			{
				if (_filestream != null)
				{
					return _filestream.Length;
				}
				using var stream = File.OpenWrite(_filepath);
				return stream.Length;
			}
		}

		public Stream DataStream => File.OpenRead(_filepath);

		public void Open()
		{
			_filestream = new FileStream(_filepath, FileMode.Append, FileAccess.Write);
		}

		public void Close()
		{
			_filestream.Close();

		}

		public void Write(byte[] buffer, int offset, int bytesCount)
		{
			_filestream.Write(buffer, offset, bytesCount);
			_filestream.Flush();
		}

		public void Dispose()
		{
			_filestream.Close();
		}
	}
}
