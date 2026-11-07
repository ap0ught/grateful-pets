using System.Collections.Generic;

namespace XRL.World.AI;

public sealed class OpinionMap : Dictionary<int, OpinionList>, IComposite
{
	public bool WantFieldReflection => false;

	public OpinionList this[GameObject Object]
	{
		get
		{
			if (!Object.HasID)
			{
				return null;
			}
			return base[Object.BaseID];
		}
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Count);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, OpinionList> current = enumerator.Current;
			Writer.WriteOptimized(current.Key);
			current.Value.Write(Writer);
		}
	}

	public void Read(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		for (int i = 0; i < num; i++)
		{
			int key = Reader.ReadOptimizedInt32();
			OpinionList opinionList = new OpinionList();
			opinionList.Read(Reader);
			Add(key, opinionList);
		}
	}

	public void ClearExpired()
	{
		XRLGame game = The.Game;
		if (game == null)
		{
			return;
		}
		List<int> list = null;
		long timeTicks = game.TimeTicks;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				var (item, opinionList2) = enumerator.Current;
				for (int num2 = opinionList2.Count - 1; num2 >= 0; num2--)
				{
					IOpinion opinion = opinionList2[num2];
					int duration = opinion.Duration;
					if (duration > 0 && timeTicks - opinion.Time >= duration)
					{
						opinionList2.RemoveAt(num2);
					}
				}
				if (opinionList2.Count == 0)
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(item);
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (int item2 in list)
		{
			Remove(item2);
		}
	}
}
