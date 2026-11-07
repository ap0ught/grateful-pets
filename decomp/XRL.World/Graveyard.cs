using XRL.Collections;

namespace XRL.World;

public class Graveyard
{
	private static RingDeque<Graveyard> Pool = new RingDeque<Graveyard>(4);

	public RingDeque<GameObject> Objects;

	public int MaxCount;

	public static Graveyard Get()
	{
		if (!Pool.TryDequeue(out var Value))
		{
			return new Graveyard(256);
		}
		return Value;
	}

	public static void Return(Graveyard Graveyard)
	{
		Graveyard.ReleaseObjects();
		Pool.Enqueue(Graveyard);
	}

	public static void Return(ref Graveyard Graveyard)
	{
		Return(Graveyard);
		Graveyard = null;
	}

	public Graveyard(int Capacity)
		: this(Capacity, Capacity)
	{
	}

	public Graveyard(int Capacity, int MaxCount)
	{
		Objects = new RingDeque<GameObject>(Capacity);
		this.MaxCount = MaxCount;
	}

	public void ReleaseObjects()
	{
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			Objects[num].Pool();
		}
		Objects.Clear();
	}

	public void ReleaseObjects(int Count)
	{
		int count = Objects.Count;
		if (Count >= count)
		{
			ReleaseObjects();
			return;
		}
		for (int i = 0; i < Count; i++)
		{
			Objects.Dequeue().Pool();
		}
	}

	public void Add(GameObject Object)
	{
		if (!Object.IsInvalid())
		{
			if (Objects.Count >= MaxCount)
			{
				Objects.Dequeue().Pool();
			}
			Object.Flags |= 4;
			Objects.Enqueue(Object);
		}
	}
}
