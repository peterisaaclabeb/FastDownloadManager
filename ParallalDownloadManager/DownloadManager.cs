namespace ParallalDownloadManager
{
	internal class DownloadManager
	{
		private readonly HttpClient _httpClient;

		public List<Download> Downloads { get; set; }

		public DownloadManager()
		{
			_httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMinutes(15)
			};
			Downloads = new List<Download>();
		}

		public async Task<Download> CreateDownloadAsync(string url, int threads = 4)
		{

			var (length, filename) = await GetFileDetailsAsync(url);

			var download = new Download(url, filename, length, threads);

			Downloads.Add(download);
			return download;
		}

		private async Task<(long, string)> GetFileDetailsAsync(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Options, url);
			var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

			response.EnsureSuccessStatusCode();

			var filename = response.Content.Headers.ContentDisposition.FileName.Trim('\"');
			var contentLength = response.Content.Headers.ContentLength.Value;

			return (contentLength, filename);
		}

	}
}
