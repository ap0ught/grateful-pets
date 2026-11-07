using XRL.Collections;

namespace XRL.World;

public class IPartPool
{
	private RingDeque<IPart> InternalPool;

	public int Count => InternalPool.Count;

	public int Capacity => InternalPool.Capacity;

	public IPartPool()
	{
		InternalPool = new RingDeque<IPart>();
	}

	public IPartPool(int Capacity)
	{
		InternalPool = new RingDeque<IPart>(Capacity);
	}

	public bool TryGet(out IPart Part)
	{
		return InternalPool.TryDequeue(out Part);
	}

	public void Return(IPart Part)
	{
		InternalPool.Enqueue(Part);
	}
}
