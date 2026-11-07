using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public class Worshippable : IComposite
{
	public string Name;

	public string Faction;

	public string Sources;

	public string Blueprints;

	public int Power;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Name);
		Writer.WriteOptimized(Faction);
		Writer.WriteOptimized(Sources);
		Writer.WriteOptimized(Blueprints);
		Writer.WriteOptimized(Power);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Name = Reader.ReadOptimizedString();
		Faction = Reader.ReadOptimizedString();
		Sources = Reader.ReadOptimizedString();
		Blueprints = Reader.ReadOptimizedString();
		Power = Reader.ReadOptimizedInt32();
	}

	public int GetRelevance(GameObject Object = null, int Base = 0)
	{
		int num = Base + Power.DiminishingReturns(1.0);
		if (Object?.GetPrimaryFaction() == Faction)
		{
			num *= 2;
			num += 10;
		}
		return num;
	}

	public int GetCounterRelevance(GameObject Object = null, int Base = 0)
	{
		int num = Base + Power.DiminishingReturns(1.0);
		string text = Object?.GetPrimaryFaction();
		if (!text.IsNullOrEmpty())
		{
			int factionWorshipAttitude = Factions.Get(text).GetFactionWorshipAttitude(Faction);
			if (factionWorshipAttitude < 0)
			{
				num = num * (100 - factionWorshipAttitude) / 100;
				num += factionWorshipAttitude / -10;
			}
		}
		return num;
	}

	public static int Sort(Worshippable A, Worshippable B)
	{
		int num = A.Power.CompareTo(B.Power);
		if (num != 0)
		{
			return -num;
		}
		int num2 = A.Name.CompareTo(B.Name);
		if (num2 != 0)
		{
			return num2;
		}
		return A.Faction.CompareTo(B.Faction);
	}

	public static bool Multiple(List<Worshippable> List, string Name)
	{
		if (List == null)
		{
			return false;
		}
		bool flag = false;
		foreach (Worshippable item in List)
		{
			if (item.Name == Name)
			{
				if (flag)
				{
					return true;
				}
				flag = true;
			}
		}
		return false;
	}

	public static bool Multiple(List<Worshippable> List, Worshippable Being)
	{
		return Multiple(List, Being.Name);
	}

	public override string ToString()
	{
		return "Name: " + Name + "\nFaction: " + Faction + "\nSources: " + Sources + "\nBlueprints: " + Blueprints + "\nPower: " + Power;
	}
}
