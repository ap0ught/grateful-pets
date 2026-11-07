using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ColdBlooded : BaseMutation
{
	public int _SpeedBonus;

	public int SpeedBonus
	{
		get
		{
			return _SpeedBonus;
		}
		set
		{
			if (_SpeedBonus != value)
			{
				if (_SpeedBonus > 0)
				{
					ParentObject.Statistics["Speed"].Bonus -= _SpeedBonus;
				}
				if (_SpeedBonus < 0)
				{
					ParentObject.Statistics["Speed"].Penalty -= -_SpeedBonus;
				}
				_SpeedBonus = value;
				if (_SpeedBonus > 0)
				{
					ParentObject.Statistics["Speed"].Bonus += _SpeedBonus;
				}
				if (_SpeedBonus < 0)
				{
					ParentObject.Statistics["Speed"].Penalty += -_SpeedBonus;
				}
			}
		}
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "Your vitality depends on your temperature; at higher temperatures, you are more lively. At lower temperatures, you are more torpid.\n\nYour base quickness score is reduced by 10.\nYour quickness increases as your temperature increases and decreases as your temperature decreases.\n+100 reputation with {{w|unshelled reptiles}}";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<EnvironmentalUpdateEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("ice", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnvironmentalUpdateEvent E)
	{
		int temperature = ParentObject.Physics.Temperature;
		if (temperature == 25)
		{
			SpeedBonus = -10;
		}
		else if (temperature < 25)
		{
			double num = 90.0;
			int num2 = Math.Min(25 - temperature + 25, 3000);
			for (int i = 26; i <= num2; i++)
			{
				num -= 1250.0 / (double)((i + 25) * (i + 25));
			}
			SpeedBonus = (int)(num - 100.0);
		}
		else
		{
			double num3 = 90.0;
			for (int j = 26; j <= temperature; j++)
			{
				num3 += 1250.0 / (double)((j + 25) * (j + 25));
			}
			SpeedBonus = (int)(num3 - 100.0);
		}
		return base.HandleEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		SpeedBonus = 0;
		return base.Unmutate(GO);
	}
}
