using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MumblesInfection : IPart
{
	public int Chance = 5;

	[NonSerialized]
	public Dictionary<string, bool> Visited = new Dictionary<string, bool>();

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Visited = Reader.ReadDictionary<string, bool>();
		base.Read(Basis, Reader);
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(Visited);
		base.Write(Basis, Writer);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("Equipped");
		Registrar.Register("Unequipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (ParentObject.Physics.Equipped != null && ParentObject.Physics.Equipped.IsPlayer())
			{
				string zoneID = IComponent<GameObject>.ThePlayer.Physics.CurrentCell.ParentZone.ZoneID;
				if (!IComponent<GameObject>.ThePlayer.Physics.CurrentCell.ParentZone.IsWorldMap())
				{
					if (Visited.ContainsKey(zoneID))
					{
						return true;
					}
					Visited.Add(zoneID, value: true);
					if (Stat.Random(0, 100) <= Chance)
					{
						IBaseJournalEntry randomUnrevealedNote = JournalAPI.GetRandomUnrevealedNote();
						JournalMapNote obj = randomUnrevealedNote as JournalMapNote;
						string text = "";
						text = ((obj == null) ? randomUnrevealedNote.Text : ("The location of " + Grammar.InitLowerIfArticle(randomUnrevealedNote.Text)));
						Popup.Show("The mouths on your skin begin to mumble coherently, revealing the wisdom of a trillion microbes:\n\n" + text);
						randomUnrevealedNote.Reveal(ParentObject.DisplayName);
						Achievement.LEARN_SECRET_FROM_MUMBLEMOUTH.Unlock();
					}
				}
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "EnteredCell");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "EnteredCell");
		}
		return base.FireEvent(E);
	}
}
