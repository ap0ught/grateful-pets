using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.UI;

public class HotkeySpread : IReadOnlyList<UnityEngine.KeyCode>, IEnumerable<UnityEngine.KeyCode>, IEnumerable, IReadOnlyCollection<UnityEngine.KeyCode>, IReadOnlyList<char>, IEnumerable<char>, IReadOnlyCollection<char>
{
	[Serializable]
	public struct CharEnumerator : IEnumerator<char>, IEnumerator, IDisposable
	{
		private List<UnityEngine.KeyCode> List;

		private int Index;

		private char Item;

		public char Current => Item;

		object IEnumerator.Current => Item;

		public CharEnumerator(List<UnityEngine.KeyCode> List)
		{
			this.List = List;
			Index = 0;
			Item = '\0';
		}

		public CharEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			if (Index >= List.Count)
			{
				Item = '\0';
				return false;
			}
			Item = Keyboard.ConvertKeycodeToLowercaseChar(List[Index++]);
			return true;
		}

		public void Dispose()
		{
			List = null;
		}

		void IEnumerator.Reset()
		{
			Index = 0;
			Item = '\0';
		}
	}

	private List<UnityEngine.KeyCode> keys = new List<UnityEngine.KeyCode>();

	private int pos;

	public int Count => keys.Count;

	public UnityEngine.KeyCode this[int Index] => keys[Index];

	char IReadOnlyList<char>.this[int Index] => Keyboard.ConvertKeycodeToLowercaseChar(keys[Index]);

	public HotkeySpread(List<UnityEngine.KeyCode> keys)
	{
		this.keys = keys;
	}

	public void restart()
	{
		pos = 0;
	}

	public void next()
	{
		pos++;
	}

	public void prev()
	{
		if (pos > 0)
		{
			pos--;
		}
	}

	public UnityEngine.KeyCode code()
	{
		return codeAt(pos);
	}

	public char ch()
	{
		return charAt(pos);
	}

	public UnityEngine.KeyCode codeAt(int n)
	{
		if (keys.Count <= n)
		{
			return UnityEngine.KeyCode.None;
		}
		if (n < 0)
		{
			return UnityEngine.KeyCode.None;
		}
		return keys[n];
	}

	public char charAt(int n)
	{
		if (keys.Count <= n)
		{
			return '\0';
		}
		if (n < 0)
		{
			return '\0';
		}
		return Keyboard.ConvertKeycodeToLowercaseChar(codeAt(n));
	}

	public static HotkeySpread get(string category)
	{
		return new HotkeySpread(ControlManager.GetHotkeySpread(CommandBindingManager.NavCategories[category].Layers));
	}

	public static HotkeySpread get()
	{
		return new HotkeySpread(ControlManager.GetHotkeySpread(ControlManager.EnabledLayers));
	}

	public static HotkeySpread get(IEnumerable<string> layers)
	{
		return new HotkeySpread(ControlManager.GetHotkeySpread(layers.ToList()));
	}

	public List<UnityEngine.KeyCode>.Enumerator GetEnumerator()
	{
		return keys.GetEnumerator();
	}

	public CharEnumerator GetCharEnumerator()
	{
		return new CharEnumerator(keys);
	}

	IEnumerator<UnityEngine.KeyCode> IEnumerable<UnityEngine.KeyCode>.GetEnumerator()
	{
		return keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return keys.GetEnumerator();
	}

	IEnumerator<char> IEnumerable<char>.GetEnumerator()
	{
		return new CharEnumerator(keys);
	}
}
