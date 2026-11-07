using XRL.World;

namespace XRL.CharacterBuilds.Qud;

public class QudCustomizeCharacterModule : QudEmbarkBuilderModule<QudCustomizeCharacterModuleData>
{
	public string genderName
	{
		get
		{
			if (data.gender != null)
			{
				return data.gender.Name;
			}
			return "nonspecific";
		}
	}

	public string pronounSetName
	{
		get
		{
			if (data.pronounSet != null)
			{
				return data.pronounSet.Name;
			}
			return "<from gender>";
		}
	}

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
	}

	public override string DataErrors()
	{
		return null;
	}

	public override string DataWarnings()
	{
		return null;
	}

	public override void InitFromSeed(string seed)
	{
	}

	public void setPet(string pet)
	{
		if (data == null)
		{
			data = new QudCustomizeCharacterModuleData();
		}
		data.pet = (string.IsNullOrEmpty(pet) ? null : pet);
		setData(data);
	}

	public void setName(string name)
	{
		if (data == null)
		{
			data = new QudCustomizeCharacterModuleData();
		}
		data.name = (string.IsNullOrEmpty(name) ? null : name);
		setData(data);
	}

	public override void Init()
	{
		PronounSet.Reinit();
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_BEFOREBOOTPLAYEROBJECT)
		{
			string text = info.getData<QudCustomizeCharacterModuleData>()?.name?.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				game.PlayerName = text;
			}
		}
		if (id == QudGameBootModule.BOOTEVENT_AFTERBOOTPLAYEROBJECT)
		{
			GameObject gameObject = element as GameObject;
			if (data != null && data.pronounSet != null && gameObject != null)
			{
				data.pronounSet.Register();
				gameObject.SetPronounSet(data.pronounSet);
			}
			if (data != null && gameObject != null && data.gender != null)
			{
				data.gender.Register();
				gameObject.SetGender(data.gender);
			}
		}
		return base.handleBootEvent(id, game, info, element);
	}
}
