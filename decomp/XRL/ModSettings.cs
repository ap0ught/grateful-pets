using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace XRL;

public class ModSettings
{
	/// <summary>A copy of the mod's display title, for easier reading of ModSettings.json by humans.</summary>
	public string Title;

	/// <value><c>false</c> if the mod has been explicitly disabled by the user; otherwise, <c>true</c>.</value>
	public bool Enabled = true;

	/// <summary>The last approved hash of all file records within the mod.</summary>
	public string FilesHash;

	/// <summary>The last approved hash of all script content within the mod.</summary>
	public string SourceHash;

	/// <summary>The last version the user was prompted with, will not prompt again until a new version is available.</summary>
	public Version? UpdateVersion;

	/// <value><c>true</c> if the mod has failed to compile; otherwise, <c>false</c>.</value>
	[JsonIgnore]
	public bool Failed;

	/// <summary>A list of errors attributed to this mod.</summary>
	[JsonIgnore]
	public List<string> Errors = new List<string>();

	/// <summary>A list of warnings attributed to this mod.</summary>
	[JsonIgnore]
	public List<string> Warnings = new List<string>();

	/// <summary>
	/// Compute the hash value for the specified file records' name and size.
	/// </summary>
	/// <param name="Root">
	/// The root mod directory to hash file paths relative to.
	/// I.e. same hash result irrespective of what the mod folder's system location is.
	/// </param>
	/// <returns>A hex string of the computed hash code.</returns>
	public string CalcFilesHash(IReadOnlyList<ModFile> Files, string Root)
	{
		using SHA1 sHA = SHA1.Create();
		int i = 0;
		for (int count = Files.Count; i < count; i++)
		{
			ModFile modFile = Files[i];
			if (IsHashFile(modFile))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(modFile.FullName.Replace(Root, ""));
				sHA.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
				bytes = BitConverter.GetBytes(modFile.Size);
				sHA.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
			}
		}
		sHA.TransformFinalBlock(new byte[0], 0, 0);
		return string.Concat(sHA.Hash.Select((byte x) => x.ToString("X2")));
	}

	public bool IsHashFile(ModFile File)
	{
		if (File.Type != ModFileType.CSharp)
		{
			return File.Type == ModFileType.XML;
		}
		return true;
	}

	/// <summary>
	/// Compute the hash value for the specified files' contents.
	/// </summary>
	/// <returns>A hex string of the computed hash code.</returns>
	public string CalcSourceHash(IReadOnlyList<ModFile> Files)
	{
		using SHA1 sHA = SHA1.Create();
		byte[] array = new byte[8192];
		int num = 0;
		int i = 0;
		for (int count = Files.Count; i < count; i++)
		{
			ModFile modFile = Files[i];
			if (!IsHashFile(modFile))
			{
				continue;
			}
			using FileStream fileStream = new FileStream(modFile.OriginalName, FileMode.Open);
			do
			{
				num = fileStream.Read(array, 0, 8192);
				sHA.TransformBlock(array, 0, num, array, 0);
			}
			while (num > 0);
		}
		sHA.TransformFinalBlock(array, 0, 0);
		return string.Concat(sHA.Hash.Select((byte x) => x.ToString("X2")));
	}
}
