using System;
using System.Collections.Generic;

namespace XRL.World.Anatomy;

[Serializable]
public class AnatomyPart
{
	public BodyPartType Type;

	public string SupportsDependent;

	public string DependsOn;

	public string RequiresType;

	public string DefaultBehavior;

	public int? Category;

	public int? Laterality;

	public int? RequiresLaterality;

	public int? Mobility;

	public bool? Integral;

	public bool? Mortal;

	public bool? Abstract;

	public bool? Extrinsic;

	public bool? Plural;

	public bool? Mass;

	public bool? Contact;

	public bool? IgnorePosition;

	public List<AnatomyPart> Subparts = new List<AnatomyPart>(0);

	public AnatomyPart(BodyPartType Type)
	{
		this.Type = Type;
	}

	public void ApplyTo(BodyPart parent)
	{
		if (parent == null)
		{
			MetricsManager.LogError("called with null parent, type " + Type);
			return;
		}
		BodyPartType type = Type;
		string supportsDependent = SupportsDependent;
		string dependsOn = DependsOn;
		string requiresType = RequiresType;
		string defaultBehavior = DefaultBehavior;
		int? category = Category;
		int valueOrDefault = Laterality.GetValueOrDefault();
		int? requiresLaterality = RequiresLaterality;
		int? mobility = Mobility;
		bool? integral = Integral;
		bool? mortal = Mortal;
		bool? flag = Abstract;
		bool? extrinsic = Extrinsic;
		bool? plural = Plural;
		bool? mass = Mass;
		bool? contact = Contact;
		bool? ignorePosition = IgnorePosition;
		BodyPart parent2 = parent.AddPart(type, valueOrDefault, defaultBehavior, supportsDependent, dependsOn, requiresType, null, category, requiresLaterality, mobility, null, integral, mortal, flag, extrinsic, null, plural, mass, contact, ignorePosition);
		foreach (AnatomyPart subpart in Subparts)
		{
			subpart.ApplyTo(parent2);
		}
	}
}
