using System.IO;
using System.Linq;
using AiUnity.Common.IO;
using UnityEngine;

namespace AiUnity.NLog.Core.Common;

public class NLogConfigFile : UnityFileInfo<NLogConfigFile>
{
	public NLogConfigFile()
	{
		string text = PlayerPrefs.GetString("AiUnityNLogConfigFullFileName");
		if (string.IsNullOrEmpty(text))
		{
			if (Application.isEditor)
			{
				string text2 = (from s in Directory.GetFiles(Application.dataPath, "NLogger.cs", SearchOption.AllDirectories)
					select s.Replace('\\', '/')).FirstOrDefault((string s) => s.Contains("/NLog/Core/"));
				string text3 = (string.IsNullOrEmpty(text2) ? Application.dataPath : text2.Substring(0, text2.IndexOf("/NLog/Core/"))) + "/UserData/NLog/Resources";
				Directory.CreateDirectory(text3);
				text = text3 + "/NLog.xml";
			}
			else
			{
				text = Application.dataPath + "/NLog.xml";
			}
		}
		FileInfo = new FileInfo(text);
	}

	public void SetConfigFileName(string configFullFileName)
	{
		PlayerPrefs.SetString("AiUnityNLogConfigFullFileName", configFullFileName);
		FileInfo = new FileInfo(configFullFileName);
	}

	public string GetConfigText()
	{
		TextAsset textAsset = Resources.Load<TextAsset>(NameWithoutExtension);
		if (textAsset != null)
		{
			return textAsset.text;
		}
		if (FileInfo.Exists)
		{
			return File.ReadAllText(FileInfo.FullName);
		}
		return null;
	}
}
