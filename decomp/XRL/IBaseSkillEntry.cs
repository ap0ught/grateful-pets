using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL;

public abstract class IBaseSkillEntry : IPartEntry
{
	public string Tile;

	public string Foreground = "w";

	public string Detail = "B";

	public string Description;

	/// <summary>Entry name will be rendered as '???' until requirements are met.</summary>
	private BaseSkill _Generic;

	public BaseSkill Generic => _Generic ?? (_Generic = Skills.GetGenericSkill(Class));

	public override IPart Instance => Generic;

	public abstract bool MeetsRequirements(GameObject Object, bool ShowPopup = false);

	public string GetFormattedDescription()
	{
		return Generic.GetDescription(this);
	}

	public override void HandleXMLNode(XmlDataHelper Reader)
	{
		Tile = Reader.ParseAttribute("Tile", Tile);
		Foreground = Reader.ParseAttribute("Foreground", Foreground);
		Detail = Reader.ParseAttribute("Detail", Detail);
		Description = Reader.ParseAttribute("Description", Description);
		string defaultValue = null;
		defaultValue = Reader.ParseAttribute("ReplaceDescriptionIfMod", defaultValue);
		if (!defaultValue.IsNullOrEmpty())
		{
			int num = defaultValue.IndexOf(':');
			if (num != -1 && ModManager.ModLoadedBySpec(defaultValue.Substring(0, num)))
			{
				Description = defaultValue.Substring(num + 1);
			}
		}
		string defaultValue2 = null;
		defaultValue2 = Reader.ParseAttribute("AddDescriptionIfMod", defaultValue2);
		if (!defaultValue2.IsNullOrEmpty())
		{
			int num2 = defaultValue2.IndexOf(':');
			if (num2 != -1 && ModManager.ModLoadedBySpec(defaultValue2.Substring(0, num2)))
			{
				Description = Description + " " + defaultValue2.Substring(num2 + 1);
			}
		}
		string defaultValue3 = null;
		defaultValue3 = Reader.ParseAttribute("AddDescription", defaultValue3);
		if (!defaultValue3.IsNullOrEmpty())
		{
			Description = Description + " " + defaultValue3;
		}
		base.HandleXMLNode(Reader);
	}
}
