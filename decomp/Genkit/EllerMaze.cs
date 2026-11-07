namespace Genkit;

public class EllerMaze
{
	public static Maze Generate(int Width, int Height)
	{
		return new Maze(Width, Height, InitialValue: false);
	}
}
