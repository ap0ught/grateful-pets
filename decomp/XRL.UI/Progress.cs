using System;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Qud.UI;

namespace XRL.UI;

public class Progress
{
	private string currentProgressText = "initializing...";

	private int currentProgress;

	private ScreenBuffer original = ScreenBuffer.create(80, 25);

	private ScreenBuffer buffer;

	public void setCurrentProgressText(string text)
	{
		currentProgressText = text;
		draw();
	}

	public void setCurrentProgress(int pos)
	{
		currentProgress = pos;
		draw();
	}

	public bool isCancelled()
	{
		return false;
	}

	public void draw()
	{
		if (Options.ModernUI)
		{
			SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync(currentProgressText);
			return;
		}
		buffer.Copy(original);
		string s = currentProgressText;
		int num = 15;
		int num2 = 65;
		int num3 = 10;
		int y = 18;
		buffer.Fill(num, num3, num2, y, 32, ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		buffer.SingleBox(num, num3, num2, y, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		buffer.Goto(num + 5, num3 + 3);
		buffer.Write(s);
		ScrollbarHelper.Paint(buffer, num3 + 5, num + 5, num2 - num - 10, ScrollbarHelper.Orientation.Horizontal, 0, 100, 0, currentProgress);
		buffer.Draw();
	}

	public void start(Action<Progress> a)
	{
		if (Options.ModernUI)
		{
			original.Copy(TextConsole.CurrentBuffer);
			buffer = TextConsole.GetScrapBuffer1();
		}
		GameManager.Instance.PushGameView("Popup:Progress");
		a(this);
		if (Options.ModernUI)
		{
			SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync(null);
		}
		else
		{
			original.Draw();
		}
		GameManager.Instance.PopGameView();
	}

	public async Task<T> startAsync<T>(Func<Progress, Task<T>> a)
	{
		T result = default;
		try
		{
			GameManager.Instance.PushGameView("Popup:Progress");
			result = await a(this);
		}
		finally
		{
			await SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync(null);
			GameManager.Instance.PopGameView();
		}
		return result;
	}
}
