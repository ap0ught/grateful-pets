using System;
using System.Collections.Generic;
using System.Xml;
using XRL.UI;

namespace XRL.World.Conversations;

[Serializable]
[HasModSensitiveStaticCache]
public class ConversationLoader
{
	private Action<object> LogHandler = MetricsManager.LogError;

	[PreGameCacheInit]
	public static void CheckInit()
	{
		if (Conversation._Blueprints == null)
		{
			Loading.LoadTask("Loading Conversations.xml", LoadConversations);
		}
	}

	private static void ReadConversation(string Path, BuildContext Context)
	{
		string text = null;
		using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(Path);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.Name == "conversations")
			{
				text = (Context.Namespace = xmlTextReader.GetAttribute("Namespace"));
				if (MarkovCorpusGenerator.Generating && xmlTextReader.GetAttribute("ExcludeFromCorpusGeneration").EqualsNoCase("true"))
				{
					break;
				}
			}
			else if (xmlTextReader.Name == "conversation")
			{
				string text2 = xmlTextReader.GetAttribute("Namespace");
				if (!text2.IsNullOrEmpty() && !text.IsNullOrEmpty())
				{
					text2 = text + "." + text2;
				}
				Context.Namespace = text2 ?? text;
				ConversationXMLBlueprint conversationXMLBlueprint = new ConversationXMLBlueprint
				{
					Inherits = "BaseConversation"
				};
				conversationXMLBlueprint.Read(xmlTextReader, Context);
				if (Conversation._Blueprints.TryGetValue(conversationXMLBlueprint.ID, out var value) && conversationXMLBlueprint.Load == 0)
				{
					value.Merge(conversationXMLBlueprint);
				}
				else
				{
					Conversation._Blueprints[conversationXMLBlueprint.ID] = conversationXMLBlueprint;
				}
			}
		}
		xmlTextReader.Close();
	}

	private static void LoadConversations()
	{
		Dictionary<string, ConversationXMLBlueprint> dictionary = (Conversation._Blueprints = new Dictionary<string, ConversationXMLBlueprint>());
		BuildContext buildContext = new BuildContext();
		foreach (DataFile item in DataManager.GetXMLFilesWithRoot("Conversations"))
		{
			DataFile dataFile = (buildContext.File = item);
			try
			{
				ReadConversation(dataFile, buildContext);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(dataFile.Mod, message);
			}
		}
		buildContext.Next.AddRange(dictionary.Values);
		int num = 20;
		while (num > 0 && buildContext.Next.Count > 0)
		{
			int count = buildContext.Next.Count;
			buildContext.Advance();
			foreach (ConversationXMLBlueprint item2 in buildContext.Current)
			{
				if (!item2.Bake(buildContext))
				{
					buildContext.Next.Add(item2);
				}
			}
			if (count == buildContext.Next.Count)
			{
				num--;
			}
		}
		foreach (string error in buildContext.Errors)
		{
			MetricsManager.LogError(error);
		}
		buildContext.Clear();
		foreach (ConversationXMLBlueprint value in dictionary.Values)
		{
			value.DistributeChildren(buildContext);
		}
	}
}
