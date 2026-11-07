using System;
using System.Text;

namespace XRL.World.Parts;

public abstract class INephalChord : IPart
{
	public const string BASE_DESC = "This creature burns bright in the chord of ";

	[NonSerialized]
	private string _DisplayName;

	public abstract string Source { get; }

	public virtual string SourceName => _DisplayName ?? (_DisplayName = GameObjectFactory.Factory.GetBlueprint(Source).CachedDisplayNameStripped);

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
		E.Postfix.Append("\n{{rules|").Append("This creature burns bright in the chord of ").Append("{{")
			.AppendLower(Source)
			.Append('|')
			.Append(SourceName)
			.Append(".}}");
		AppendRules(E.Postfix);
		E.Postfix.Append("}}");
		return base.HandleEvent(E);
	}

	public abstract void AppendRules(StringBuilder Postfix);
}
