using UnityEngine;

namespace XRL;

public static class ClipboardHelper
{
	public static bool bPutClipboard;

	public static bool bUpdateClipboard;

	public static string ClipboardData;

	public static string PutData;

	public static string clipBoard
	{
		get
		{
			return GUIUtility.systemCopyBuffer;
		}
		set
		{
			GUIUtility.systemCopyBuffer = value;
		}
	}

	public static string GetClipboardData()
	{
		bUpdateClipboard = true;
		while (bUpdateClipboard)
		{
		}
		return ClipboardData;
	}

	public static void SetClipboardData(string D)
	{
		if (!bPutClipboard)
		{
			bPutClipboard = true;
			PutData = D;
		}
	}

	public static void UpdateFromMainThread()
	{
		if (bPutClipboard)
		{
			clipBoard = PutData;
			bPutClipboard = false;
		}
		if (bUpdateClipboard)
		{
			ClipboardData = clipBoard;
			bUpdateClipboard = false;
		}
	}
}
