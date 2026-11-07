namespace XRL.World.Anatomy;

public class BodyPartType
{
	public string BasedOn;

	public string FinalType;

	public string Type;

	public string Description;

	public string DescriptionPrefix;

	public string Name;

	public string DefaultBehavior;

	public string UsuallyOn;

	public string UsuallyOnVariant;

	public string ImpliedBy;

	public string RequiresType;

	public string LimbBlueprintProperty = "SeveredLimbBlueprint";

	public string LimbBlueprintDefault = "GenericLimb";

	public string EquipSound;

	public string UnequipSound;

	public int? Category;

	public int? Laterality;

	public int? ImpliedPer;

	public int? RequiresLaterality;

	public int? Mobility;

	public int? ChimeraWeight;

	public bool? Appendage;

	public bool? Integral;

	public bool? Abstract;

	public bool? Mortal;

	public bool? Extrinsic;

	public bool? Plural;

	public bool? Mass;

	public bool? Contact;

	public bool? IgnorePosition;

	public bool? NoArmorAveraging;

	public int[] Branching;

	public BodyPartType(string Type, string Description = null, string Name = null)
	{
		this.Type = (FinalType = Type);
		this.Description = Description ?? this.Type;
		this.Name = Name ?? this.Type.ToLower();
	}

	public BodyPartType(BodyPartType Base)
	{
		Type = Base.Type;
		FinalType = Base.FinalType;
		Description = Base.Description;
		DescriptionPrefix = Base.DescriptionPrefix;
		Name = Base.Name;
		LimbBlueprintProperty = Base.LimbBlueprintProperty;
		LimbBlueprintDefault = Base.LimbBlueprintDefault;
		EquipSound = Base.EquipSound;
		UnequipSound = Base.UnequipSound;
		DefaultBehavior = Base.DefaultBehavior;
		UsuallyOn = Base.UsuallyOn;
		UsuallyOnVariant = Base.UsuallyOnVariant;
		ImpliedBy = Base.ImpliedBy;
		RequiresType = Base.RequiresType;
		Category = Base.Category;
		Laterality = Base.Laterality;
		ImpliedPer = Base.ImpliedPer;
		RequiresLaterality = Base.RequiresLaterality;
		Mobility = Base.Mobility;
		ChimeraWeight = Base.ChimeraWeight;
		Appendage = Base.Appendage;
		Integral = Base.Integral;
		Mortal = Base.Mortal;
		Abstract = Base.Abstract;
		Extrinsic = Base.Extrinsic;
		Plural = Base.Plural;
		Mass = Base.Mass;
		Contact = Base.Contact;
		IgnorePosition = Base.IgnorePosition;
		NoArmorAveraging = Base.NoArmorAveraging;
		Branching = Base.Branching;
	}

	public BodyPartType(BodyPartType Base, string Type)
		: this(Base)
	{
		this.Type = Type;
	}

	public void ApplyTo(BodyPart Part)
	{
		Part.DefaultBehaviorBlueprint = DefaultBehavior;
		Part.Type = FinalType;
		Part.VariantType = Type;
		Part.Description = Description;
		Part.DescriptionPrefix = DescriptionPrefix;
		Part.Name = Name;
		if (RequiresType != null)
		{
			Part.RequiresType = RequiresType;
		}
		if (Category.HasValue)
		{
			Part.Category = Category.Value;
		}
		if (Laterality.HasValue)
		{
			Part.Laterality = Laterality.Value;
		}
		if (RequiresLaterality.HasValue)
		{
			Part.RequiresLaterality = RequiresLaterality.Value;
		}
		if (Mobility.HasValue)
		{
			Part.Mobility = Mobility.Value;
		}
		if (Appendage.HasValue)
		{
			Part.Appendage = Appendage.Value;
		}
		if (Integral.HasValue)
		{
			Part.Integral = Integral.Value;
		}
		if (Abstract.HasValue)
		{
			Part.Abstract = Abstract.Value;
		}
		if (Mortal.HasValue)
		{
			Part.Mortal = Mortal.Value;
		}
		if (Extrinsic.HasValue)
		{
			Part.Extrinsic = Extrinsic.Value;
		}
		if (Plural.HasValue)
		{
			Part.Plural = Plural.Value;
		}
		if (Mass.HasValue)
		{
			Part.Mass = Mass.Value;
		}
		if (Contact.HasValue)
		{
			Part.Contact = Contact.Value;
		}
		if (IgnorePosition.HasValue)
		{
			Part.IgnorePosition = IgnorePosition.Value;
		}
	}

	public int RequiresLateralityFor(int Laterality)
	{
		if (RequiresLaterality.HasValue)
		{
			if (RequiresLaterality != 65535)
			{
				return RequiresLaterality.Value | Laterality;
			}
			return Laterality;
		}
		if (Laterality != 0)
		{
			return Laterality;
		}
		return 65535;
	}

	public int GetImpliedPer()
	{
		return ImpliedPer ?? 2;
	}
}
