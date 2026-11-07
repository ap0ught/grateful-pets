using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XRL.Collections;
using XRL.Messages;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class Skills : IPart
{
	[NonSerialized]
	public List<BaseSkill> SkillList = new List<BaseSkill>();

	[NonSerialized]
	public static readonly StringMap<string> WeaponClassName = new StringMap<string>
	{
		{ "ShortBlades", "short-blade" },
		{ "LongBlades", "long-blade" },
		{ "Cudgel", "cudgel" },
		{ "Axe", "axe" },
		{ "Pistol", "pistol" },
		{ "Rifles", "bow and rifle" },
		{ "HeavyWeapons", "heavy weapon" }
	};

	[NonSerialized]
	private static Dictionary<string, BaseSkill> GenericSkills = new Dictionary<string, BaseSkill>(32);

	public static BaseSkill GetGenericSkill(string Skill, GameObject Actor = null)
	{
		BaseSkill value = Actor?.GetPart(Skill) as BaseSkill;
		if (value != null)
		{
			return value;
		}
		if (GenericSkills.TryGetValue(Skill, out value))
		{
			return value;
		}
		Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + Skill);
		if (type == null)
		{
			Debug.LogWarning("Cannot resolve skill type for " + Skill);
			return null;
		}
		if (!(Activator.CreateInstance(type) is BaseSkill baseSkill))
		{
			Debug.LogWarning(Skill + " is not a skill");
			return null;
		}
		GenericSkills[Skill] = baseSkill;
		return baseSkill;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		Skills obj = base.DeepCopy(Parent) as Skills;
		obj.SkillList = new List<BaseSkill>(SkillList);
		return obj;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		Writer.Write(SkillList.Count);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		Reader.ReadInt32();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void AddSkill(string Class)
	{
		if (!ParentObject.HasSkill(Class) && ModManager.CreateInstance("XRL.World.Parts.Skill." + Class) is BaseSkill newSkill)
		{
			AddSkill(newSkill);
		}
	}

	public bool AddSkill(BaseSkill NewSkill, GameObject Source = null, string Context = null)
	{
		if (ParentObject.HasSkill(NewSkill.Name))
		{
			return false;
		}
		NewSkill.ParentObject = ParentObject;
		BeforeAddSkillEvent E = BeforeAddSkillEvent.FromSkill(NewSkill, Source, Context);
		IBaseSkillEntry entry = E.Entry;
		BeforeAddSkillEvent.Send(E);
		if (!NewSkill.AddSkill(ParentObject))
		{
			return false;
		}
		ParentObject.AddPart(NewSkill);
		SkillList.Add(NewSkill);
		List<BaseSkill> list = null;
		if (!E.Include.IsNullOrEmpty())
		{
			list = new List<BaseSkill>(E.Include.Count);
			foreach (IBaseSkillEntry item2 in E.Include)
			{
				if (ModManager.CreateInstance("XRL.World.Parts.Skill." + item2.Class) is BaseSkill item)
				{
					list.Add(item);
				}
			}
		}
		BeforeAddSkillEvent.ResetTo(ref E);
		if (!list.IsNullOrEmpty())
		{
			string context = Context.AddDelimitedSubstring(',', "Inclusion");
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!AddSkill(list[num], Source, context))
				{
					list.RemoveAt(num);
				}
			}
		}
		AfterAddSkillEvent.Send(ParentObject, NewSkill, entry, Source, Context, list);
		return true;
	}

	public void RemoveSkill(BaseSkill Skill)
	{
		if (Skill == null)
		{
			return;
		}
		BeforeRemoveSkillEvent.Send(ParentObject, Skill);
		if (Skill.RemoveSkill(ParentObject))
		{
			ParentObject.RemovePart(Skill);
			SkillList.Remove(Skill);
			if (SkillFactory.Factory.SkillByClass.TryGetValue(Skill.Name, out var value))
			{
				foreach (PowerEntry value2 in value.Powers.Values)
				{
					if (value2.Cost == 0 && ParentObject.GetPart(value2.Class) is BaseSkill skill)
					{
						RemoveSkill(skill);
					}
				}
			}
		}
		AfterRemoveSkillEvent.Send(ParentObject, Skill);
	}

	[WishCommand(null, null, Command = "skill")]
	public static void WishSkill(string argument)
	{
		if (WishSkillAdd(argument))
		{
			return;
		}
		foreach (SkillEntry value in SkillFactory.Factory.SkillByClass.Values)
		{
			if (value.Name.EqualsNoCase(argument) && WishSkillAdd(value.Class))
			{
				return;
			}
		}
		foreach (PowerEntry value2 in SkillFactory.Factory.PowersByClass.Values)
		{
			if (value2.Name.EqualsNoCase(argument) && WishSkillAdd(value2.Class))
			{
				return;
			}
		}
		Popup.Show("No skill by that name could be found.");
	}

	[WishCommand(null, null, Command = "allskills")]
	public static void WishSkillAll()
	{
		Popup.Suppress = true;
		MessageQueue.Suppress = true;
		foreach (SkillEntry value in SkillFactory.Factory.SkillByClass.Values)
		{
			WishSkillAdd(value.Class);
		}
		foreach (PowerEntry value2 in SkillFactory.Factory.PowersByClass.Values)
		{
			WishSkillAdd(value2.Class);
		}
		Popup.Suppress = false;
		MessageQueue.Suppress = false;
		IComponent<GameObject>.XDidY(IComponent<GameObject>.ThePlayer, "gain", "all the skills", "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
	}

	private static bool WishSkillAdd(string Class)
	{
		Skills skills = IComponent<GameObject>.ThePlayer?.GetPart<Skills>();
		if (skills == null)
		{
			return false;
		}
		Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + Class, IgnoreCase: true);
		if (type == null)
		{
			return false;
		}
		if (!(Activator.CreateInstance(type) is BaseSkill baseSkill))
		{
			return false;
		}
		IComponent<GameObject>.XDidY(IComponent<GameObject>.ThePlayer, "gain", "the skill " + baseSkill.DisplayName, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		skills.AddSkill(baseSkill);
		return true;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (BaseSkill skill in SkillList)
		{
			stringBuilder.Append(skill.Name).Append('\n');
		}
		return stringBuilder.ToString();
	}
}
