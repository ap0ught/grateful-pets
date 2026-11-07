using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleLib.Console;

public class TextConsole : IDisposable
{
	public static bool bExtended = false;

	public static ScreenBuffer ScrapBuffer;

	public static ScreenBuffer ScrapBuffer2;

	public static TerminalMode Mode = TerminalMode.Unity;

	public static FilterMode TexFilterMode = FilterMode.Nearest;

	public static TerminalFont TexFont = TerminalFont.Nice;

	public static Thread UIThread = null;

	private IntPtr hStdout;

	private IntPtr hStdin;

	public static string ThinkString;

	public static object BufferCS = new object();

	public static ScreenBuffer CurrentBuffer;

	public static bool _BufferUpdated = false;

	public static Queue<IScreenBufferExtra> bufferExtras = new Queue<IScreenBufferExtra>();

	public static AutoResetEvent DrawBufferEvent = new AutoResetEvent(initialState: false);

	public static bool BufferUpdated
	{
		get
		{
			return _BufferUpdated;
		}
		set
		{
			_BufferUpdated = value;
		}
	}

	public static void LoadScrapBuffers()
	{
		ScrapBuffer.Copy(CurrentBuffer);
		ScrapBuffer2.Copy(CurrentBuffer);
	}

	public static ScreenBuffer GetScrapBuffer1(bool bLoadFromCurrent = false)
	{
		if (bLoadFromCurrent)
		{
			lock (BufferCS)
			{
				ScrapBuffer.Copy(CurrentBuffer);
			}
		}
		return ScrapBuffer;
	}

	public static ScreenBuffer GetScrapBuffer2(bool bLoadFromCurrent = false)
	{
		if (bLoadFromCurrent)
		{
			lock (BufferCS)
			{
				ScrapBuffer2.Copy(CurrentBuffer);
			}
		}
		return ScrapBuffer2;
	}

	public static void ConsoleUIThread()
	{
	}

	public void Hide()
	{
	}

	public TextConsole()
	{
		CurrentBuffer = ScreenBuffer.create(80, 25);
		ScrapBuffer = ScreenBuffer.create(80, 25);
		ScrapBuffer2 = ScreenBuffer.create(80, 25);
		Keyboard.Init();
	}

	public void Dispose()
	{
	}

	public static void StopUI()
	{
		try
		{
			if (UIThread != null)
			{
				UIThread.Abort();
			}
		}
		catch
		{
		}
		Keyboard.KeyEvent.Set();
	}

	public void FocusUI()
	{
	}

	public ScreenBuffer GetCopy()
	{
		ScreenBuffer screenBuffer = ScreenBuffer.create(80, 25);
		screenBuffer.Copy(CurrentBuffer);
		return screenBuffer;
	}

	public void ShowCursor(bool bShow)
	{
	}

	public void Close()
	{
	}

	public void WaitFrame()
	{
		while (BufferUpdated)
		{
		}
	}

	public void DrawBuffer(ScreenBuffer Buffer, IScreenBufferExtra BufferExtra = null, bool bSkipIfOverlay = false)
	{
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
		lock (BufferCS)
		{
			if (!bSkipIfOverlay || !GameManager.Instance.ModernUI)
			{
				CurrentBuffer.Copy(Buffer);
				CurrentBuffer.ViewTag = GameManager.Instance?.CurrentGameView;
				if (BufferExtra != null)
				{
					bufferExtras.Enqueue(BufferExtra);
				}
			}
			BufferUpdated = true;
		}
		DrawBufferEvent.Set();
	}

	public static void WaitForDraw(int Timeout)
	{
		DrawBufferEvent.Reset();
		DrawBufferEvent.WaitOne(Timeout);
	}

	public static void Constrain(ref int X, ref int Y)
	{
		if (X < 0)
		{
			X = 0;
		}
		else if (X > 79)
		{
			X = 79;
		}
		if (Y < 0)
		{
			Y = 0;
		}
		else if (Y > 24)
		{
			Y = 24;
		}
	}

	public static void Constrain(ref int Left, ref int Right, ref int Top, ref int Bottom)
	{
		if (Left < 0)
		{
			Left = 0;
		}
		if (Right > 79)
		{
			Right = 79;
		}
		if (Top < 0)
		{
			Top = 0;
		}
		if (Bottom > 24)
		{
			Bottom = 24;
		}
	}
}
