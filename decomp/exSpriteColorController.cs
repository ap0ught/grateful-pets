using System;
using System.Collections.Generic;
using UnityEngine;

public class exSpriteColorController : MonoBehaviour
{
	[Serializable]
	public class ColorInfo
	{
		public exSpriteBase sprite;

		public Color color;
	}

	[SerializeField]
	protected Color color_ = Color.white;

	public List<ColorInfo> colorInfos = new List<ColorInfo>();

	public Color color
	{
		get
		{
			return color_;
		}
		set
		{
			if (color_ != value)
			{
				color_ = value;
				for (int i = 0; i < colorInfos.Count; i++)
				{
					ColorInfo colorInfo = colorInfos[i];
					colorInfo.sprite.color = colorInfo.color * color_;
				}
			}
		}
	}

	private void Awake()
	{
		enabled = false;
	}

	public void EnableSprites(bool _enabled)
	{
		for (int i = 0; i < colorInfos.Count; i++)
		{
			colorInfos[i].sprite.enabled = _enabled;
		}
	}

	public void RegisterColor(GameObject _go)
	{
		exSpriteBase[] componentsInChildren = _go.GetComponentsInChildren<exSpriteBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RegisterColor(componentsInChildren[i]);
		}
	}

	public void RegisterColor(exSpriteBase _sprite)
	{
		bool flag = false;
		for (int i = 0; i < colorInfos.Count; i++)
		{
			ColorInfo colorInfo = colorInfos[i];
			if (colorInfo.sprite == _sprite)
			{
				colorInfo.color = _sprite.color;
				flag = true;
			}
		}
		if (!flag)
		{
			ColorInfo colorInfo2 = new ColorInfo();
			colorInfo2.sprite = _sprite;
			colorInfo2.color = _sprite.color;
			colorInfos.Add(colorInfo2);
		}
	}

	public void UnregisterColor(GameObject _go)
	{
		exSpriteBase[] componentsInChildren = _go.GetComponentsInChildren<exSpriteBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnregisterColor(componentsInChildren[i]);
		}
	}

	public void UnregisterColor(exSpriteBase _sprite)
	{
		for (int i = 0; i < colorInfos.Count; i++)
		{
			if (colorInfos[i].sprite == _sprite)
			{
				colorInfos.RemoveAt(i);
				break;
			}
		}
	}

	public void RegisterAllChildren()
	{
		colorInfos.Clear();
		exSpriteBase[] componentsInChildren = GetComponentsInChildren<exSpriteBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ColorInfo colorInfo = new ColorInfo();
			colorInfo.color = (colorInfo.sprite = componentsInChildren[i]).color;
			colorInfos.Add(colorInfo);
		}
	}

	public void RemoveNullSprites()
	{
		for (int num = colorInfos.Count - 1; num >= 0; num--)
		{
			if (colorInfos[num].sprite == null)
			{
				colorInfos.RemoveAt(num);
			}
		}
	}
}
