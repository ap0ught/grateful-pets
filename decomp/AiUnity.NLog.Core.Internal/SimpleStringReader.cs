namespace AiUnity.NLog.Core.Internal;

internal class SimpleStringReader
{
	private readonly string text;

	internal int Position { get; set; }

	internal string Text => text;

	public SimpleStringReader(string text)
	{
		this.text = text;
		Position = 0;
	}

	internal int Peek()
	{
		if (Position < text.Length)
		{
			return text[Position];
		}
		return -1;
	}

	internal int Read()
	{
		if (Position < text.Length)
		{
			return text[Position++];
		}
		return -1;
	}

	internal string Substring(int p0, int p1)
	{
		return text.Substring(p0, p1 - p0);
	}
}
