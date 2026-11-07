using System.Text;
using ConsoleLib.Console;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class FactionsLineData : PooledFrameworkDataElement<FactionsLineData>
{
	public string id;

	public string label;

	public IRenderable icon;

	public bool expanded = true;

	public string name;

	public int rep;

	private static StringBuilder searchBulder = new StringBuilder();

	public string _searchText;

	public string searchText
	{
		get
		{
			if (_searchText == null)
			{
				searchBulder.Length = 0;
				Faction faction = Factions.Get(id);
				searchBulder.Append(faction.Name);
				searchBulder.Append(" ");
				searchBulder.Append(faction.GetFeelingText());
				searchBulder.Append(" ");
				searchBulder.Append(faction.GetRankText());
				searchBulder.Append(" ");
				searchBulder.Append(faction.GetPetText());
				searchBulder.Append(" ");
				searchBulder.Append(faction.GetHolyPlaceText());
				searchBulder.Append(" ");
				searchBulder.Append(Faction.GetPreferredSecretDescription(id));
				_searchText = searchBulder.ToString().ToLower();
			}
			return _searchText;
		}
		set
		{
			_searchText = value?.ToLower();
		}
	}

	public FactionsLineData set(string id, string label, IRenderable icon, bool expanded)
	{
		this.id = id;
		this.label = label;
		this.icon = icon;
		this.expanded = expanded;
		searchText = null;
		Faction faction = Factions.Get(id);
		name = faction.DisplayName;
		rep = Faction.PlayerReputation.Get(id);
		return this;
	}

	public override void free()
	{
		id = null;
		label = null;
		icon = null;
		base.free();
	}
}
