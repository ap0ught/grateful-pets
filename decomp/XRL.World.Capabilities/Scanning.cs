using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Capabilities;

public static class Scanning
{
	public enum Scan
	{
		Bio,
		Tech,
		Structure,
		Unscannable
	}

	[NonSerialized]
	private static Dictionary<Scan, string> scanNames = new Dictionary<Scan, string>
	{
		{
			Scan.Bio,
			"bioscanning"
		},
		{
			Scan.Tech,
			"techscanning"
		},
		{
			Scan.Structure,
			"structural scanning"
		},
		{
			Scan.Unscannable,
			"unscannable"
		}
	};

	[NonSerialized]
	private static Dictionary<Scan, string> scanSubjectNames = new Dictionary<Scan, string>
	{
		{
			Scan.Bio,
			"biometrics"
		},
		{
			Scan.Tech,
			"telemetry"
		},
		{
			Scan.Structure,
			"structural scan"
		},
		{
			Scan.Unscannable,
			"unscannable"
		}
	};

	[NonSerialized]
	private static Dictionary<Scan, string> scanProperties = new Dictionary<Scan, string>
	{
		{
			Scan.Bio,
			"BioScannerEquipped"
		},
		{
			Scan.Tech,
			"TechScannerEquipped"
		},
		{
			Scan.Structure,
			"StructureScannerEquipped"
		},
		{
			Scan.Unscannable,
			null
		}
	};

	public static string GetScanDisplayName(Scan scan)
	{
		return scanNames[scan];
	}

	public static string GetScanSubjectName(Scan scan)
	{
		return scanSubjectNames[scan];
	}

	public static string GetScanPropertyName(Scan scan)
	{
		return scanProperties[scan];
	}

	public static Scan GetScanTypeFor(GameObject obj)
	{
		return GetScanTypeEvent.GetFor(obj);
	}

	public static bool HasScanningFor(GameObject who, Scan scan)
	{
		return who.GetIntProperty(GetScanPropertyName(scan)) > 0;
	}

	public static bool HasScanningFor(GameObject who, GameObject obj)
	{
		return HasScanningFor(who, GetScanTypeFor(obj));
	}

	public static int GetScanEpistemicStatus(GameObject who, GameObject obj)
	{
		if (!GameObject.Validate(ref who) || !GameObject.Validate(ref obj))
		{
			return 0;
		}
		if (Options.SifrahExamine)
		{
			return 0;
		}
		Scan scan = GetScanTypeFor(obj);
		if (scan == Scan.Structure)
		{
			scan = Scan.Tech;
		}
		if (HasScanningFor(who, scan))
		{
			return 2;
		}
		return 0;
	}
}
