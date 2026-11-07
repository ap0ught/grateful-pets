using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.Anatomy;

[Serializable]
public class Anatomy
{
	public string Name;

	public List<AnatomyPart> Parts = new List<AnatomyPart>();

	public int? Category;

	public string BodyType = "Body";

	public int? BodyCategory;

	public int? BodyMobility;

	public string ThrownWeapon = "Thrown Weapon";

	public string FloatingNearby = "Floating Nearby";

	public Anatomy(string Name)
	{
		this.Name = Name;
	}

	public void ApplyTo(Body body)
	{
		body.built = false;
		body._Anatomy = Name;
		body.DismemberedParts = null;
		BodyPart bodyPart = (body._Body = new BodyPart(BodyType, body, Parts.Count));
		if (BodyCategory.HasValue)
		{
			bodyPart.Category = BodyCategory.Value;
		}
		if (BodyMobility.HasValue)
		{
			bodyPart.Mobility = BodyMobility.Value;
		}
		foreach (AnatomyPart part in Parts)
		{
			part.ApplyTo(bodyPart);
		}
		if (!ThrownWeapon.IsNullOrEmpty())
		{
			bodyPart.AddPart(ThrownWeapon);
		}
		if (!FloatingNearby.IsNullOrEmpty())
		{
			bodyPart.AddPart(FloatingNearby);
		}
		if (Category.HasValue)
		{
			bodyPart.CategorizeAll(Category.Value);
		}
		body.MarkAllNative();
		body.built = true;
		body.UpdateBodyParts();
	}

	public bool Contains(BodyPartType type)
	{
		foreach (AnatomyPart part in Parts)
		{
			if (part.Type == type)
			{
				return true;
			}
		}
		return false;
	}
}
