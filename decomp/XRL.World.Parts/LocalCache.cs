using System;
using XRL.Collections;

namespace XRL.World.Parts;

[Serializable]
public class LocalCache : IPart
{
	private struct Entry
	{
		public GameObject Object;

		public int X;

		public int Y;

		public void Write(SerializationWriter Writer)
		{
			Writer.WriteGameObject(Object);
			Writer.WriteOptimized(X);
			Writer.WriteOptimized(Y);
		}

		public void Read(SerializationReader Reader)
		{
			Object = Reader.ReadGameObject();
			X = Reader.ReadOptimizedInt32();
			Y = Reader.ReadOptimizedInt32();
		}
	}

	[NonSerialized]
	private Rack<Entry> Values;

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Entry[] array = Values?.GetArray();
		int num = Values?.Count ?? 0;
		Writer.WriteOptimized(num);
		for (int i = 0; i < num; i++)
		{
			array[i].Write(Writer);
		}
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		if (num > 0)
		{
			Values = new Rack<Entry>(num);
			for (int i = 0; i < num; i++)
			{
				Entry item = default;
				item.Read(Reader);
				Values.Add(item);
			}
		}
	}
}
