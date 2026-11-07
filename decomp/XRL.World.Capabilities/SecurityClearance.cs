using System.Collections.Generic;

namespace XRL.World.Capabilities;

public static class SecurityClearance
{
	public static string GetKeySpecificationBySecurityClearance(int Clearance)
	{
		return Clearance switch
		{
			5 => "*Psychometry,Purple Security Card", 
			4 => "*Psychometry,Purple Security Card,Blue Security Card", 
			3 => "*Psychometry,Purple Security Card,Blue Security Card,Green Security Card", 
			2 => "*Psychometry,Purple Security Card,Blue Security Card,Green Security Card,Yellow Security Card", 
			1 => "*Psychometry,Purple Security Card,Blue Security Card,Green Security Card,Yellow Security Card,Red Security Card", 
			-5 => "Purple Security Card", 
			-4 => "Purple Security Card,Blue Security Card", 
			-3 => "Purple Security Card,Blue Security Card,Green Security Card", 
			-2 => "Purple Security Card,Blue Security Card,Green Security Card,Yellow Security Card", 
			-1 => "Purple Security Card,Blue Security Card,Green Security Card,Yellow Security Card,Red Security Card", 
			_ => null, 
		};
	}

	public static void HandleSecurityClearanceSpecification(int Spec, ref string KeyObject)
	{
		string keySpecificationBySecurityClearance = GetKeySpecificationBySecurityClearance(Spec);
		if (keySpecificationBySecurityClearance != null)
		{
			KeyObject = keySpecificationBySecurityClearance;
		}
	}

	public static int GetSecurityClearanceByKeySpecification(string KeySpec)
	{
		List<string> list = KeySpec.CachedCommaExpansion();
		if (list.Contains("Red Security Card"))
		{
			return 1;
		}
		if (list.Contains("Yellow Security Card"))
		{
			return 2;
		}
		if (list.Contains("Green Security Card"))
		{
			return 3;
		}
		if (list.Contains("Blue Security Card"))
		{
			return 4;
		}
		if (list.Contains("Purple Security Card"))
		{
			return 5;
		}
		return 0;
	}
}
