using System;
using System.Collections.Generic;
using UnityEngine;

public class exBitmapFont : ScriptableObject
{
	[Serializable]
	public class CharInfo
	{
		public int id = -1;

		public int trim_x = -1;

		public int trim_y = -1;

		public int x = -1;

		public int y = -1;

		public int width = -1;

		public int height = -1;

		public int xoffset = -1;

		public int yoffset = -1;

		public int xadvance = -1;

		public bool rotated;

		public int rotatedWidth
		{
			get
			{
				if (rotated)
				{
					return height;
				}
				return width;
			}
		}

		public int rotatedHeight
		{
			get
			{
				if (rotated)
				{
					return width;
				}
				return height;
			}
		}

		public CharInfo()
		{
		}

		public CharInfo(CharInfo _c)
		{
			id = _c.id;
			x = _c.x;
			y = _c.y;
			width = _c.width;
			height = _c.height;
			xoffset = _c.xoffset;
			yoffset = _c.yoffset;
			xadvance = _c.xadvance;
			rotated = _c.rotated;
		}
	}

	[Serializable]
	public class KerningInfo
	{
		public int first = -1;

		public int second = -1;

		public int amount = -1;
	}

	public struct KerningTableKey(char _first, char _second)
	{
		public class Comparer : IEqualityComparer<KerningTableKey>
		{
			private static Comparer instance_;

			public static Comparer instance
			{
				get
				{
					if (instance_ == null)
					{
						instance_ = new Comparer();
					}
					return instance_;
				}
			}

			public bool Equals(KerningTableKey _lhs, KerningTableKey _rhs)
			{
				if (_lhs.first == _rhs.first)
				{
					return _lhs.second == _rhs.second;
				}
				return false;
			}

			public int GetHashCode(KerningTableKey _obj)
			{
				return (int)(((uint)_obj.first << 16) ^ _obj.second);
			}
		}

		public char first = _first;

		public char second = _second;
	}

	public string rawFontGUID = "";

	public string rawTextureGUID = "";

	public string rawAtlasGUID = "";

	public Texture2D texture;

	public List<CharInfo> charInfos = new List<CharInfo>();

	public List<KerningInfo> kernings = new List<KerningInfo>();

	public int baseLine;

	public int lineHeight;

	public int size;

	protected Dictionary<int, CharInfo> charInfoTable;

	protected Dictionary<KerningTableKey, int> kerningTable;

	public void Reset()
	{
		rawFontGUID = "";
		texture = null;
		charInfos.Clear();
		kernings.Clear();
		baseLine = 0;
		lineHeight = 0;
		size = 0;
		charInfoTable = null;
		kerningTable = null;
	}

	public void RebuildCharInfoTable()
	{
		if (charInfoTable == null)
		{
			charInfoTable = new Dictionary<int, CharInfo>(charInfos.Count);
		}
		charInfoTable.Clear();
		for (int i = 0; i < charInfos.Count; i++)
		{
			CharInfo charInfo = charInfos[i];
			charInfoTable[charInfo.id] = charInfo;
		}
	}

	public CharInfo GetCharInfo(char _symbol)
	{
		if (charInfoTable == null || charInfoTable.Count == 0)
		{
			RebuildCharInfoTable();
		}
		if (charInfoTable.TryGetValue(_symbol, out var value))
		{
			return value;
		}
		return null;
	}

	public void RebuildKerningTable()
	{
		if (kerningTable == null)
		{
			kerningTable = new Dictionary<KerningTableKey, int>(kernings.Count, KerningTableKey.Comparer.instance);
		}
		kerningTable.Clear();
		for (int i = 0; i < kernings.Count; i++)
		{
			KerningInfo kerningInfo = kernings[i];
			kerningTable[new KerningTableKey((char)kerningInfo.first, (char)kerningInfo.second)] = kerningInfo.amount;
		}
	}

	public int GetKerning(char _first, char _second)
	{
		if (kerningTable == null)
		{
			RebuildKerningTable();
		}
		if (kerningTable.TryGetValue(new KerningTableKey(_first, _second), out var value))
		{
			return value;
		}
		return 0;
	}
}
