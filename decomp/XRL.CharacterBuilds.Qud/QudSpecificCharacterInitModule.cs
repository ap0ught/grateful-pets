using System;
using XRL.UI;
using XRL.World;

namespace XRL.CharacterBuilds.Qud;

public class QudSpecificCharacterInitModule : AbstractEmbarkBuilderModule
{
	public override void InitFromSeed(string seed)
	{
	}

	public override void Init()
	{
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_BEFOREBOOTPLAYEROBJECT)
		{
			string text = "Humanoid";
			GenotypeEntry genotypeEntry = builder.GetModule<QudGenotypeModule>()?.data?.Entry;
			if (genotypeEntry != null)
			{
				text = (genotypeEntry.BodyObject.IsNullOrEmpty() ? text : genotypeEntry.BodyObject);
			}
			SubtypeEntry subtypeEntry = builder.GetModule<QudSubtypeModule>()?.data?.Entry;
			if (subtypeEntry != null)
			{
				text = (subtypeEntry.BodyObject.IsNullOrEmpty() ? text : subtypeEntry.BodyObject);
			}
			text = info.fireBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT, game, text);
			GameObject gameObject = GameObject.Create(text);
			if (gameObject == null)
			{
				Popup.ShowAsync("Error creating player body. Unknown blueprint \"" + text + "\"").Wait();
				throw new Exception("Unknown blueprint " + text);
			}
			gameObject.BaseID = 1;
			gameObject.InjectGeneID("OriginalPlayer");
			gameObject.Brain.Allegiance.Clear();
			gameObject.Brain.Allegiance["Player"] = 100;
			string value = subtypeEntry?.Species ?? genotypeEntry?.Species;
			if (!string.IsNullOrEmpty(value))
			{
				gameObject.SetStringProperty("Species", value);
			}
			gameObject.SetStringProperty("OriginalPlayerBody", "1");
			gameObject.SetIntProperty("Renamed", 1);
			return gameObject;
		}
		return base.handleBootEvent(id, game, info, element);
	}
}
