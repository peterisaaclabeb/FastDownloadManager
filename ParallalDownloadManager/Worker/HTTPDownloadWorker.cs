namespace ParallalDownloadManager.Worker
{
	internal class HTTPDownloadWorker : IDownloadWorker
	{
		private readonly string _url;
		private readonly HttpClient _httpClient;

		public int Index { get; set; }
		public HTTPDownloadWorker(string url, int index)
		{
			_url = url;
			Index = index;
			_httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMinutes(15)
			};
		}

		public async Task<Stream> GetRangeAsync(long from, long to, CancellationToken token)
		{

			var request = new HttpRequestMessage(HttpMethod.Get, _url);
			request.Headers.Add("Range", $"bytes={from}-{to}");

			var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
			response.EnsureSuccessStatusCode();

			var stream = await response.Content.ReadAsStreamAsync(token);

			return stream;
		}
	}
}
