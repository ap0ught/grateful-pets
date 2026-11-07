using System;
using System.Collections.Generic;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud;

public class QudPregenModule : QudEmbarkBuilderModule<QudPregenModuleData>
{
	public class QudPregenData : FrameworkDataElement
	{
		public string Name;

		public string Genotype;

		public string Tile;

		public string Code;

		public string Foreground;

		public string Background;

		public string Detail;
	}

	public Dictionary<string, QudPregenData> pregens = new Dictionary<string, QudPregenData>();

	private QudPregenData currentReadingQudPregenData;

	public override Dictionary<string, Action<XmlDataHelper>> XmlNodes
	{
		get
		{
			Dictionary<string, Action<XmlDataHelper>> xmlNodes = base.XmlNodes;
			xmlNodes.Add("pregens", (XmlDataHelper xml) =>
			{
				xml.HandleNodes(XmlNodeHandlers);
			});
			return xmlNodes;
		}
	}

	public Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
	{
		{
			"pregen",
			(XmlDataHelper xml) =>
			{
				string attribute = xml.GetAttribute("Name");
				if (!pregens.TryGetValue(attribute, out currentReadingQudPregenData))
				{
					currentReadingQudPregenData = new QudPregenData
					{
						Name = attribute
					};
					pregens.Add(attribute, currentReadingQudPregenData);
				}
				currentReadingQudPregenData.Name = xml.GetAttribute("Name");
				currentReadingQudPregenData.Id = attribute;
				currentReadingQudPregenData.Genotype = xml.GetAttribute("Genotype");
				currentReadingQudPregenData.Tile = xml.GetAttribute("Tile");
				currentReadingQudPregenData.Foreground = xml.GetAttribute("Foreground");
				currentReadingQudPregenData.Background = xml.GetAttribute("Background");
				currentReadingQudPregenData.Detail = xml.GetAttribute("Detail");
				xml.HandleNodes(XmlNodeHandlers);
				currentReadingQudPregenData = null;
			}
		},
		{
			"code",
			(XmlDataHelper xml) =>
			{
				currentReadingQudPregenData.Code = xml.GetTextNode();
			}
		},
		{
			"description",
			(XmlDataHelper xml) =>
			{
				currentReadingQudPregenData.Description = xml.GetTextNode();
			}
		}
	};

	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudChartypeModule>()?.data?.type == "Pregen";
	}

	public void SelectPregen(string pregen)
	{
		setData(new QudPregenModuleData(pregen));
		builder.InitModulesFromCode(pregens[pregen].Code);
		builder.advanceToSummary();
	}

	public override void InitFromSeed(string seed)
	{
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE && data.Pregen != null)
		{
			return pregens[data.Pregen].Tile;
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEFOREGROUND && data.Pregen != null)
		{
			return pregens[data.Pregen].Foreground;
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEBACKGROUND && data.Pregen != null)
		{
			return pregens[data.Pregen].Background;
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEDETAIL && data.Pregen != null)
		{
			return pregens[data.Pregen].Detail;
		}
		return base.handleBootEvent(id, game, info, element);
	}
}
