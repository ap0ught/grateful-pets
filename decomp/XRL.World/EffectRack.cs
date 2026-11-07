using XRL.Collections;

namespace XRL.World;

public class EffectRack : Rack<Effect>
{
	public EffectRack()
	{
	}

	public EffectRack(int Capacity)
		: base(Capacity)
	{
	}

	public void FinalizeRead(GameObject Basis, SerializationReader Reader)
	{
		Effect[] items = Items;
		int i = 0;
		for (int length = Length; i < length; i++)
		{
			Effect effect = items[i];
			effect.ApplyRegistrar(Basis);
			effect.FinalizeRead(Reader);
			if (length != Length)
			{
				length = Length;
				items = Items;
				if (i < length && items[i] != effect)
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
			Effect effect2 = items[j];
			if (!ComponentReflection.EffectExtensions.TryGetValue(effect2.GetType(), out var value))
			{
				continue;
			}
			value.FinalizeRead(Basis, effect2);
			if (length2 != Length)
			{
				length2 = Length;
				for (items = Items; effect2 != items[j] && j < length2; j++)
				{
				}
			}
		}
	}
}
