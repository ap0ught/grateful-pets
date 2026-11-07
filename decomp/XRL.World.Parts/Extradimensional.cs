using System;

namespace XRL.World.Parts;

[Serializable]
public class Extradimensional : IPart
{
	public string DimensionName;

	public int WeaponModIndex;

	public int MissileWeaponModIndex;

	public int ArmorModIndex;

	public int ShieldModIndex;

	public int MiscModIndex;

	public string Training;

	public string SecretID;

	public Extradimensional()
	{
		DimensionName = "";
	}

	public Extradimensional(string DimensionName, int WeaponModIndex, int MissileWeaponModIndex, int ArmorModIndex, int ShieldModIndex, int MiscModIndex, string Training, string SecretID)
		: this()
	{
		this.DimensionName = DimensionName;
		this.WeaponModIndex = WeaponModIndex;
		this.MissileWeaponModIndex = MissileWeaponModIndex;
		this.ArmorModIndex = ArmorModIndex;
		this.ShieldModIndex = ShieldModIndex;
		this.Training = Training;
		this.SecretID = SecretID;
		this.MiscModIndex = MiscModIndex;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(DimensionName))
		{
			E.Postfix.Compound("This creature is native to the dimension known as " + DimensionName + ".", '\n');
		}
		return base.HandleEvent(E);
	}
}
