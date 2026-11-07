using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;
using Qud.API;
using XRL.Language;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public class FactionInterest : IComposite
{
	public string _Tags;

	public List<string> _TagList;

	public bool WillBuy = true;

	public bool WillSell = true;

	public bool MatchAny;

	public bool Inverse;

	public string Description;

	public int Weight;

	public string SourceFileName;

	[NonSerialized]
	public int SourceLineNumber;

	[NonSerialized]
	public bool SourceWasMod;

	[NonSerialized]
	private string DescriptionCache;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	public string Tags
	{
		get
		{
			return _Tags;
		}
		set
		{
			_Tags = value;
			_TagList = null;
		}
	}

	public List<string> TagList
	{
		get
		{
			if (_TagList == null)
			{
				if (_Tags != null)
				{
					_TagList = new List<string>(_Tags.Split(','));
				}
				else
				{
					_TagList = new List<string>();
				}
			}
			return _TagList;
		}
		set
		{
			Tags = string.Join(",", value.ToArray());
			_TagList = value;
		}
	}

	public string DebugName
	{
		get
		{
			string tags = Tags;
			tags = ((!MatchAny) ? tags.Replace(",", "+") : tags.Replace(",", "/"));
			if (Inverse)
			{
				tags = "!" + tags;
			}
			tags += (WillBuy ? "+B" : "-B");
			tags += (WillSell ? "+S" : "-S");
			if (!string.IsNullOrEmpty(SourceFileName))
			{
				tags = tags + "(" + SourceFileName + ":" + SourceLineNumber + (SourceWasMod ? "[mod]" : "") + ")";
			}
			return tags;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(_Tags);
		Writer.Write(_TagList);
		Writer.Write(WillBuy);
		Writer.Write(WillSell);
		Writer.Write(MatchAny);
		Writer.Write(Inverse);
		Writer.WriteOptimized(Description);
		Writer.WriteOptimized(Weight);
		Writer.WriteOptimized(SourceFileName);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		_Tags = Reader.ReadOptimizedString();
		_TagList = Reader.ReadList<string>();
		WillBuy = Reader.ReadBoolean();
		WillSell = Reader.ReadBoolean();
		MatchAny = Reader.ReadBoolean();
		Inverse = Reader.ReadBoolean();
		Description = Reader.ReadOptimizedString();
		Weight = Reader.ReadOptimizedInt32();
		SourceFileName = Reader.ReadOptimizedString();
	}

	public bool SameAs(FactionInterest o)
	{
		if (o._Tags != _Tags)
		{
			return false;
		}
		if (o.WillBuy != WillBuy)
		{
			return false;
		}
		if (o.WillSell != WillSell)
		{
			return false;
		}
		if (o.MatchAny != MatchAny)
		{
			return false;
		}
		if (o.Inverse != Inverse)
		{
			return false;
		}
		if (o.Description != Description)
		{
			return false;
		}
		return true;
	}

	public string GetDescription(Faction faction)
	{
		if (DescriptionCache == null)
		{
			DescriptionCache = (string.IsNullOrEmpty(Description) ? GenerateDescription(faction) : Description);
		}
		return DescriptionCache;
	}

	public bool HasDescriptionOverride()
	{
		return !string.IsNullOrEmpty(Description);
	}

	public string GenerateDescription(Faction faction)
	{
		if (TagList.Count == 1)
		{
			switch (Tags)
			{
			case "gossip":
				return "all sorts of gossip";
			case "sultan":
				return "all sultans";
			case "tech":
				return "technology";
			case "cybernetics":
				return "the locations of becoming nooks";
			case "settlement":
				return "the locations of all settlements";
			case "consortium":
				return "the locations of Consortium merchants";
			case "underground":
				return "all underground locations";
			case "mountains":
				return "locations in the mountains";
			case "jungle":
				return "locations in the jungle";
			case "bananagrove":
				return "locations in the banana grove";
			case "saltdunes":
				return "locations in the salt dunes";
			case "saltmarsh":
				return "locations in the salt marsh";
			case "desertcanyons":
				return "locations in the desert canyons";
			case "flowerfields":
				return "locations in the flower fields";
			case "fungal":
				return "the locations of fungus forests";
			case "lakehinnom":
				return "locations around Lake Hinnom";
			case "palladiumreef":
				return "locations in the Palladium Reef";
			case "moonstair":
				return "locations in the Moon Stair";
			case "girsh":
				return "the locations of Girsh lairs";
			case "book":
				return "books";
			case "ruins":
				return "the locations of ruins";
			case "historic":
				return "the locations of historic sites";
			case "dromad":
				if (!(faction.Name == "Dromad"))
				{
					return "the locations of dromad merchants";
				}
				return "the locations of other dromad merchants";
			case "merchant":
				if (!(faction.Name == "Merchants"))
				{
					return "the locations of merchants";
				}
				return "the locations of other merchants";
			case "slimy":
				return "the locations of slime bogs";
			case "rusty":
				return "the locations of rust bogs";
			case "oddity":
				return "the locations of oddities";
			case "weep":
				return "all weep locations";
			case "recipe":
				return "cooking recipes";
			case "stopsvalinn":
				return "the location of Stopsvalinn";
			case "artifact":
				return "the locations of storied items";
			case "mountainstream":
				return "locations along mountain streams";
			case "Ptoh":
				return "the darkling star";
			case "rebekah":
				return "Resheph's healer Rebekah";
			case "sultanTombPropaganda":
				return "sultan tomb inscriptions";
			case "kindrish":
				return "the location of the ancestral bracelet Kindrish";
			case "*":
				return "everything";
			}
			if (Tags.StartsWith("river"))
			{
				return "locations along the River " + char.ToUpper(Tags[5]) + Tags.Substring(6);
			}
			if (Tags.EndsWith("duskwaters"))
			{
				string text = Tags.Replace("duskwaters", "");
				return "locations in the River " + char.ToUpper(text[0]) + text.Substring(1) + "'s duskwaters";
			}
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.GetTag("LairAdjectives") == Tags)
				{
					string text2 = Grammar.MakePossessive(Grammar.Pluralize(ColorUtility.StripFormatting(blueprint.DisplayName())));
					string text3 = Grammar.Pluralize(blueprint.GetTag("LairName", "lair"));
					return "the locations of " + text2 + " " + text3;
				}
			}
			if (Tags.StartsWith("gossip:"))
			{
				string text4 = Tags.Substring(7);
				if (text4 == faction.Name)
				{
					return "gossip that's about them";
				}
				Faction ifExists = Factions.GetIfExists(text4);
				if (ifExists != null)
				{
					return "gossip that's about " + ifExists.GetFormattedName();
				}
				return "gossip that's about " + text4;
			}
		}
		if (TagList.Count == 2 && TagList[1] == "lair")
		{
			if (TagList[0] == "templar")
			{
				return "the locations of Putus Templar enclaves";
			}
			if (TagList[0] == "oboroqoru")
			{
				return "Oboroqoru's lair";
			}
			if (faction.DisplayName.Contains(TagList[0]))
			{
				return "the locations of other " + Grammar.Pluralize(string.Join(" ", TagList.ToArray()));
			}
		}
		if (TagList.Count == 2 && TagList[1] == "settlement")
		{
			if (TagList[0] == "snapjaw")
			{
				if (faction.Name == "Snapjaws")
				{
					return "the locations of their forts";
				}
				return "the locations of snapjaw forts";
			}
			if (TagList[0] == "pig")
			{
				return "the locations of pig farms";
			}
			if (TagList[0] == "apple")
			{
				return "the locations of apple farms";
			}
		}
		return "the locations of " + Grammar.Pluralize(string.Join(" ", TagList.ToArray()));
	}

	public bool AppliesTo(IBaseJournalEntry note)
	{
		if (Tags == "*")
		{
			return !Inverse;
		}
		if (MatchAny)
		{
			foreach (string tag in TagList)
			{
				if (note.Has(tag))
				{
					return !Inverse;
				}
			}
			return Inverse;
		}
		foreach (string tag2 in TagList)
		{
			if (!note.Has(tag2))
			{
				return Inverse;
			}
		}
		return !Inverse;
	}

	public bool AppliesTo(List<string> Parts)
	{
		if (Tags == "*")
		{
			return !Inverse;
		}
		if (MatchAny)
		{
			foreach (string tag in TagList)
			{
				if (Parts.Contains(tag))
				{
					return !Inverse;
				}
			}
			return Inverse;
		}
		foreach (string tag2 in TagList)
		{
			if (!Parts.Contains(tag2))
			{
				return Inverse;
			}
		}
		return !Inverse;
	}

	public bool AppliesTo(string Spec)
	{
		return AppliesTo(Spec.CachedCommaExpansion());
	}

	public bool Includes(IBaseJournalEntry note, bool Buy, bool Sell)
	{
		if (Buy && !WillBuy)
		{
			return false;
		}
		if (Sell && !WillSell)
		{
			return false;
		}
		return AppliesTo(note);
	}

	public bool Includes(List<string> Parts, bool Buy, bool Sell)
	{
		if (Buy && !WillBuy)
		{
			return false;
		}
		if (Sell && !WillSell)
		{
			return false;
		}
		return AppliesTo(Parts);
	}

	public bool Includes(string Spec, bool Buy, bool Sell)
	{
		return Includes(Spec.CachedCommaExpansion(), Buy, Sell);
	}

	public bool Excludes(IBaseJournalEntry note, bool Buy, bool Sell)
	{
		if (Buy && WillBuy)
		{
			return false;
		}
		if (Sell && WillSell)
		{
			return false;
		}
		return AppliesTo(note);
	}

	public bool Excludes(List<string> Parts, bool Buy, bool Sell)
	{
		if (Buy && WillBuy)
		{
			return false;
		}
		if (Sell && WillSell)
		{
			return false;
		}
		return AppliesTo(Parts);
	}

	public bool Excludes(string Spec, bool Buy, bool Sell)
	{
		return Excludes(Spec.CachedCommaExpansion(), Buy, Sell);
	}
}
