using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
public class GripChange : IPart
{
	public const string ABL_CMD = "CommandChangeGrip";

	public Guid AbilityID;

	private static readonly string[] Skills = new string[4] { "Axe", "Cudgel", "LongBlades", "ShortBlades" };

	private static readonly string[] EquipSounds = new string[4] { "sfx_equip_weapon_axe_metal_heavy", "sfx_equip_weapon_bludgeon_metal_heavy", "sfx_equip_weapon_blade_long_metal_heavy", "sfx_equip_weapon_blade_short_metal_heavy" };

	[NonSerialized]
	private List<string> Options;

	[NonSerialized]
	private List<char> Hotkeys;

	[NonSerialized]
	private List<IRenderable> Icons;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("CommandChangeGrip", "change grip", "CommandChangeGrip", null, 'c');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "CommandChangeGrip" && TryChooseGrip(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (AbilityID == Guid.Empty && ParentObject.IsEquippedProperly(E.Part))
		{
			Renderable uITileDefault = null;
			if (ParentObject.TryGetPart<MeleeWeapon>(out var Part) && SkillFactory.Factory.TryGetFirstEntry(Part.Skill, out var Entry))
			{
				uITileDefault = new Renderable(Entry.Tile, " ", "&" + Entry.Foreground, null, Entry.Detail[0]);
			}
			AbilityID = E.Actor.AddActivatedAbility("Change Grip", "CommandChangeGrip", "Items", null, "é", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, null, uITileDefault);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RemoveActivatedAbility(ref AbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandChangeGrip" && TryChooseGrip(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public bool TryChooseGrip(GameObject Actor)
	{
		if (!Actor.IsPlayer())
		{
			return false;
		}
		List<string> list = Options ?? (Options = new List<string>());
		List<char> list2 = Hotkeys ?? (Hotkeys = new List<char>());
		List<IRenderable> list3 = Icons ?? (Icons = new List<IRenderable>());
		list.Clear();
		list2.Clear();
		list3.Clear();
		MeleeWeapon part = ParentObject.GetPart<MeleeWeapon>();
		if (part == null)
		{
			return false;
		}
		int num = Array.IndexOf(Skills, part.Skill);
		int i = 0;
		for (int num2 = Skills.Length; i < num2; i++)
		{
			IBaseSkillEntry firstEntry = SkillFactory.Factory.GetFirstEntry(Skills[i]);
			list.Add((i == num) ? ("{{W|" + firstEntry.Name + "}}") : firstEntry.Name);
			list2.Add(ControlManager.GetHotkeyCharFor(firstEntry.Name, list2));
			list3.Add(new Renderable(firstEntry.Tile, " ", "&" + firstEntry.Foreground, null, firstEntry.Detail[0]));
		}
		int num3 = Popup.PickOption("Choose a style", null, "", "Sounds/UI/ui_notification", list, list2, list3, null, null, null, null, 0, 60, num, -1, AllowEscape: true);
		if (num3 < 0 || num3 >= Skills.Length)
		{
			return false;
		}
		if (num3 != num)
		{
			part.Skill = Skills[num3];
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null)
			{
				SetAbilityIcon(equipped, (Renderable)list3[num3]);
				equipped.PlayWorldSound(EquipSounds[num3]);
			}
		}
		return true;
	}

	public void SetAbilityIcon(GameObject Actor, Renderable Renderable)
	{
		ActivatedAbilityEntry activatedAbility = Actor.GetActivatedAbility(AbilityID);
		if (activatedAbility != null)
		{
			activatedAbility.UITileDefault = Renderable;
		}
	}
}
