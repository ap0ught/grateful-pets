using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using XRL.UI;

namespace XRL.CharacterBuilds;

public static class CodeCompressor
{
	[Serializable]
	public class BuildCodePayload
	{
		public string gameversion;

		public string buildversion;

		public List<CodeEntry> modules;
	}

	[Serializable]
	public class CodeEntry
	{
		public Type moduleType;

		public string requiredMod;

		public AbstractEmbarkBuilderModuleData data;
	}

	public class TypeBinder : DefaultSerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
			try
			{
				return base.BindToType(assemblyName, typeName);
			}
			catch (Exception ex)
			{
				try
				{
					return base.BindToType(assemblyName, typeName.Replace("XRL.CharacterBuilds.", "XRL.CharacterBuilds.Qud."));
				}
				catch
				{
					MetricsManager.LogError(ex);
				}
				throw ex;
			}
		}
	}

	public static JsonSerializerSettings SERIALIZER_SETTINGS = new JsonSerializerSettings
	{
		TypeNameHandling = TypeNameHandling.Auto,
		TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
		MissingMemberHandling = MissingMemberHandling.Ignore,
		SerializationBinder = new TypeBinder()
	};

	public static string Compress(string s)
	{
		using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(s));
		using MemoryStream memoryStream2 = new MemoryStream();
		using (GZipStream destination = new GZipStream(memoryStream2, CompressionMode.Compress))
		{
			memoryStream.CopyTo(destination);
		}
		return Convert.ToBase64String(memoryStream2.ToArray());
	}

	public static string Decompress(string s)
	{
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(s));
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
		{
			gZipStream.CopyTo(memoryStream);
		}
		string text = Encoding.Unicode.GetString(memoryStream.ToArray(), 0, 5);
		if (text.Length == 0 || text[0] == '{')
		{
			return Encoding.Unicode.GetString(memoryStream.ToArray());
		}
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	public static string generateCode(IEnumerable<AbstractEmbarkBuilderModule> modules)
	{
		BuildCodePayload buildCodePayload = new BuildCodePayload();
		buildCodePayload.buildversion = "1.0.1";
		buildCodePayload.gameversion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		buildCodePayload.modules = new List<CodeEntry>();
		foreach (AbstractEmbarkBuilderModule module in modules)
		{
			if (module.IncludeInBuildCodes())
			{
				CodeEntry codeEntry = new CodeEntry();
				codeEntry.requiredMod = module.GetRequiredMod();
				codeEntry.moduleType = module.GetType();
				codeEntry.data = module.getData();
				buildCodePayload.modules.Add(codeEntry);
			}
		}
		return Compress(JsonConvert.SerializeObject(buildCodePayload, Formatting.Indented, SERIALIZER_SETTINGS));
	}

	public static void loadCode(string code, List<AbstractEmbarkBuilderModule> modules, bool silent = false)
	{
		try
		{
			JObject jObject = JObject.Parse(Decompress(code));
			BuildCodePayload buildCodePayload = new BuildCodePayload();
			buildCodePayload.buildversion = jObject.Value<string>("buildversion");
			buildCodePayload.gameversion = jObject.Value<string>("gameversion");
			buildCodePayload.modules = new List<CodeEntry>();
			foreach (JToken item in jObject.Value<JArray>("modules"))
			{
				try
				{
					CodeEntry payloadModule = JsonConvert.DeserializeObject<CodeEntry>(item.ToString(), SERIALIZER_SETTINGS);
					buildCodePayload.modules.Add(payloadModule);
					foreach (AbstractEmbarkBuilderModule item2 in modules.Where((AbstractEmbarkBuilderModule m) => m.GetType() == payloadModule.moduleType))
					{
						if (silent)
						{
							item2.setDataDirect(payloadModule.data);
						}
						else
						{
							item2.setData(payloadModule.data);
						}
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogError("Error deserializing build code", x);
					string text = item.Value<string>("requiredMod");
					if (!text.IsNullOrEmpty())
					{
						Popup.ShowAsync("Error decoding build code - Required Mod \"" + text + "\" not found.");
					}
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
