using System;

namespace XRL.World.ZoneBuilders;

[Serializable]
public sealed class RoomData : IComposite
{
	public int Left = 999;

	public int Right;

	public int Top = 999;

	public int Bottom;

	public int Width;

	public int Height;

	public int Size;

	public int[,] Room;

	public bool WantFieldReflection => false;

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Left);
		Writer.WriteOptimized(Right);
		Writer.WriteOptimized(Top);
		Writer.WriteOptimized(Bottom);
		Writer.WriteOptimized(Width);
		Writer.WriteOptimized(Height);
		Writer.WriteOptimized(Size);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Writer.WriteOptimized(Room[i, j]);
			}
		}
	}

	public void Read(SerializationReader Reader)
	{
		Left = Reader.ReadOptimizedInt32();
		Right = Reader.ReadOptimizedInt32();
		Top = Reader.ReadOptimizedInt32();
		Bottom = Reader.ReadOptimizedInt32();
		Width = Reader.ReadOptimizedInt32();
		Height = Reader.ReadOptimizedInt32();
		Size = Reader.ReadOptimizedInt32();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Room[i, j] = Reader.ReadOptimizedInt32();
			}
		}
	}
}
