using XRL.Collections;

namespace XRL.World;

public class PartRack : Rack<IPart>
{
	public PartRack()
	{
	}

	public PartRack(int Capacity)
		: base(Capacity)
	{
	}

	public void FinalizeRead(GameObject Basis, SerializationReader Reader)
	{
		IPart[] items = Items;
		int i = 0;
		for (int length = Length; i < length; i++)
		{
			IPart part = items[i];
			part.ApplyRegistrar(Basis);
			part.FinalizeRead(Reader);
			if (length != Length)
			{
				length = Length;
				items = Items;
				if (i < length && items[i] != part)
				{
					i--;
				}
			}
		}
		if (Reader.ExtensionHash == ComponentReflection.ExtensionHash)
		{
			return;
		}
		int j = 0;
		for (int length2 = Length; j < length2; j++)
		{
			IPart part2 = items[j];
			if (!ComponentReflection.PartExtensions.TryGetValue(part2.GetType(), out var value))
			{
				continue;
			}
			value.FinalizeRead(Basis, part2);
			if (length2 != Length)
			{
				length2 = Length;
				for (items = Items; part2 != items[j] && j < length2; j++)
				{
				}
			}
		}
	}
}
