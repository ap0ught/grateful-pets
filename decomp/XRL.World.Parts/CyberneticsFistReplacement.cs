using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsFistReplacement : IPart
{
	public string FistObject;

	public List<int> ReplacedFists = new List<int>();

	public List<string> OriginalFistBlueprints = new List<string>();

	public string ImplantDependency;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		if (Reader.FileVersion <= 400)
		{
			ReplacedFists.Clear();
			OriginalFistBlueprints.Clear();
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID && ID != PooledEvent<RegenerateDefaultEquipmentEvent>.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ImplantDependency = E.Part.DependsOn;
		ApplyFists(E.Part.ParentBody);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		ImplantDependency = null;
		E.Part?.ParentBody?.UpdateBodyParts();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RegenerateDefaultEquipmentEvent E)
	{
		ApplyFists(E.Body);
		return base.HandleEvent(E);
	}

	public void ApplyFists(Body Body)
	{
		if (Body == null || ImplantDependency.IsNullOrEmpty())
		{
			return;
		}
		foreach (BodyPart item in Body.GetPart("Hand", EvenIfDismembered: true))
		{
			if (!item.Extrinsic && !(item.SupportsDependent != ImplantDependency))
			{
				if (!ReplacedFists.Contains(item.ID))
				{
					ReplacedFists.Add(item.ID);
					OriginalFistBlueprints.Add(item.DefaultBehavior?.Blueprint);
				}
				else if (item.DefaultBehavior?.Blueprint == FistObject)
				{
					continue;
				}
				item.DefaultBehavior?.Obliterate();
				item.DefaultBehavior = GameObject.Create(FistObject);
				item.DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "CyberneticsFistReplacement");
			}
		}
	}
}
