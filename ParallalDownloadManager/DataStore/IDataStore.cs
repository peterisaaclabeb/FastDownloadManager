
namespace ParallalDownloadManager.DataStore
{
	internal interface IDataStore : IDisposable
	{
		Stream DataStream { get; }
		long Length { get; }

		void Close();
		void Open();
		void Write(byte[] buffer, int offset, int bytesCount);
	}
}