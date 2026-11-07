using System;
using UnityEngine;

namespace RedShadow.CommonDialogs;

[Serializable]
public struct Place
{
	public Environment.SpecialFolder SpecialFolder;

	public string Name;

	public string Path;

	public Sprite Sprite;
}
