namespace ConsoleLib.Console;

public abstract class IConsoleCharExtra
{
	public abstract void Clear(bool overtyping = false);

	public abstract IConsoleCharExtra Copy();

	public virtual void BeforeRender(int x, int y, ConsoleChar ch, ex3DSprite2 sprite, ScreenBuffer buffer)
	{
	}

	public virtual void AfterRender(int x, int y, ConsoleChar ch, ex3DSprite2 sprite, ScreenBuffer buffer)
	{
	}
}
