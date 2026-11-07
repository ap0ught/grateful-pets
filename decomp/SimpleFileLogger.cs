using System;
using System.Threading.Tasks;
using Platform.IO;
using XRL;

public class SimpleFileLogger
{
	private string filePath;

	public static async Task<SimpleFileLogger> Create(string filePath)
	{
		SimpleFileLogger logger = new SimpleFileLogger
		{
			filePath = DataManager.SavePath(filePath)
		};
		if (await File.ExistsAsync(filePath))
		{
			(await Blob.CopyAsync(filePath, filePath + ".prev", overwrite: true)).LogIfErrored();
		}
		(await Blob.WriteAllTextAsync(filePath, "--log start--\n")).LogIfErrored();
		return logger;
	}

	public void Info(string logRaw)
	{
		string content = DecorateLog(logRaw);
		File.AppendAllText(filePath, content);
	}

	public void Error(string logRaw)
	{
		string content = DecorateLog(logRaw);
		File.AppendAllText(filePath, content);
	}

	private string DecorateLog(string log)
	{
		return string.Concat("[" + DateTime.Now.ToString("s") + "] ", log, "\n");
	}
}
