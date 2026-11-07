using ConsoleLib.Console;
using Genkit;

public class QudScreenBufferExtra : IScreenBufferExtra
{
	public Point2D playerPosition = Point2D.invalid;

	public QudScreenBufferExtra setPlayerPosition(Point2D pos)
	{
		playerPosition = pos;
		return this;
	}

	public void Clear()
	{
		playerPosition = Point2D.invalid;
	}

	public void Free()
	{
		ImposterManager.freeExtra(this);
	}
}
