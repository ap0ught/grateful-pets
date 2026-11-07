using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AiUnity.Common.Attributes;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Internal.FileAppenders;
using AiUnity.NLog.Core.Layouts;
using UnityEngine;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("File")]
[Preserve]
public class FileTarget : TargetWithLayoutHeaderAndFooter, ICreateFileParameters
{
	private class DynamicArchiveFileHandlerClass
	{
		private readonly Queue<string> archiveFileEntryQueue;

		public int MaxArchiveFileToKeep { get; set; }

		public DynamicArchiveFileHandlerClass(int MaxArchivedFiles)
			: this()
		{
			MaxArchiveFileToKeep = MaxArchivedFiles;
		}

		public DynamicArchiveFileHandlerClass()
		{
			MaxArchiveFileToKeep = -1;
			archiveFileEntryQueue = new Queue<string>();
		}

		public bool AddToArchive(string archiveFileName, string fileName, bool createDirectoryIfNotExists)
		{
			if (MaxArchiveFileToKeep < 1)
			{
				Logger.Warn("AddToArchive is called. Even though the MaxArchiveFiles is set to less than 1");
				return false;
			}
			if (!File.Exists(fileName))
			{
				Logger.Error("Error while trying to archive, Source File : {0} Not found.", fileName);
				return false;
			}
			while (archiveFileEntryQueue.Count >= MaxArchiveFileToKeep)
			{
				string text = archiveFileEntryQueue.Dequeue();
				try
				{
					File.Delete(text);
				}
				catch (Exception ex)
				{
					Logger.Warn("Can't Delete Old Archive File : {0} , Exception : {1}", text, ex);
				}
			}
			string text2 = archiveFileName;
			if (archiveFileEntryQueue.Contains(archiveFileName))
			{
				Logger.Trace("Archive File {0} seems to be already exist. Trying with Different File Name..", archiveFileName);
				int i = 1;
				for (text2 = Path.GetFileNameWithoutExtension(archiveFileName) + ".{#}" + Path.GetExtension(archiveFileName); File.Exists(ReplaceNumber(text2, i)); i++)
				{
					Logger.Trace("Archive File {0} seems to be already exist, too. Trying with Different File Name..", archiveFileName);
				}
			}
			try
			{
				File.Move(fileName, text2);
			}
			catch (DirectoryNotFoundException)
			{
				if (!createDirectoryIfNotExists)
				{
					throw;
				}
				Logger.Trace("Directory For Archive File is not created. Creating it..");
				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
					File.Move(fileName, text2);
				}
				catch (Exception ex3)
				{
					Logger.Error("Can't create Archive File Directory , Exception : {0}", ex3);
					throw;
				}
			}
			catch (Exception ex4)
			{
				Logger.Error("Can't Archive File : {0} , Exception : {1}", fileName, ex4);
				throw;
			}
			archiveFileEntryQueue.Enqueue(archiveFileName);
			return true;
		}
	}

	private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

	private LineEndingMode lineEndingMode;

	private IFileAppenderFactory appenderFactory;

	private BaseFileAppender[] recentAppenders;

	private Timer autoClosingTimer;

	private int initializedFilesCounter;

	private int _MaxArchiveFilesField;

	private readonly DynamicArchiveFileHandlerClass dynamicArchiveFileHandler;

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[Display("File name", "Name of log file.", false, 0)]
	[DefaultValue("unity_log.txt")]
	[RequiredParameter]
	public Layout FileName { get; set; }

	[DefaultValue(true)]
	public bool CreateDirs { get; set; }

	[Display("Auto clear", "Clear existing log file on startup.  Note log file will also be clear if archiving occurs.", false, 0)]
	[DefaultValue(true)]
	public bool AutoClear { get; set; }

	[Display("Archive last", "Archive previous log file when new log created", true, 0)]
	[DefaultValue(false)]
	public bool ArchiveOldFileOnStartup { get; set; }

	[DefaultValue(false)]
	public bool ReplaceFileContentsOnEachWrite { get; set; }

	[DefaultValue(true)]
	public bool KeepFileOpen { get; set; }

	[DefaultValue(true)]
	public bool EnableFileDelete { get; set; }

	[DefaultValue("")]
	public string ArchiveDateFormat { get; set; }

	public Win32FileAttributes FileAttributes { get; set; }

	public LineEndingMode LineEnding
	{
		get
		{
			return lineEndingMode;
		}
		set
		{
			lineEndingMode = value;
			switch (value)
			{
			case LineEndingMode.CR:
				NewLineChars = "\r";
				break;
			case LineEndingMode.LF:
				NewLineChars = "\n";
				break;
			case LineEndingMode.CRLF:
				NewLineChars = "\r\n";
				break;
			case LineEndingMode.Default:
				NewLineChars = Environment.NewLine;
				break;
			case LineEndingMode.None:
				NewLineChars = string.Empty;
				break;
			}
		}
	}

	[Display("Auto flush", "Flush file buffers after each log message", false, 0)]
	[DefaultValue(true)]
	public bool AutoFlush { get; set; }

	[DefaultValue(5)]
	public int OpenFileCacheSize { get; set; }

	[DefaultValue(-1)]
	public int OpenFileCacheTimeout { get; set; }

	[DefaultValue(32768)]
	public int BufferSize { get; set; }

	public Encoding Encoding { get; set; }

	[DefaultValue(false)]
	public bool ConcurrentWrites { get; set; }

	[DefaultValue(false)]
	public bool NetworkWrites { get; set; }

	[DefaultValue(10)]
	public int ConcurrentWriteAttempts { get; set; }

	[DefaultValue(1)]
	public int ConcurrentWriteAttemptDelay { get; set; }

	[Display("Archive trigger", "Archive file when size exceeds specified size in bytes.  Use -1 to disable functionality.", true, 0)]
	[DefaultValue(-1)]
	public long ArchiveAboveSize { get; set; }

	[Display("Archive interval", "Archive file on specified interval.", true, 0)]
	[DefaultValue(FileArchivePeriod.None)]
	public FileArchivePeriod ArchiveEvery { get; set; }

	[Display("Archive name", "Name of archive file with placeholder {#} replaced by archive number sequence.", true, 0)]
	[DefaultValue("unity_log_archive_{#}.txt")]
	public Layout ArchiveFileName { get; set; }

	[Display("Max archives", "Maximum number of archive files before overwritting previous archives.", true, 0)]
	[DefaultValue(9)]
	public int MaxArchiveFiles
	{
		get
		{
			return _MaxArchiveFilesField;
		}
		set
		{
			_MaxArchiveFilesField = value;
			dynamicArchiveFileHandler.MaxArchiveFileToKeep = value;
		}
	}

	[DefaultValue(false)]
	public bool ForceManaged { get; set; }

	public ArchiveNumberingMode ArchiveNumbering { get; set; }

	protected internal string NewLineChars { get; private set; }

	public FileTarget()
	{
		ArchiveNumbering = ArchiveNumberingMode.Sequence;
		_MaxArchiveFilesField = 9;
		ConcurrentWriteAttemptDelay = 1;
		ArchiveEvery = FileArchivePeriod.None;
		ArchiveAboveSize = -1L;
		ConcurrentWriteAttempts = 10;
		ConcurrentWrites = false;
		Encoding = Encoding.Default;
		BufferSize = 32768;
		AutoFlush = true;
		FileAttributes = Win32FileAttributes.Normal;
		NewLineChars = Environment.NewLine;
		EnableFileDelete = true;
		OpenFileCacheTimeout = -1;
		OpenFileCacheSize = 5;
		CreateDirs = true;
		dynamicArchiveFileHandler = new DynamicArchiveFileHandlerClass(MaxArchiveFiles);
		ForceManaged = false;
		ArchiveDateFormat = string.Empty;
		FileName = "unity_log.txt";
		KeepFileOpen = true;
	}

	public void CleanupInitializedFiles()
	{
		CleanupInitializedFiles(DateTime.Now.AddDays(-2.0));
	}

	public void CleanupInitializedFiles(DateTime cleanupThreshold)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, DateTime> initializedFile in initializedFiles)
		{
			string key = initializedFile.Key;
			if (initializedFile.Value < cleanupThreshold)
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			WriteFooterAndUninitialize(item);
		}
	}

	protected override void FlushAsync(AsyncContinuation asyncContinuation)
	{
		try
		{
			BaseFileAppender[] array = recentAppenders;
			foreach (BaseFileAppender baseFileAppender in array)
			{
				if (baseFileAppender == null)
				{
					break;
				}
				baseFileAppender.Flush();
			}
			asyncContinuation(null);
		}
		catch (Exception exception)
		{
			if (exception.MustBeRethrown())
			{
				throw;
			}
			asyncContinuation(exception);
		}
	}

	protected override void InitializeTarget()
	{
		base.InitializeTarget();
		if (!KeepFileOpen)
		{
			throw new NotImplementedException("Feature removed for Unity");
		}
		if (ArchiveAboveSize != -1 || ArchiveEvery != FileArchivePeriod.None)
		{
			if (NetworkWrites)
			{
				throw new NotImplementedException("Feature removed for Unity");
			}
			if (ConcurrentWrites)
			{
				throw new NotImplementedException("Feature removed for Unity");
			}
			appenderFactory = CountingSingleProcessFileAppender.TheFactory;
		}
		else
		{
			if (NetworkWrites)
			{
				throw new NotImplementedException("Feature removed for Unity");
			}
			if (ConcurrentWrites)
			{
				throw new NotImplementedException("Feature removed for Unity");
			}
			appenderFactory = SingleProcessFileAppender.TheFactory;
		}
		recentAppenders = new BaseFileAppender[OpenFileCacheSize];
		if ((OpenFileCacheSize > 0 || EnableFileDelete) && OpenFileCacheTimeout > 0)
		{
			autoClosingTimer = new Timer(AutoClosingTimerCallback, null, OpenFileCacheTimeout * 1000, OpenFileCacheTimeout * 1000);
		}
	}

	protected override void CloseTarget()
	{
		base.CloseTarget();
		foreach (string item in new List<string>(initializedFiles.Keys))
		{
			WriteFooterAndUninitialize(item);
		}
		if (autoClosingTimer != null)
		{
			autoClosingTimer.Change(-1, -1);
			autoClosingTimer.Dispose();
			autoClosingTimer = null;
		}
		if (recentAppenders != null)
		{
			for (int i = 0; i < recentAppenders.Length && recentAppenders[i] != null; i++)
			{
				recentAppenders[i].Close();
				recentAppenders[i] = null;
			}
		}
	}

	protected override void Write(LogEventInfo logEvent)
	{
		string fileName = Path.Combine(Application.isEditor ? "" : Application.persistentDataPath, CleanupFileName(FileName.Render(logEvent)));
		byte[] bytesToWrite = GetBytesToWrite(logEvent);
		if (ShouldAutoArchive(fileName, logEvent, bytesToWrite.Length))
		{
			InvalidateCacheItem(fileName);
			DoAutoArchive(fileName, logEvent);
		}
		WriteToFile(fileName, bytesToWrite, justData: false);
	}

	protected override void Write(AsyncLogEventInfo[] logEvents)
	{
		Dictionary<string, List<AsyncLogEventInfo>> dictionary = logEvents.BucketSort((AsyncLogEventInfo c) => FileName.Render(c.LogEvent));
		using MemoryStream memoryStream = new MemoryStream();
		List<AsyncContinuation> list = new List<AsyncContinuation>();
		foreach (KeyValuePair<string, List<AsyncLogEventInfo>> item in dictionary)
		{
			string currentFileName = CleanupFileName(item.Key);
			memoryStream.SetLength(0L);
			memoryStream.Position = 0L;
			LogEventInfo logEventInfo = null;
			foreach (AsyncLogEventInfo item2 in item.Value)
			{
				if (logEventInfo == null)
				{
					logEventInfo = item2.LogEvent;
				}
				byte[] bytesToWrite = GetBytesToWrite(item2.LogEvent);
				memoryStream.Write(bytesToWrite, 0, bytesToWrite.Length);
				list.Add(item2.Continuation);
			}
			FlushCurrentFileWrites(currentFileName, logEventInfo, memoryStream, list);
		}
	}

	protected virtual string GetFormattedMessage(LogEventInfo logEvent)
	{
		return Layout.Render(logEvent);
	}

	protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
	{
		string s = GetFormattedMessage(logEvent) + NewLineChars;
		return TransformBytes(Encoding.GetBytes(s));
	}

	protected virtual byte[] TransformBytes(byte[] value)
	{
		return value;
	}

	private static bool IsContainValidNumberPatternForReplacement(string pattern)
	{
		int num = pattern.IndexOf("{#", StringComparison.Ordinal);
		int num2 = pattern.IndexOf("#}", StringComparison.Ordinal);
		if (num != -1 && num2 != -1)
		{
			return num < num2;
		}
		return false;
	}

	private static string ReplaceNumber(string pattern, int value)
	{
		int num = pattern.IndexOf("{#", StringComparison.Ordinal);
		int num2 = pattern.IndexOf("#}", StringComparison.Ordinal) + 2;
		int totalWidth = num2 - num - 2;
		return pattern.Substring(0, num) + Convert.ToString(value, 10).PadLeft(totalWidth, '0') + pattern.Substring(num2);
	}

	private void FlushCurrentFileWrites(string currentFileName, LogEventInfo firstLogEvent, MemoryStream ms, List<AsyncContinuation> pendingContinuations)
	{
		Exception exception = null;
		try
		{
			if (currentFileName != null)
			{
				if (ShouldAutoArchive(currentFileName, firstLogEvent, (int)ms.Length))
				{
					WriteFooterAndUninitialize(currentFileName);
					InvalidateCacheItem(currentFileName);
					DoAutoArchive(currentFileName, firstLogEvent);
				}
				WriteToFile(currentFileName, ms.ToArray(), justData: false);
			}
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			exception = ex;
		}
		foreach (AsyncContinuation pendingContinuation in pendingContinuations)
		{
			pendingContinuation(exception);
		}
		pendingContinuations.Clear();
	}

	private void RecursiveRollingRename(string fileName, string pattern, int archiveNumber)
	{
		if (archiveNumber >= MaxArchiveFiles)
		{
			File.Delete(fileName);
		}
		else
		{
			if (!File.Exists(fileName))
			{
				return;
			}
			string text = ReplaceNumber(pattern, archiveNumber);
			if (File.Exists(fileName))
			{
				RecursiveRollingRename(text, pattern, archiveNumber + 1);
			}
			Logger.Trace("Renaming {0} to {1}", fileName, text);
			try
			{
				MoveFileToArchive(fileName, text);
			}
			catch (IOException)
			{
				string directoryName = Path.GetDirectoryName(text);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				MoveFileToArchive(fileName, text);
			}
		}
	}

	private void SequentialArchive(string fileName, string pattern)
	{
		string fileName2 = Path.GetFileName(pattern);
		int num = fileName2.IndexOf("{#", StringComparison.Ordinal);
		int num2 = fileName2.IndexOf("#}", StringComparison.Ordinal) + 2;
		int num3 = fileName2.Length - num2;
		string searchPattern = fileName2.Substring(0, num) + "*" + fileName2.Substring(num2);
		string directoryName = Path.GetDirectoryName(Path.GetFullPath(pattern));
		int num4 = -1;
		int num5 = -1;
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		try
		{
			string[] files = Directory.GetFiles(directoryName, searchPattern);
			foreach (string text in files)
			{
				string fileName3 = Path.GetFileName(text);
				string value = fileName3.Substring(num, fileName3.Length - num3 - num);
				int num6;
				try
				{
					num6 = Convert.ToInt32(value);
				}
				catch (FormatException)
				{
					continue;
				}
				num4 = Math.Max(num4, num6);
				num5 = ((num5 != -1) ? Math.Min(num5, num6) : num6);
				dictionary[num6] = text;
			}
			num4++;
		}
		catch (DirectoryNotFoundException)
		{
			Directory.CreateDirectory(directoryName);
			num4 = 0;
		}
		if (num5 != -1)
		{
			int num7 = num4 - MaxArchiveFiles + 1;
			for (int j = num5; j < num7; j++)
			{
				if (dictionary.TryGetValue(j, out var value2))
				{
					File.Delete(value2);
				}
			}
		}
		string archiveFileName = ReplaceNumber(pattern, num4);
		MoveFileToArchive(fileName, archiveFileName);
	}

	private void MoveFileToArchive(string existingFileName, string archiveFileName)
	{
		File.Move(existingFileName, archiveFileName);
		string fileName = Path.GetFileName(existingFileName);
		if (fileName != null)
		{
			if (initializedFiles.ContainsKey(fileName))
			{
				initializedFiles.Remove(fileName);
			}
			else if (initializedFiles.ContainsKey(existingFileName))
			{
				initializedFiles.Remove(existingFileName);
			}
		}
	}

	private void DateAndSequentialArchive(string fileName, string pattern, LogEventInfo logEvent)
	{
		string fileName2 = Path.GetFileName(pattern);
		if (string.IsNullOrEmpty(fileName2))
		{
			return;
		}
		int length = fileName2.IndexOf("{#", StringComparison.Ordinal);
		int num = fileName2.IndexOf("#}", StringComparison.Ordinal) + 2;
		int num2 = fileName2.Length - num;
		string text = fileName2.Substring(0, length) + "*" + fileName2.Substring(num);
		string dateFormatString = GetDateFormatString(ArchiveDateFormat);
		string directoryName = Path.GetDirectoryName(Path.GetFullPath(pattern));
		if (string.IsNullOrEmpty(directoryName))
		{
			return;
		}
		bool isNextCycle = false;
		if (GetFileInfo(fileName, out var lastWriteTime, out var _))
		{
			string dateFormatString2 = GetDateFormatString(string.Empty);
			string text2 = lastWriteTime.ToString(dateFormatString2);
			string text3 = logEvent.TimeStamp.ToLocalTime().ToString(dateFormatString2);
			isNextCycle = text2 != text3;
		}
		int num3 = -1;
		try
		{
			List<string> list = (from n in new DirectoryInfo(directoryName).GetFiles(text)
				orderby n.CreationTime
				select n.FullName).ToList();
			List<string> list2 = new List<string>();
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				string fileName3 = Path.GetFileName(list[num4]);
				if (!string.IsNullOrEmpty(fileName3))
				{
					string text4 = fileName3.Substring(text.LastIndexOf('*'), dateFormatString.Length);
					string value = fileName3.Substring(text.LastIndexOf('*') + dateFormatString.Length + 1, fileName3.Length - num2 - (text.LastIndexOf('*') + dateFormatString.Length + 1));
					int val;
					try
					{
						val = Convert.ToInt32(value);
					}
					catch (FormatException)
					{
						continue;
					}
					if (text4 == GetArchiveDate(isNextCycle).ToString(dateFormatString))
					{
						num3 = Math.Max(num3, val);
					}
					if (DateTime.TryParseExact(text4, dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
					{
						list2.Add(list[num4]);
					}
				}
			}
			num3++;
			for (int num5 = 0; num5 < list2.Count && num5 <= list.Count - MaxArchiveFiles; num5++)
			{
				File.Delete(list2[num5]);
			}
		}
		catch (DirectoryNotFoundException)
		{
			Directory.CreateDirectory(directoryName);
			num3 = 0;
		}
		string archiveFileName = Path.Combine(directoryName, text.Replace("*", $"{GetArchiveDate(isNextCycle).ToString(dateFormatString)}.{num3}"));
		MoveFileToArchive(fileName, archiveFileName);
	}

	private void DateArchive(string fileName, string pattern)
	{
		string fileName2 = Path.GetFileName(pattern);
		int length = fileName2.IndexOf("{#", StringComparison.Ordinal);
		int startIndex = fileName2.IndexOf("#}", StringComparison.Ordinal) + 2;
		string text = fileName2.Substring(0, length) + "*" + fileName2.Substring(startIndex);
		string directoryName = Path.GetDirectoryName(Path.GetFullPath(pattern));
		string dateFormatString = GetDateFormatString(ArchiveDateFormat);
		try
		{
			List<string> list = (from n in new DirectoryInfo(directoryName).GetFiles(text)
				orderby n.CreationTime
				select n.FullName).ToList();
			List<string> list2 = new List<string>();
			for (int num = 0; num < list.Count; num++)
			{
				string s = Path.GetFileName(list[num]).Substring(text.LastIndexOf('*'), dateFormatString.Length);
				DateTime result = DateTime.MinValue;
				if (DateTime.TryParseExact(s, dateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					list2.Add(list[num]);
				}
			}
			for (int num2 = 0; num2 < list2.Count && num2 <= list.Count - MaxArchiveFiles; num2++)
			{
				File.Delete(list2[num2]);
			}
		}
		catch (DirectoryNotFoundException)
		{
			Directory.CreateDirectory(directoryName);
		}
		string archiveFileName = Path.Combine(directoryName, text.Replace("*", GetArchiveDate(isNextCycle: true).ToString(dateFormatString)));
		MoveFileToArchive(fileName, archiveFileName);
	}

	private string GetDateFormatString(string defaultFormat)
	{
		string text = defaultFormat;
		if (string.IsNullOrEmpty(text))
		{
			text = ArchiveEvery switch
			{
				FileArchivePeriod.Year => "yyyy", 
				FileArchivePeriod.Month => "yyyyMM", 
				FileArchivePeriod.Hour => "yyyyMMddHH", 
				FileArchivePeriod.Minute => "yyyyMMddHHmm", 
				_ => "yyyyMMdd", 
			};
		}
		return text;
	}

	private DateTime GetArchiveDate(bool isNextCycle)
	{
		DateTime result = DateTime.Now;
		int num = (isNextCycle ? (-1) : 0);
		switch (ArchiveEvery)
		{
		case FileArchivePeriod.Day:
			result = result.AddDays(num);
			break;
		case FileArchivePeriod.Hour:
			result = result.AddHours(num);
			break;
		case FileArchivePeriod.Minute:
			result = result.AddMinutes(num);
			break;
		case FileArchivePeriod.Month:
			result = result.AddMonths(num);
			break;
		case FileArchivePeriod.Year:
			result = result.AddYears(num);
			break;
		}
		return result;
	}

	private void DoAutoArchive(string fileName, LogEventInfo ev)
	{
		FileInfo fileInfo = new FileInfo(fileName);
		if (!fileInfo.Exists)
		{
			return;
		}
		string text;
		if (ArchiveFileName == null)
		{
			string extension = Path.GetExtension(fileName);
			text = Path.ChangeExtension(fileInfo.FullName, ".{#}" + extension);
		}
		else
		{
			text = ArchiveFileName.Render(ev);
		}
		if (!IsContainValidNumberPatternForReplacement(text))
		{
			if (dynamicArchiveFileHandler.AddToArchive(text, fileInfo.FullName, CreateDirs) && initializedFiles.ContainsKey(fileInfo.FullName))
			{
				initializedFiles.Remove(fileInfo.FullName);
			}
			return;
		}
		switch (ArchiveNumbering)
		{
		case ArchiveNumberingMode.Rolling:
			RecursiveRollingRename(fileInfo.FullName, text, 0);
			break;
		case ArchiveNumberingMode.Sequence:
			SequentialArchive(fileInfo.FullName, text);
			break;
		case ArchiveNumberingMode.Date:
			DateArchive(fileInfo.FullName, text);
			break;
		case ArchiveNumberingMode.DateAndSequence:
			DateAndSequentialArchive(fileInfo.FullName, text, ev);
			break;
		}
	}

	private bool ShouldAutoArchive(string fileName, LogEventInfo ev, int upcomingWriteSize)
	{
		if (ArchiveAboveSize == -1 && ArchiveEvery == FileArchivePeriod.None)
		{
			return false;
		}
		if (!GetFileInfo(fileName, out var lastWriteTime, out var fileLength))
		{
			return false;
		}
		if (ArchiveAboveSize != -1 && fileLength + upcomingWriteSize > ArchiveAboveSize)
		{
			return true;
		}
		if (ArchiveEvery != FileArchivePeriod.None)
		{
			string dateFormatString = GetDateFormatString(string.Empty);
			string text = lastWriteTime.ToString(dateFormatString);
			string text2 = ev.TimeStamp.ToLocalTime().ToString(dateFormatString);
			if (text != text2)
			{
				return true;
			}
		}
		return false;
	}

	private void AutoClosingTimerCallback(object state)
	{
		lock (SyncRoot)
		{
			if (!IsInitialized)
			{
				return;
			}
			try
			{
				DateTime dateTime = DateTime.Now.AddSeconds(-OpenFileCacheTimeout);
				for (int i = 0; i < recentAppenders.Length && recentAppenders[i] != null; i++)
				{
					if (recentAppenders[i].OpenTime < dateTime)
					{
						for (int j = i; j < recentAppenders.Length && recentAppenders[j] != null; j++)
						{
							recentAppenders[j].Close();
							recentAppenders[j] = null;
						}
						break;
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}
				Logger.Warn("Exception in AutoClosingTimerCallback: {0}", ex);
			}
		}
	}

	private void WriteToFile(string fileName, byte[] bytes, bool justData)
	{
		if (ReplaceFileContentsOnEachWrite)
		{
			using (FileStream fileStream = File.Create(fileName))
			{
				byte[] headerBytes = GetHeaderBytes();
				byte[] footerBytes = GetFooterBytes();
				if (headerBytes != null)
				{
					fileStream.Write(headerBytes, 0, headerBytes.Length);
				}
				fileStream.Write(bytes, 0, bytes.Length);
				if (footerBytes != null)
				{
					fileStream.Write(footerBytes, 0, footerBytes.Length);
				}
				return;
			}
		}
		bool flag = false;
		if (!justData)
		{
			if (!initializedFiles.ContainsKey(fileName))
			{
				if (ArchiveOldFileOnStartup)
				{
					try
					{
						DoAutoArchive(fileName, null);
					}
					catch (Exception ex)
					{
						if (ex.MustBeRethrown())
						{
							throw;
						}
						Logger.Warn("Unable to archive old log file '{0}': {1}", fileName, ex);
					}
				}
				if (AutoClear)
				{
					try
					{
						File.Delete(fileName);
					}
					catch (Exception ex2)
					{
						if (ex2.MustBeRethrown())
						{
							throw;
						}
						Logger.Warn("Unable to delete old log file '{0}': {1}", fileName, ex2);
					}
				}
				initializedFiles[fileName] = DateTime.Now;
				initializedFilesCounter++;
				flag = true;
				if (initializedFilesCounter >= 100)
				{
					initializedFilesCounter = 0;
					CleanupInitializedFiles();
				}
			}
			initializedFiles[fileName] = DateTime.Now;
		}
		BaseFileAppender baseFileAppender = null;
		int num = recentAppenders.Length - 1;
		for (int i = 0; i < recentAppenders.Length; i++)
		{
			if (recentAppenders[i] == null)
			{
				num = i;
				break;
			}
			if (recentAppenders[i].FileName == fileName)
			{
				BaseFileAppender baseFileAppender2 = recentAppenders[i];
				for (int num2 = i; num2 > 0; num2--)
				{
					recentAppenders[num2] = recentAppenders[num2 - 1];
				}
				recentAppenders[0] = baseFileAppender2;
				baseFileAppender = baseFileAppender2;
				break;
			}
		}
		if (baseFileAppender == null)
		{
			BaseFileAppender baseFileAppender3 = appenderFactory.Open(fileName, this);
			try
			{
				if (recentAppenders[num] != null)
				{
					recentAppenders[num].Close();
					recentAppenders[num] = null;
				}
			}
			catch (Exception ex3)
			{
				Logger.Error(ex3.ToString());
			}
			for (int num3 = num; num3 > 0; num3--)
			{
				recentAppenders[num3] = recentAppenders[num3 - 1];
			}
			recentAppenders[0] = baseFileAppender3;
			baseFileAppender = baseFileAppender3;
		}
		if (flag && (!baseFileAppender.GetFileInfo(out var _, out var fileLength) || fileLength == 0L))
		{
			byte[] headerBytes2 = GetHeaderBytes();
			if (headerBytes2 != null)
			{
				baseFileAppender.Write(headerBytes2);
			}
		}
		baseFileAppender.Write(bytes);
		if (AutoFlush)
		{
			baseFileAppender.Flush();
		}
	}

	private byte[] GetHeaderBytes()
	{
		if (Header == null)
		{
			return null;
		}
		string s = Header.Render(LogEventInfo.CreateNullEvent()) + NewLineChars;
		return TransformBytes(Encoding.GetBytes(s));
	}

	private byte[] GetFooterBytes()
	{
		if (Footer == null)
		{
			return null;
		}
		string s = Footer.Render(LogEventInfo.CreateNullEvent()) + NewLineChars;
		return TransformBytes(Encoding.GetBytes(s));
	}

	private void WriteFooterAndUninitialize(string fileName)
	{
		byte[] footerBytes = GetFooterBytes();
		if (footerBytes != null && File.Exists(fileName))
		{
			WriteToFile(fileName, footerBytes, justData: true);
		}
		initializedFiles.Remove(fileName);
	}

	private bool GetFileInfo(string fileName, out DateTime lastWriteTime, out long fileLength)
	{
		BaseFileAppender[] array = recentAppenders;
		foreach (BaseFileAppender baseFileAppender in array)
		{
			if (baseFileAppender == null)
			{
				break;
			}
			if (baseFileAppender.FileName == fileName)
			{
				baseFileAppender.GetFileInfo(out lastWriteTime, out fileLength);
				return true;
			}
		}
		FileInfo fileInfo = new FileInfo(fileName);
		if (fileInfo.Exists)
		{
			fileLength = fileInfo.Length;
			lastWriteTime = fileInfo.LastWriteTime;
			return true;
		}
		fileLength = -1L;
		lastWriteTime = DateTime.MinValue;
		return false;
	}

	private void InvalidateCacheItem(string fileName)
	{
		for (int i = 0; i < recentAppenders.Length && recentAppenders[i] != null; i++)
		{
			if (recentAppenders[i].FileName == fileName)
			{
				recentAppenders[i].Close();
				for (int j = i; j < recentAppenders.Length - 1; j++)
				{
					recentAppenders[j] = recentAppenders[j + 1];
				}
				recentAppenders[recentAppenders.Length - 1] = null;
				break;
			}
		}
	}

	private static string CleanupFileName(string fileName)
	{
		int num = fileName.LastIndexOfAny(new char[2]
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		});
		string seed = fileName.Substring(num + 1);
		string path = ((num > 0) ? fileName.Substring(0, num) : string.Empty);
		seed = Path.GetInvalidFileNameChars().Aggregate(seed, (string current, char c) => current.Replace(c, '_'));
		return Path.Combine(path, seed);
	}
}
