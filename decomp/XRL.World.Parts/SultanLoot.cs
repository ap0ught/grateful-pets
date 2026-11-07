using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class SultanLoot : IPart
{
	public int Period = 1;

	public bool generated;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!generated)
		{
			generated = true;
			ParentObject.Inventory.AddObject(generateFace());
			generateRelics().ForEach((GameObject r) =>
			{
				ParentObject.Inventory.AddObject(r);
			});
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public GameObject generateFace()
	{
		GameObject result = null;
		if (Period == 1)
		{
			result = GameObject.Create("The Kesil Face");
		}
		if (Period == 2)
		{
			result = GameObject.Create("The Shemesh Face");
		}
		if (Period == 3)
		{
			result = GameObject.Create("The Earth Face");
		}
		if (Period == 4)
		{
			result = GameObject.Create("The Levant Face");
		}
		if (Period == 5)
		{
			result = GameObject.Create("The Olive Face");
		}
		if (Period == 6)
		{
			result = GameObject.Create("The Nil Face");
		}
		return result;
	}

	public List<GameObject> generateRelics()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (string id in HistoryAPI.GetSultanForPeriod(Period).GetList("items"))
		{
			HistoricEntity historicEntity = XRLCore.Core.Game.sultanHistory.entities.Where((HistoricEntity e) => e.GetCurrentSnapshot().Name == id).FirstOrDefault();
			if (historicEntity != null)
			{
				list.Add(RelicGenerator.GenerateRelic(historicEntity.GetCurrentSnapshot()));
			}
		}
		return list;
	}
}
