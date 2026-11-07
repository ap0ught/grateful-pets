using System;
using XRL.World.Parts;

namespace XRL.World.Anatomy;

public static class BodyPartCategory
{
	public const int ANIMAL = 1;

	public const int ARTHROPOD = 2;

	public const int PLANT = 3;

	public const int FUNGAL = 4;

	public const int PROTOPLASMIC = 5;

	public const int CYBERNETIC = 6;

	public const int MECHANICAL = 7;

	public const int METAL = 8;

	public const int WOODEN = 9;

	public const int STONE = 10;

	public const int GLASS = 11;

	public const int LEATHER = 12;

	public const int BONE = 13;

	public const int CHITIN = 14;

	public const int PLASTIC = 15;

	public const int CLOTH = 16;

	public const int PSIONIC = 17;

	public const int EXTRADIMENSIONAL = 18;

	public const int MOLLUSK = 19;

	public const int JELLY = 20;

	public const int CRYSTAL = 21;

	public const int LIGHT = 22;

	public const int LIQUID = 23;

	public static readonly int[] ANIMATE = new int[7] { 1, 2, 3, 4, 5, 19, 20 };

	public static readonly int[] INANIMATE = new int[19]
	{
		6, 7, 3, 4, 5, 8, 9, 10, 11, 12,
		13, 14, 15, 16, 17, 18, 21, 22, 23
	};

	public static readonly int[] MACHINE = new int[2] { 6, 7 };

	public static string GetColor(int Category)
	{
		if (Category == 6)
		{
			return "C";
		}
		return null;
	}

	public static string GetName(int Category)
	{
		return Category switch
		{
			1 => "Animal", 
			2 => "Arthropod", 
			3 => "Plant", 
			4 => "Fungal", 
			5 => "Protoplasmic", 
			6 => "Cybernetic", 
			7 => "Mechanical", 
			8 => "Metal", 
			19 => "Mollusk", 
			9 => "Wooden", 
			10 => "Stone", 
			11 => "Glass", 
			12 => "Leather", 
			13 => "Bone", 
			14 => "Chitin", 
			15 => "Plastic", 
			16 => "Cloth", 
			17 => "Psionic", 
			18 => "Extradimensional", 
			20 => "Jelly", 
			21 => "Crystal", 
			22 => "Light", 
			23 => "Liquid", 
			_ => throw new Exception("invalid category " + Category), 
		};
	}

	public static int GetCode(string Name, bool ThrowExceptions = true)
	{
		switch (Name)
		{
		case "Animal":
			return 1;
		case "Arthropod":
			return 2;
		case "Plant":
			return 3;
		case "Fungal":
			return 4;
		case "Protoplasmic":
			return 5;
		case "Cybernetic":
			return 6;
		case "Mechanical":
			return 7;
		case "Metal":
			return 8;
		case "Mollusk":
			return 19;
		case "Wooden":
			return 9;
		case "Stone":
			return 10;
		case "Glass":
			return 11;
		case "Leather":
			return 12;
		case "Bone":
			return 13;
		case "Chitin":
			return 14;
		case "Plastic":
			return 15;
		case "Cloth":
			return 16;
		case "Psionic":
			return 17;
		case "Extradimensional":
			return 18;
		case "Jelly":
			return 20;
		case "Crystal":
			return 21;
		case "Light":
			return 22;
		case "Liquid":
			return 23;
		default:
			if (ThrowExceptions)
			{
				throw new Exception("invalid category name '" + Name + "'");
			}
			return 0;
		}
	}

	public static int GetCodeIfExists(string Name)
	{
		return GetCode(Name, ThrowExceptions: false);
	}

	public static int BestGuessForCategoryDerivedFromGameObject(GameObject obj)
	{
		string propertyOrTag = obj.GetPropertyOrTag("DerivedBodyPartCategory");
		if (propertyOrTag != null)
		{
			return GetCode(propertyOrTag);
		}
		if (obj.HasPart<ModExtradimensional>() || obj.HasPart<Extradimensional>())
		{
			return 18;
		}
		if (obj.HasTagOrProperty("LivePlant"))
		{
			return 3;
		}
		if (obj.HasTagOrProperty("LiveFungus"))
		{
			return 4;
		}
		if (obj.HasTagOrProperty("LiveAnimal"))
		{
			return 1;
		}
		if (obj.Render.DisplayName.Contains("skull"))
		{
			return 13;
		}
		if (obj.Render.DisplayName.Contains("shell"))
		{
			return 14;
		}
		if (obj.HasPart<EnergyCellSocket>())
		{
			return 7;
		}
		if (obj.HasPart<LiquidFueledPowerPlant>())
		{
			return 7;
		}
		if (obj.HasPart<Metal>())
		{
			return 8;
		}
		return 12;
	}

	public static bool IsLiveCategory(int Category)
	{
		if ((uint)(Category - 1) <= 4u || (uint)(Category - 18) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static int GetElectricalConductivity(int Category)
	{
		return Category switch
		{
			1 => 80, 
			2 => 70, 
			3 => 90, 
			4 => 60, 
			5 => 90, 
			6 => 70, 
			7 => 90, 
			8 => 100, 
			19 => 80, 
			9 => 20, 
			10 => 10, 
			11 => 0, 
			12 => 0, 
			13 => 0, 
			14 => 10, 
			15 => 0, 
			16 => 0, 
			17 => 30, 
			18 => 80, 
			20 => 90, 
			21 => 10, 
			22 => 0, 
			23 => 100, 
			_ => throw new Exception("invalid category " + Category), 
		};
	}

	public static bool ProcessList(string Spec, out int? Category, out int[] Categories, bool ThrowExceptions = true)
	{
		Category = null;
		Categories = null;
		if (!Spec.IsNullOrEmpty())
		{
			switch (Spec)
			{
			case "Animate":
				Categories = ANIMATE;
				break;
			case "Inanimate":
				Categories = INANIMATE;
				break;
			case "Machine":
				Categories = MACHINE;
				break;
			default:
				if (Spec.Contains(","))
				{
					string[] array = Spec.Split(',');
					int[] array2 = new int[array.Length];
					int i = 0;
					for (int num = array.Length; i < num; i++)
					{
						int code = GetCode(array[i], ThrowExceptions);
						if (code <= 0)
						{
							return false;
						}
						array2[i] = code;
					}
					Categories = array2;
				}
				else
				{
					int code2 = GetCode(Spec, ThrowExceptions);
					if (code2 <= 0)
					{
						return false;
					}
					Category = code2;
				}
				break;
			}
		}
		return true;
	}
}
