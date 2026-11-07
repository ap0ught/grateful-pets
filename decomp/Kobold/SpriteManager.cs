using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ConsoleLib.Console;
using UnityEngine;
using XRL;
using XRL.Collections;
using XRL.Core;

namespace Kobold;

[HasModSensitiveStaticCache]
public static class SpriteManager
{
	private static GameObject _BaseSpritePrefab;

	private static Shader[] Shaders;

	private static GameObject _BaseSplitSpritePrefab;

	[ModSensitiveStaticCache(false)]
	private static StringMap<exTextureInfo> InfoMap;

	private static Dictionary<exTextureInfo, Sprite> SpriteMap;

	private static StringMap<exTextureInfo> PathMap;

	private static StringMap<string> KoboldMap;

	private static HashSet<string> Warned;

	private static exTextureInfo InvalidInfo;

	private static Rack<char> KeyBuffer;

	private static ReaderWriterLockSlim InfoLock;

	private static readonly string KeyPrefix;

	private static readonly string InvalidTexturePath;

	public static exTextureInfo InvalidTexture => InvalidInfo;

	private static GameObject CloneSpritePrefab()
	{
		if (_BaseSpritePrefab == null)
		{
			_BaseSpritePrefab = Resources.Load("KoboldBaseSprite") as GameObject;
			UnityEngine.Object.DontDestroyOnLoad(_BaseSpritePrefab);
		}
		return UnityEngine.Object.Instantiate(_BaseSpritePrefab);
	}

	public static Shader GetShaderMode(int n)
	{
		if (Shaders == null)
		{
			Shaders = new Shader[2];
			Shaders[0] = Shader.Find("Kobold/Alpha Blended Dual Color");
			Shaders[1] = Shader.Find("Kobold/Alpha Blended Truecolor");
		}
		return Shaders[n];
	}

	private static GameObject CloneSplitSpritePrefab()
	{
		if (_BaseSplitSpritePrefab == null)
		{
			_BaseSplitSpritePrefab = Resources.Load("KoboldBaseSlicedSprite") as GameObject;
			UnityEngine.Object.DontDestroyOnLoad(_BaseSplitSpritePrefab);
		}
		return UnityEngine.Object.Instantiate(_BaseSplitSpritePrefab);
	}

	public static Sprite GetUnitySprite(string path)
	{
		return GetUnitySprite(GetTextureInfo(path));
	}

	public static Sprite GetUnitySprite(Texture2D texture)
	{
		return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
	}

	public static Sprite GetUnitySprite(exTextureInfo info)
	{
		if (SpriteMap.TryGetValue(info, out var value))
		{
			return value;
		}
		Texture2D texture = info.texture;
		Texture2D texture2D = new Texture2D(info.width, info.height, TextureFormat.ARGB32, mipChain: false);
		texture2D.filterMode = UnityEngine.FilterMode.Point;
		Color[] pixels = texture.GetPixels(info.x, info.y, info.width, info.height, 0);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 40f);
		SpriteMap.Add(info, value);
		return value;
	}

	public static void SetSprite(GameObject Sprite, string Path)
	{
		Sprite.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path);
	}

	public static Vector2i GetSpriteSize(string Path)
	{
		exTextureInfo textureInfo = GetTextureInfo(Path);
		Debug.Log(Path + " " + textureInfo.rawWidth + "x" + textureInfo.rawWidth + "   " + textureInfo.trim_x + "x" + textureInfo.trim_y);
		return new Vector2i(textureInfo.width, textureInfo.height);
	}

	static SpriteManager()
	{
		_BaseSpritePrefab = null;
		Shaders = null;
		_BaseSplitSpritePrefab = null;
		InfoMap = null;
		SpriteMap = null;
		PathMap = null;
		KoboldMap = null;
		Warned = new HashSet<string>();
		InfoLock = new ReaderWriterLockSlim();
		KeyPrefix = "assets/content/textures/";
		InvalidTexturePath = "Text_32.bmp";
		KeyBuffer = new Rack<char>(128);
	}

	public static void Initialize()
	{
		using (InfoLock.TakeWriteLock())
		{
			if (InfoMap == null)
			{
				InfoMap = new StringMap<exTextureInfo>();
				PathMap = new StringMap<exTextureInfo>();
				KoboldMap = new StringMap<string>();
				SpriteMap = new Dictionary<exTextureInfo, Sprite>();
				if (!GameManager.IsOnUIContext())
				{
					GameManager.Instance.uiQueue.awaitTask(Load);
				}
				else
				{
					Load();
				}
			}
		}
	}

	private static void Load()
	{
		MemoryHelper.GCCollect();
		KoboldDatabaseScriptable koboldDatabaseScriptable = Resources.Load<KoboldDatabaseScriptable>("KoboldDatabase");
		if (koboldDatabaseScriptable == null)
		{
			exTextureInfo[] array = Resources.LoadAll<exTextureInfo>("TextureInfo");
			InfoMap.EnsureCapacity(array.Length);
			PathMap.EnsureCapacity(array.Length);
			exTextureInfo[] array2 = array;
			foreach (exTextureInfo exTextureInfo2 in array2)
			{
				if (exTextureInfo2 != null)
				{
					try
					{
						InfoMap[GetKey(exTextureInfo2.name)] = exTextureInfo2;
					}
					catch (Exception ex)
					{
						Debug.Log("Error adding - " + exTextureInfo2.name + " ... " + ex.Message);
					}
				}
			}
		}
		else
		{
			InfoMap.EnsureCapacity(koboldDatabaseScriptable.koboldTextureInfos.Length);
			KoboldMap.EnsureCapacity(koboldDatabaseScriptable.koboldTextureInfos.Length);
			PathMap.EnsureCapacity(koboldDatabaseScriptable.koboldTextureInfos.Length);
			string[] koboldTextureInfos = koboldDatabaseScriptable.koboldTextureInfos;
			foreach (string text in koboldTextureInfos)
			{
				if (text.IsNullOrEmpty())
				{
					Debug.LogWarning("Info in koboldTextureInfos is null");
					continue;
				}
				try
				{
					string key = GetKey(text).ToString();
					InfoMap[key] = null;
					KoboldMap.TryAdd(key, text);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("SpriteManager", x);
				}
			}
		}
		foreach (ModInfo activeMod in ModManager.ActiveMods)
		{
			foreach (ModFile file in activeMod.Files)
			{
				if (file.Type != ModFileType.Sprite)
				{
					continue;
				}
				try
				{
					int num = file.RelativeName.LastSubdirectoryIndex("textures");
					if (num != -1)
					{
						ReadOnlySpan<char> key2 = GetKey(file.RelativeName.AsSpan(num + 9));
						Texture2D texture2D = new Texture2D(activeMod.TextureConfiguration.TextureWidth, activeMod.TextureConfiguration.TextureHeight);
						byte[] data = File.ReadAllBytes(file.OriginalName);
						texture2D.LoadImage(data);
						texture2D.filterMode = UnityEngine.FilterMode.Point;
						exTextureInfo exTextureInfo3 = ScriptableObject.CreateInstance<exTextureInfo>();
						exTextureInfo3.texture = texture2D;
						exTextureInfo3.width = texture2D.width;
						exTextureInfo3.height = texture2D.height;
						exTextureInfo3.x = 0;
						exTextureInfo3.y = 0;
						exTextureInfo3.ShaderMode = activeMod.TextureConfiguration.ShaderMode;
						InfoMap[key2] = exTextureInfo3;
					}
				}
				catch (Exception x2)
				{
					MetricsManager.LogException("Mod texture load", x2);
				}
			}
		}
		PopulateInvalidTexture();
	}

	private static void PopulateInvalidTexture()
	{
		exTextureInfo exTextureInfo2 = InvalidInfo ?? (InvalidInfo = ScriptableObject.CreateInstance<exTextureInfo>());
		ReadOnlySpan<char> key = GetKey(InvalidTexturePath);
		exTextureInfo exTextureInfo3 = InfoMap[key];
		if ((object)exTextureInfo3 == null)
		{
			exTextureInfo3 = (InfoMap[key] = LoadTextureInfo(key));
		}
		exTextureInfo2.name = "InvalidTexture";
		exTextureInfo2.texture = exTextureInfo3.texture;
		exTextureInfo2.width = exTextureInfo3.width;
		exTextureInfo2.height = exTextureInfo3.height;
		exTextureInfo2.rawWidth = exTextureInfo3.rawWidth;
		exTextureInfo2.rawHeight = exTextureInfo3.rawHeight;
		exTextureInfo2.x = exTextureInfo3.x;
		exTextureInfo2.y = exTextureInfo3.y;
		exTextureInfo2.ShaderMode = exTextureInfo3.ShaderMode;
	}

	public static exTextureInfo GetTextureInfo(string Path, bool returnSpaceOnInvalid = true)
	{
		if (TryGetTextureInfo(Path, out var Info))
		{
			return Info;
		}
		if (!returnSpaceOnInvalid)
		{
			return null;
		}
		lock (Warned)
		{
			if (Warned.Add(Path))
			{
				Debug.LogError("SpriteManager: No texture found by ID '" + Path + "'.");
			}
		}
		return InvalidInfo;
	}

	public static ReadOnlySpan<char> GetKey(ReadOnlySpan<char> Path)
	{
		int num = Path.Length;
		char[] array = KeyBuffer.GetArray(num);
		for (int i = 0; i < num; i++)
		{
			char c = Path[i];
			switch (c)
			{
			case '.':
				break;
			case '\\':
			case '_':
				array[i] = '/';
				continue;
			default:
				array[i] = c.ToLowerASCII();
				continue;
			}
			num = i;
			break;
		}
		ReadOnlySpan<char> readOnlySpan = new ReadOnlySpan<char>(array, 0, num);
		if (readOnlySpan.StartsWith(KeyPrefix, StringComparison.Ordinal))
		{
			return readOnlySpan.Slice(KeyPrefix.Length);
		}
		return readOnlySpan;
	}

	private static exTextureInfo LoadTextureInfo(ReadOnlySpan<char> Key)
	{
		if (KoboldMap.TryGetValue(Key, out var path))
		{
			if (GameManager.IsOnUIContext())
			{
				return Resources.Load<exTextureInfo>("TextureInfo/" + path);
			}
			exTextureInfo info = InvalidInfo;
			GameManager.Instance.uiQueue.awaitTask(() =>
			{
				info = Resources.Load<exTextureInfo>("TextureInfo/" + path);
			});
			return info;
		}
		return null;
	}

	public static bool TryGetTextureInfo(string Path, out exTextureInfo Info)
	{
		if (Path == null)
		{
			Info = null;
			return false;
		}
		if (InfoMap == null)
		{
			Initialize();
		}
		using (InfoLock.TakeReadLock())
		{
			if (PathMap.TryGetValue(Path, out Info))
			{
				return true;
			}
		}
		using (InfoLock.TakeWriteLock())
		{
			ReadOnlySpan<char> key = GetKey(Path);
			if (InfoMap.TryGetValue(key, out Info))
			{
				if ((object)Info == null)
				{
					InfoMap[key] = (Info = LoadTextureInfo(key));
				}
				PathMap.Add(Path, Info);
				return true;
			}
		}
		return false;
	}

	public static bool HasTextureInfo(string Path)
	{
		if (Path == null)
		{
			return false;
		}
		if (InfoMap == null)
		{
			Initialize();
		}
		using (InfoLock.TakeReadLock())
		{
			if (PathMap.ContainsKey(Path))
			{
				return true;
			}
		}
		using (InfoLock.TakeWriteLock())
		{
			return InfoMap.ContainsKey(GetKey(Path));
		}
	}

	public static GameObject CreateEmptySprite()
	{
		return CloneSpritePrefab();
	}

	public static GameObject CreateSplitSprite(string Path)
	{
		GameObject gameObject = CloneSplitSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().anchor = Anchor.MidCenter;
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}

	public static GameObject CreateCollidableSprite(string Path, Anchor _Anchor, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().anchor = _Anchor;
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		gameObject.GetComponent<ex3DSprite2>().bCollide = true;
		return gameObject;
	}

	public static GameObject CreateCollidableSprite(string Path, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		gameObject.GetComponent<ex3DSprite2>().bCollide = true;
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, Anchor _Anchor, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().anchor = _Anchor;
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, Color Foreground, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().color = Foreground;
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, Color Foreground, Color Background, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().color = Foreground;
		gameObject.GetComponent<ex3DSprite2>().backcolor = Background;
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}

	public static ex3DSprite2 GetPooledSprite(string Path, Color Foreground, Color Background, Color Detail, bool HFlip = false, bool VFlip = false)
	{
		ex3DSprite2 component = PooledPrefabManager.Instantiate("KoboldBaseSprite", null).GetComponent<ex3DSprite2>();
		component.textureInfo = GetTextureInfo(Path);
		component.shader = GetShaderMode(component.textureInfo.ShaderMode);
		component.color = Foreground;
		component.backcolor = Background;
		component.detailcolor = Detail;
		BoxCollider component2 = component.GetComponent<BoxCollider>();
		if (HFlip)
		{
			if (VFlip)
			{
				component.transform.localScale = new Vector3(-1f, -1f, -1f);
				component2.size = new Vector3(0f - Math.Abs(component2.size.x), 0f - Math.Abs(component2.size.y), 0f - Math.Abs(component2.size.z));
			}
			else
			{
				component.transform.localScale = new Vector3(-1f, 1f, 1f);
				component2.size = new Vector3(0f - Math.Abs(component2.size.x), Math.Abs(component2.size.y), Math.Abs(component2.size.z));
			}
		}
		else if (VFlip)
		{
			component.transform.localScale = new Vector3(1f, -1f, 1f);
			component2.size = new Vector3(Math.Abs(component2.size.x), 0f - Math.Abs(component2.size.y), Math.Abs(component2.size.z));
		}
		else
		{
			component.transform.localScale = new Vector3(1f, 1f, 1f);
			component2.size = new Vector3(Math.Abs(component2.size.x), Math.Abs(component2.size.y), Math.Abs(component2.size.z));
		}
		return component;
	}

	public static ex3DSprite2 GetPooledSprite(IRenderable Tile, bool Transparent = false)
	{
		ColorChars colorChars = Tile.getColorChars();
		string text = Tile.getTile();
		Color color = ConsoleLib.Console.ColorUtility.ColorMap.GetValue(colorChars.foreground).WithAlpha((!Transparent || colorChars.foreground != 'k') ? 1 : 0);
		Color color2 = ConsoleLib.Console.ColorUtility.ColorMap.GetValue(colorChars.background).WithAlpha((!Transparent || colorChars.background != 'k') ? 1 : 0);
		Color color3 = ConsoleLib.Console.ColorUtility.ColorMap.GetValue(colorChars.detail).WithAlpha((!Transparent || colorChars.detail != 'k') ? 1 : 0);
		if (Globals.RenderMode == RenderModeType.Text || text.IsNullOrEmpty())
		{
			int num = 32;
			string renderString = Tile.getRenderString();
			if (!renderString.IsNullOrEmpty())
			{
				num = renderString[0];
				if (num < 0 || num > 255)
				{
					num = 32;
				}
			}
			text = $"assets_content_textures_text_{num}.bmp";
			color3 = color;
			color = color2;
			color2 = color3;
		}
		return GetPooledSprite(text, color, color2, color3, Tile.getHFlip(), Tile.getVFlip());
	}

	public static void Return(ex3DSprite2 Sprite)
	{
		PooledPrefabManager.Return(Sprite.gameObject);
	}

	public static void Return(GameObject Object)
	{
		PooledPrefabManager.Return(Object);
	}
}
