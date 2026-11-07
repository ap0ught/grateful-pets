using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL;

[Serializable]
public class MutationEntry : IPartEntry
{
	public MutationCategory Category;

	public string Exclusions;

	public string Variant;

	public string Type;

	public bool Ranked;

	public bool Defect;

	public string XMLDisplayName;

	public string Help = "";

	public string Stat = "";

	public string Tile;

	public string Foreground = "w";

	public string Detail = "W";

	public string Property = "";

	public string ForceProperty;

	private string[] _Exclusions;

	public int Maximum = 1;

	public string BiomeTable;

	public string BiomeAdjective = "";

	public string BiomeEpithet = "";

	public int MaxLevel = 10;

	public bool Prerelease;

	private BaseMutation _Mutation;

	private Type _MutationType;

	private bool? _HasVariants;

	private bool? _CanSelectVariant;

	[Obsolete("Use Name if you want the stable ID, or GetDisplayName() if you want a string for UI display.")]
	public string DisplayName
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	[Obsolete("Use Variant field.")]
	public string Constructor
	{
		get
		{
			return Variant;
		}
		set
		{
			Variant = value;
		}
	}

	public string BearerDescription
	{
		get
		{
			return Snippet;
		}
		set
		{
			Snippet = value;
		}
	}

	public BaseMutation Mutation => _Mutation ?? (_Mutation = Mutations.GetGenericMutation(Class, Variant));

	public override IPart Instance => Mutation;

	public Type MutationType
	{
		get
		{
			if ((object)_MutationType == null)
			{
				_MutationType = ModManager.ResolveType("XRL.World.Parts.Mutation." + Mutation);
			}
			return _MutationType;
		}
	}

	public bool HasVariants
	{
		get
		{
			bool valueOrDefault = _HasVariants == true;
			if (!_HasVariants.HasValue)
			{
				valueOrDefault = Mutation?.HasVariants ?? false;
				_HasVariants = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanSelectVariant => (_CanSelectVariant ?? (_CanSelectVariant = Mutation?.CanSelectVariant)) ?? true;

	public string GetStat()
	{
		if (!string.IsNullOrEmpty(Stat))
		{
			return Stat;
		}
		return Category?.Stat;
	}

	public string GetProperty()
	{
		if (!string.IsNullOrEmpty(Property))
		{
			return Property;
		}
		return Category?.Property;
	}

	public string GetCategoryForceProperty()
	{
		return Category?.ForceProperty;
	}

	public string GetForceProperty()
	{
		if (string.IsNullOrEmpty(ForceProperty))
		{
			ForceProperty = "MutationBonus_" + Class;
		}
		return ForceProperty;
	}

	public string[] GetExclusions()
	{
		if (_Exclusions == null)
		{
			if (!string.IsNullOrEmpty(Exclusions))
			{
				_Exclusions = Exclusions.Split(',');
			}
			else
			{
				_Exclusions = new string[0];
			}
		}
		return _Exclusions;
	}

	public List<string> GetVariants()
	{
		return Mutation?.GetVariants();
	}

	public BaseMutation CreateInstance()
	{
		return BaseMutation.Create(this);
	}

	public IRenderable GetRenderable()
	{
		if (Tile != null)
		{
			return new Renderable
			{
				Tile = Tile,
				ColorString = "&" + Foreground,
				DetailColor = Detail[0]
			};
		}
		return null;
	}

	public bool OkWith(MutationEntry Entry, bool CheckOther = true, bool allowMultipleDefects = false)
	{
		if (Entry == this || Entry == null)
		{
			return true;
		}
		if (CheckOther && !Entry.OkWith(this, CheckOther: false, allowMultipleDefects))
		{
			return false;
		}
		if (!allowMultipleDefects && !Options.DisableDefectLimit && Entry.IsDefect() && IsDefect())
		{
			return false;
		}
		string[] exclusions = GetExclusions();
		foreach (string text in exclusions)
		{
			if (text == Entry.Name)
			{
				return false;
			}
			if (text.Length > 0 && text[0] == '*' && text.Length == Entry.Category.Name.Length + 1 && text.EndsWith(Entry.Category.Name))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsDefect()
	{
		return Defect;
	}

	public bool IsMental()
	{
		BaseMutation mutation = Mutation;
		if (mutation == null)
		{
			return false;
		}
		return mutation.GetMutationType()?.Contains("Mental") == true;
	}

	public bool IsPhysical()
	{
		BaseMutation mutation = Mutation;
		if (mutation == null)
		{
			return false;
		}
		return mutation.GetMutationType()?.Contains("Physical") == true;
	}

	public string GetDisplayName(bool WithAnnotations = false)
	{
		string text = XMLDisplayName ?? Name;
		if (WithAnnotations && IsDefect())
		{
			return text + " ({{r|D}})";
		}
		return text;
	}

	public override void HandleXMLNode(XmlDataHelper Reader)
	{
		base.HandleXMLNode(Reader);
		Name = Reader.ParseAttribute("Name", Name);
		XMLDisplayName = Reader.ParseAttribute("DisplayName", XMLDisplayName);
		Variant = Reader.ParseAttribute("Variant", Variant);
		Type = Reader.ParseAttribute("Type", Type);
		Tile = Reader.ParseAttribute("Tile", Tile);
		Foreground = Reader.ParseAttribute("Foreground", Foreground);
		Detail = Reader.ParseAttribute("Detail", Detail);
		Stat = Reader.ParseAttribute("Stat", Stat);
		Property = Reader.ParseAttribute("Property", Property);
		ForceProperty = Reader.ParseAttribute("ForceProperty", ForceProperty);
		BiomeTable = Reader.ParseAttribute("BiomeTable", BiomeTable);
		BiomeAdjective = Reader.ParseAttribute("BiomeAdjective", BiomeAdjective);
		BiomeEpithet = Reader.ParseAttribute("BiomeEpithet", BiomeEpithet);
		Maximum = Reader.ParseAttribute("MaxSelected", Maximum);
		MaxLevel = Reader.ParseAttribute("MaxLevel", MaxLevel);
		Ranked = Reader.ParseAttribute("Ranked", Ranked);
		Defect = Reader.ParseAttribute("Defect", Defect);
		Exclusions = Reader.ParseAttribute("Exclusions", Exclusions);
		Prerelease = Reader.ParseAttribute("Prerelease", Prerelease);
		if (Snippet.IsNullOrEmpty())
		{
			Snippet = Reader.ParseAttribute("BearerDescription", "");
		}
	}
}
