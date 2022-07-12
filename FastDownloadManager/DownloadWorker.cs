namespace FastDownloadManager
{
	internal class DownloadWorker
	{
		private readonly string _url;
		private readonly HttpClient _httpClient;

		public int Index { get; set; }
		public DownloadWorker(string url, int index)
		{
			_url = url;
			Index = index;
			_httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMinutes(15)
			};
		}

		//public async Task Start(string url, DownloadChunk chunk)
		//{
		//	var buffer = new byte[1024 * 16];
		//	var request = new HttpRequestMessage(HttpMethod.Get, url);
		//	request.Headers.Add("Range", $"bytes={chunk.Pointer}-{chunk.To}");

		//	var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
		//	response.EnsureSuccessStatusCode();

		//	var stream = await response.Content.ReadAsStreamAsync();

		//	var bytesRead = 0;
		//	do
		//	{
		//		bytesRead = await stream.ReadAsync(buffer);
		//		chunk.BytesDownloaded(buffer, 0, bytesRead);

		//	} while (stream.CanRead && bytesRead > 0);
		//}

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
