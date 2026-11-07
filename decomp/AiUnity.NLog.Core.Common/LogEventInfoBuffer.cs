using System;

namespace AiUnity.NLog.Core.Common;

public class LogEventInfoBuffer
{
	private readonly bool growAsNeeded;

	private readonly int growLimit;

	private AsyncLogEventInfo[] buffer;

	private int getPointer;

	private int putPointer;

	private int count;

	public int Size => buffer.Length;

	public LogEventInfoBuffer(int size, bool growAsNeeded, int growLimit)
	{
		this.growAsNeeded = growAsNeeded;
		buffer = new AsyncLogEventInfo[size];
		this.growLimit = growLimit;
		getPointer = 0;
		putPointer = 0;
	}

	public int Append(AsyncLogEventInfo eventInfo)
	{
		lock (this)
		{
			if (count >= buffer.Length)
			{
				if (growAsNeeded && buffer.Length < growLimit)
				{
					int num = buffer.Length * 2;
					if (num >= growLimit)
					{
						num = growLimit;
					}
					AsyncLogEventInfo[] destinationArray = new AsyncLogEventInfo[num];
					Array.Copy(buffer, 0, destinationArray, 0, buffer.Length);
					buffer = destinationArray;
				}
				else
				{
					getPointer++;
				}
			}
			putPointer %= buffer.Length;
			buffer[putPointer] = eventInfo;
			putPointer++;
			count++;
			if (count >= buffer.Length)
			{
				count = buffer.Length;
			}
			return count;
		}
	}

	public AsyncLogEventInfo[] GetEventsAndClear()
	{
		lock (this)
		{
			int num = count;
			AsyncLogEventInfo[] array = new AsyncLogEventInfo[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = (getPointer + i) % buffer.Length;
				AsyncLogEventInfo asyncLogEventInfo = buffer[num2];
				buffer[num2] = default;
				array[i] = asyncLogEventInfo;
			}
			count = 0;
			getPointer = 0;
			putPointer = 0;
			return array;
		}
	}
}
