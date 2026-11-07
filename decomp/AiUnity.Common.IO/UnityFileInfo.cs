using System.IO;
using AiUnity.Common.Patterns;
using UnityEngine;

namespace AiUnity.Common.IO;

public abstract class UnityFileInfo<T> : Singleton<T> where T : new()
{
	public FileInfo FileInfo { get; set; }

	public string NameWithoutExtension => Path.GetFileNameWithoutExtension(FileInfo.Name);

	public string RelativeName
	{
		get
		{
			if (!string.IsNullOrEmpty(FileInfo.FullName))
			{
				return FileInfo.FullName.Substring(Application.dataPath.Length - 6);
			}
			return null;
		}
	}

	public string RelativeNameWithoutExtension => Path.Combine(Path.GetDirectoryName(RelativeName), NameWithoutExtension);
}
