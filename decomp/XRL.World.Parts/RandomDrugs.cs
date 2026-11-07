using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class RandomDrugs : IPart
{
	public bool Shrooms;

	public bool bFirst = true;

	public string Number = "1d2+1";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public void AddRandomMed()
	{
		int num = Stat.Random(1, 8);
		if (num == 1)
		{
			ParentObject.ApplyEffect(new Blaze_Tonic(1000));
		}
		if (num == 2)
		{
			ParentObject.ApplyEffect(new HulkHoney_Tonic(1000));
		}
		if (num == 3)
		{
			ParentObject.ApplyEffect(new Rubbergum_Tonic(1000));
		}
		if (num == 4)
		{
			ParentObject.ApplyEffect(new Salve_Tonic(30));
		}
		if (num == 5)
		{
			ParentObject.ApplyEffect(new ShadeOil_Tonic(1000));
		}
		if (num == 6)
		{
			ParentObject.ApplyEffect(new Skulk_Tonic(1000));
		}
		if (num == 7)
		{
			ParentObject.ApplyEffect(new SphynxSalt_Tonic(1000));
		}
		if (num == 8)
		{
			ParentObject.ApplyEffect(new Ubernostrum_Tonic(30));
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		if (bFirst)
		{
			bFirst = false;
			if (Shrooms)
			{
				ParentObject.ApplyEffect(new Hoarshroom_Tonic(1000));
				ParentObject.ApplyEffect(new Hoarshroom_Tonic(1000));
				ParentObject.ApplyEffect(new Blaze_Tonic(1000));
				ParentObject.ApplyEffect(new SphynxSalt_Tonic(1000));
			}
			else
			{
				int num = Stat.Roll(Number);
				for (int i = 0; i < num; i++)
				{
					AddRandomMed();
				}
			}
		}
		return base.HandleEvent(E);
	}
}
