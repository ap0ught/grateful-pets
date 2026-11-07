using Platform.IO;

namespace XRL;

public class ModFile
{
	public ModInfo Mod;

	public string Name;

	public string FullName;

	public string RelativeName;

	public string OriginalName;

	public string Extension;

	public long Size;

	public ModFileType Type;

	public ModFile(ModInfo Mod, FileInfo File)
	{
		this.Mod = Mod;
		OriginalName = File.FullName;
		RelativeName = Path.GetRelativePath(Mod.Path, OriginalName).ToLowerInvariant();
		FullName = OriginalName.ToLowerInvariant();
		Name = Path.GetFileName(FullName);
		Extension = Path.GetExtension(FullName);
		Size = File.Length;
		Type = Extension switch
		{
			".xml" => ModFileType.XML, 
			".json" => ModFileType.JSON, 
			".cs" => ModFileType.CSharp, 
			".png" => ModFileType.Sprite, 
			".wav" => ModFileType.Audio, 
			".ogg" => ModFileType.Audio, 
			".aiff" => ModFileType.Audio, 
			".mp3" => ModFileType.Audio, 
			".dll" => ModFileType.Assembly, 
			".rpm" => ModFileType.Map, 
			_ => ModFileType.Unknown, 
		};
	}
}
