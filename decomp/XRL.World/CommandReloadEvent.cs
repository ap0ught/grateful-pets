using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Singleton)]
public class CommandReloadEvent : SingletonEvent<CommandReloadEvent>
{
	public new static readonly int CascadeLevel = 17;

	public const int PASSES = 3;

	public GameObject Actor;

	public GameObject Weapon;

	public GameObject LastAmmo;

	public List<IComponent<GameObject>> CheckedForReload = new List<IComponent<GameObject>>();

	public List<IComponent<GameObject>> NeededReload = new List<IComponent<GameObject>>();

	public List<IComponent<GameObject>> TriedToReload = new List<IComponent<GameObject>>();

	public List<IComponent<GameObject>> Reloaded = new List<IComponent<GameObject>>();

	public List<GameObject> ObjectsReloaded = new List<GameObject>();

	public bool FreeAction;

	public bool FromDialog;

	public int MinimumCharge;

	public int MaxEnergyCost;

	public int TotalEnergyCost;

	public int Pass;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Weapon = null;
		LastAmmo = null;
		CheckedForReload.Clear();
		NeededReload.Clear();
		TriedToReload.Clear();
		Reloaded.Clear();
		ObjectsReloaded.Clear();
		FreeAction = false;
		FromDialog = false;
		MinimumCharge = 0;
		MaxEnergyCost = 0;
		TotalEnergyCost = 0;
		Pass = 0;
	}

	public void EnergyCost(int amount)
	{
		TotalEnergyCost += amount;
		if (amount > MaxEnergyCost)
		{
			MaxEnergyCost = amount;
		}
	}

	public static bool Execute(GameObject Actor, bool FreeAction = false, bool FromDialog = false, int MinimumCharge = 0)
	{
		if (Actor.WantEvent(SingletonEvent<CommandReloadEvent>.ID, CascadeLevel))
		{
			if (!Actor.CanMoveExtremities("Reload", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			SingletonEvent<CommandReloadEvent>.Instance.Reset();
			SingletonEvent<CommandReloadEvent>.Instance.Actor = Actor;
			SingletonEvent<CommandReloadEvent>.Instance.Weapon = null;
			SingletonEvent<CommandReloadEvent>.Instance.FreeAction = FreeAction;
			SingletonEvent<CommandReloadEvent>.Instance.FromDialog = FromDialog;
			SingletonEvent<CommandReloadEvent>.Instance.MinimumCharge = MinimumCharge;
			for (int i = 1; i <= 3; i++)
			{
				SingletonEvent<CommandReloadEvent>.Instance.Pass = i;
				if (!Actor.HandleEvent(SingletonEvent<CommandReloadEvent>.Instance))
				{
					return false;
				}
			}
			if (!FreeAction)
			{
				Actor.UseEnergy(SingletonEvent<CommandReloadEvent>.Instance.MaxEnergyCost, "Reload");
			}
		}
		return true;
	}

	public static bool Execute(GameObject Actor, GameObject Weapon, GameObject LastAmmo = null, bool FreeAction = false, bool FromDialog = false, int MinimumCharge = 0)
	{
		if (Weapon.WantEvent(SingletonEvent<CommandReloadEvent>.ID, CascadeLevel))
		{
			if (!Actor.CanMoveExtremities("Reload", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			SingletonEvent<CommandReloadEvent>.Instance.Reset();
			SingletonEvent<CommandReloadEvent>.Instance.Actor = Actor;
			SingletonEvent<CommandReloadEvent>.Instance.Weapon = Weapon;
			SingletonEvent<CommandReloadEvent>.Instance.LastAmmo = LastAmmo;
			SingletonEvent<CommandReloadEvent>.Instance.FreeAction = FreeAction;
			SingletonEvent<CommandReloadEvent>.Instance.FromDialog = FromDialog;
			SingletonEvent<CommandReloadEvent>.Instance.MinimumCharge = MinimumCharge;
			for (int i = 1; i <= 3; i++)
			{
				SingletonEvent<CommandReloadEvent>.Instance.Pass = i;
				if (!Weapon.HandleEvent(SingletonEvent<CommandReloadEvent>.Instance))
				{
					return false;
				}
			}
			if (!FreeAction)
			{
				Actor.UseEnergy(SingletonEvent<CommandReloadEvent>.Instance.MaxEnergyCost, "Reload");
			}
		}
		return true;
	}
}
