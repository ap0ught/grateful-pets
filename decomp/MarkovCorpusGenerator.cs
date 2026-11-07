using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.Names;
using XRL.UI;
using XRL.World;
using XRL.World.Conversations;
using XRL.World.Skills;

public class MarkovCorpusGenerator : MonoBehaviour
{
	public List<string> downloadURLs = new List<string>();

	public List<string> textFiles = new List<string>();

	public UnityEngine.GameObject rootObject;

	public static bool Generating;

	private void Start()
	{
	}

	private IEnumerator Generate_()
	{
		yield return new WaitForEndOfFrame();
		downloadURLs.Clear();
		textFiles.Clear();
		Regex replacer = new Regex("=[^\\s]*?[=:]");
		Regex spice = new Regex("<[^\\s]*?>");
		Popup.Suppress = true;
		MessageQueue.Suppress = true;
		MinEvent.SuppressThreadWarning = true;
		Generating = true;
		if (!string.IsNullOrEmpty(rootObject.transform.Find("Include/URLList").GetComponent<InputField>().text))
		{
			string[] array = rootObject.transform.Find("Include/URLList").GetComponent<InputField>().text.Split('\n');
			foreach (string text in array)
			{
				Debug.Log("Fetching: " + text);
				downloadURLs.Add(text);
			}
		}
		if (!string.IsNullOrEmpty(rootObject.transform.Find("Include/TextList").GetComponent<InputField>().text))
		{
			string[] array = rootObject.transform.Find("Include/TextList").GetComponent<InputField>().text.Split('\n');
			foreach (string text2 in array)
			{
				Debug.Log("File: " + text2);
				textFiles.Add(text2.Replace("\r", ""));
			}
		}
		StringBuilder CorpusBuilder = new StringBuilder();
		StringBuilder ObjectCorpusBuilder = new StringBuilder();
		HashSet<string> CorpusAdded = new HashSet<string>();
		if (GetCheck("URLToggle"))
		{
			for (int x = 0; x < downloadURLs.Count; x++)
			{
				ProgressUpdate("Downloading: " + downloadURLs[x]);
				yield return new WaitForEndOfFrame();
				using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(downloadURLs[x]))
				{
					unityWebRequest.SendWebRequest();
					if (!unityWebRequest.isNetworkError && !unityWebRequest.isHttpError)
					{
						CorpusBuilder.Append(unityWebRequest.downloadHandler.text);
					}
					Debug.Log(unityWebRequest.downloadHandler.text);
				}
				CorpusBuilder.Append("\n");
			}
		}
		ProgressUpdate("Hotloading game configuration...");
		yield return new WaitForEndOfFrame();
		XRLCore.Core.HotloadConfiguration(bGenerateCorpusData: true);
		GameObjectFactory factory = GameObjectFactory.Factory;
		if (GetCheck("BooksToggle"))
		{
			foreach (string bookid in BookUI.BookCorpus.Keys)
			{
				ProgressUpdate("Processing book: " + bookid);
				yield return new WaitForEndOfFrame();
				CorpusBuilder.Append(ConsoleLib.Console.ColorUtility.StripFormatting(BookUI.BookCorpus[bookid]));
				CorpusBuilder.Append("\n");
			}
		}
		if (GetCheck("ConversationsToggle"))
		{
			foreach (KeyValuePair<string, ConversationXMLBlueprint> pair in Conversation.Blueprints)
			{
				if (pair.Key.StartsWith("Base"))
				{
					continue;
				}
				ProgressUpdate("Processing conversation: " + pair.Key);
				yield return new WaitForEndOfFrame();
				try
				{
					GameObjectBlueprint gameObjectBlueprint = factory.BlueprintList.Find((GameObjectBlueprint gameObjectBlueprint3) => !gameObjectBlueprint3.IsBaseBlueprint() && gameObjectBlueprint3.GetPartParameter<string>("ConversationScript", "ConversationID") == pair.Key);
					ConversationUI.Speaker = ((gameObjectBlueprint != null) ? factory.CreateSampleObject(gameObjectBlueprint) : factory.CreateSampleObject("Arconaut"));
					ConversationUI.Listener = The.Player;
					if (!ConversationUI.Speaker.HasProperName)
					{
						ConversationUI.Speaker.DisplayName = NameMaker.MakeName(ConversationUI.Speaker);
					}
					Conversation conversation = (ConversationUI.CurrentConversation = new Conversation());
					conversation.Load(pair.Value);
					conversation.Awake();
					conversation.Enter();
					conversation.Entered();
					conversation.Prepare();
					foreach (IConversationElement element in conversation.Elements)
					{
						try
						{
							element.Awake();
							element.Enter();
							element.Entered();
							string text3 = element.Texts?.FirstOrDefault().Text;
							if (!text3.IsNullOrEmpty() && !text3.Contains("MARKOV") && !text3.Contains("GGRESULT") && CorpusAdded.Add(text3))
							{
								element.Prepare();
								string text4 = Regex.Replace(element.Text, "\\{\\{emote\\|[^\\0]*?\\}\\}", "");
								text4 = ConsoleLib.Console.ColorUtility.StripFormatting(text4).Trim();
								if (replacer.IsMatch(text4))
								{
									MetricsManager.LogWarning("MarkovCorpusGenerator::ConversationElement::" + conversation.ID + "::" + element.ID + ": Parsed text has leftover variables, skipping.");
								}
								else if (!string.IsNullOrWhiteSpace(text4))
								{
									CorpusBuilder.Append(text4).Append("\n\n");
									element.Leave();
									element.Left();
								}
							}
						}
						catch (Exception x2)
						{
							MetricsManager.LogException("MarkovCorpusGenerator::ConversationElement::" + conversation.ID + "::" + element.ID, x2);
						}
					}
					ConversationUI.Speaker.Pool();
					ConversationUI.Speaker = null;
					ConversationUI.Listener = null;
					ConversationUI.CurrentConversation = null;
				}
				catch (Exception x3)
				{
					MetricsManager.LogException("MarkovCorpusGenerator::Conversation", x3);
				}
			}
		}
		if (GetCheck("ManualToggle"))
		{
			for (int x = 0; x < XRLCore.Manual.Page.Count; x++)
			{
				ProgressUpdate("Processing manual page: " + x);
				yield return new WaitForEndOfFrame();
				CorpusBuilder.Append(ConsoleLib.Console.ColorUtility.StripFormatting(XRLCore.Manual.Page[x].GetData(StripBrackets: false)));
			}
		}
		if (GetCheck("QuestsToggle"))
		{
			foreach (string bookid in QuestLoader.Loader.QuestsByID.Keys)
			{
				ProgressUpdate("Processing quest: " + bookid);
				yield return new WaitForEndOfFrame();
				foreach (string key in QuestLoader.Loader.QuestsByID[bookid].StepsByID.Keys)
				{
					CorpusBuilder.Append(ConsoleLib.Console.ColorUtility.StripFormatting(QuestLoader.Loader.QuestsByID[bookid].StepsByID[key].Text));
					CorpusBuilder.Append("\n");
				}
			}
		}
		if (GetCheck("SkillsToggle"))
		{
			foreach (string bookid in SkillFactory.Factory.SkillList.Keys)
			{
				ProgressUpdate("Processing skill: " + bookid);
				yield return new WaitForEndOfFrame();
				CorpusBuilder.Append(ConsoleLib.Console.ColorUtility.StripFormatting(SkillFactory.Factory.SkillList[bookid].Description));
				CorpusBuilder.Append("\n");
			}
		}
		if (GetCheck("TextToggle"))
		{
			for (int num = 0; num < textFiles.Count; num++)
			{
				ProgressUpdate("Reading: " + textFiles[num]);
				CorpusBuilder.Append(Regex.Replace(File.ReadAllText(textFiles[num]), "[^\\u0000-\\u007F]", string.Empty));
			}
		}
		if (GetCheck("ObjectShortDescriptionsToggle"))
		{
			for (int x = 0; x < factory.BlueprintList.Count; x++)
			{
				if (x % 100 == 0)
				{
					ProgressUpdate("Processing gameobject: " + x);
					yield return new WaitForEndOfFrame();
				}
				GameObjectBlueprint gameObjectBlueprint2 = factory.BlueprintList[x];
				if ((gameObjectBlueprint2.Name.Contains("Figurine") && gameObjectBlueprint2.DescendsFrom("Random Figurine")) || (gameObjectBlueprint2.Name.Contains("Statue") && gameObjectBlueprint2.DescendsFrom("Random Statue")))
				{
					continue;
				}
				string partParameter = gameObjectBlueprint2.GetPartParameter<string>("Description", "Short");
				if (partParameter.IsNullOrEmpty() || !CorpusAdded.Add(partParameter))
				{
					continue;
				}
				try
				{
					XRL.World.GameObject gameObject = factory.CreateSampleObject(gameObjectBlueprint2);
					string text5 = GameText.VariableReplace(partParameter, gameObject);
					if (replacer.IsMatch(text5))
					{
						MetricsManager.LogWarning("MarkovCorpusGenerator::ShortDescription::" + gameObjectBlueprint2.Name + ": Parsed text has leftover variables, skipping.");
						continue;
					}
					ObjectCorpusBuilder.Append(ConsoleLib.Console.ColorUtility.StripFormatting(text5));
					ObjectCorpusBuilder.Append("\n");
					ObjectCorpusBuilder.Append("\n");
					gameObject.Pool();
				}
				catch (Exception x4)
				{
					MetricsManager.LogException("MarkovCorpusGenerator::ShortDescription::" + gameObjectBlueprint2.Name, x4);
				}
			}
		}
		if (GetCheck("SultanGospelsToggle"))
		{
			History sultanHistory = The.Game.sultanHistory;
			foreach (HistoricEntity entity in sultanHistory.entities)
			{
				foreach (HistoricEvent story in entity.events)
				{
					if (story.entityProperties != null && story.entityProperties.TryGetValue("type", out var value) && value == "sultan")
					{
						story.entityProperties.TryGetValue("name", out var value2);
						ProgressUpdate("Processing sultan: " + value2);
						yield return new WaitForEndOfFrame();
					}
					try
					{
						if (story.eventProperties != null && story.eventProperties.TryGetValue("gospel", out var value3))
						{
							if (spice.IsMatch(value3))
							{
								throw new FormatException("Parsed text has leftover variables.");
							}
							ObjectCorpusBuilder.Append(ConsoleLib.Console.ColorUtility.StripFormatting(value3));
							ObjectCorpusBuilder.Append("\n");
							ObjectCorpusBuilder.Append("\n");
						}
					}
					catch (Exception x5)
					{
						MetricsManager.LogException("MarkovCorpusGenerator::SultanGospel", x5);
					}
				}
			}
		}
		StripTextInBrackets(CorpusBuilder);
		StripSentenceQuotes(CorpusBuilder);
		StripTextInBrackets(ObjectCorpusBuilder);
		StripSentenceQuotes(ObjectCorpusBuilder);
		ProgressUpdate("Generating corpus...");
		yield return new WaitForEndOfFrame();
		if (GetCheck("DebugText"))
		{
			Debug.LogWarning("Using debug text instead");
			CorpusBuilder.Length = 0;
			CorpusBuilder.Append("I come from the west. My parents sold me into slavery as a babe, though the practice had been formally abolished in Perth years earlier. As a boy I cut my teeth on the reefs of the Shore of Songs, and lulled the Pale Sea to sleep with many a hymn, that she might return our sailors from the Black Stair unscathed. How the melodies whisked inside my skull even then. I was given no name at birth, but the sailors called me Catu. At five and ten years I ran away. On Ettinspine I roosted with an eremite who ushered me into manhood. I learned to hunt, and laugh, and dream. In time I came to know of the Free City and the ingress afforded my kind. I shoved off from my stony nook, and trekked to their high gate, and settled there, though I preferred to sup with the anglers and other low folk. In a sun-warmed pool under the rampart the Free Seer first noticed me. The slant of his neck from where he watched on the parapet presaged my movements, I still recall. I stood from my bath and shamefully padded to my discarded vest, but stopped. I saw him from that distance, his eyes tucked deep into his shorn dome, his frock as inert as a statue's, and I felt no more shame. Under his tutelage I rose to prominence in his court. I stood by his side as he treated with the tuyuldars of Odrum, as he plotted against the southern freeholds. When a traitorous canter slipped poison into his cup, I smashed it from his hands and sundered the quisling's mind. That night, in the hour of the Beetle Moon, he introduced me to the mirth of an assembled mind. Together with the other canters we Joined, and he named me Mirthful, and raised me unto his honor guard. I would be dispossessed of it in under a year. But I must pause now. The Cant loudens, and the Elder nears. I vow to conclude my story if I still draw breath, and if I am free. It is dusk. I sit on the bank of the River Svy, but no fire warms my bones. The risk of drawing moon-doting beasts is too great, and any aggregation of minds is sure to attract the attention of the Elder. Instead, I write by a flame kindled on my thumb. I left off at being named Mirthful in the Hall of the Free Seer. No honor is higher for a canter of Oth. For months I trained with my masked compatriots beneath the Hall, where our clay cisterns stored the freshwater that would later haunt my dreams. In the spring a letter arrived from the Angler King. He sought a treaty with the Free Seer and invited us to a neutral council on the doorstep of Ibunudr, arbitrated by the tuyuldar of Doria. After much preparation we set out, rowing our sketties beyond the Bowl of Iris and to the frosty shores of that far tundra. Over the course of the journey the Free Seer's disposition soured. He withdrew from the counsel of the Mirthful, even my own, and spent the better part of the wintry trek alone in his tent. At first I attributed his mood to the fragility of our diplomatic high ground, but I soon learned the true tack. We arrived at the camp on Ibunudr on the summer solstice. The twin banners of Athenreach and Doria thrashed in the wind, and the hymns of the Eustace-Sutta carried. I remember smiling at the throaty crooning of their priests. As we studied the campground from a hillock, as the Free Seer ambled up to my side, I saw the jewel of the Hanging Hills, Nisramet, the Angler King's wife, she who cemented the peace between her father's fiefdoms and the Angler King's freehold. I saw her, and I immediately knew my own treachery. The Free Seer's gaze was acknowledgement enough. I've arrived at Kyakukya. Kind Nuntu shares his hut with me, offers me smoked mushroom, and asks nothing in return. I write these words at his desk, and though he lusts for knowledge himself, he leaves me be to chronicle my past. The campaign back toward Oth was grim. I awaited my fate at the Free Seer's hands, though I anticipated being given the dignity of first returning to the Hall and my home. I was not afforded so much. One dismal night, under the Beetle Moon, I was accosted in my tent by my masked brothers. To my bewilderment, the Stithening had come already. How had the Free Seer acquired the cherrydotters? Had he bargained for them at Ibunudr, or had he possessed them all along? To this day I do not know. I remember little of the Stithening outside the beating of my own heart high in my ears. I passed out, and when I awoke next morning, the cavalcade had vanished. Only a moment did my recognition last before the chanting in my head took hold. I had heard it before, of course. All my life, in fact. It was the chanting that I weaved my melodies around on the shores of Perth as a small boy. Now, though, it was as if a single, distant canter had summoned a thousand of his kin, and they each summoned a thousand of their own. I wept, then I screamed, then I tore the hair from my head. The chant animated my bones more surely than my volition ever had. East and south, it urged. East and south. East and south. Toward Qud. My recollection of the voyage to the Great Salt Desert is fractured and fitful. I grew sicker by the day, but the chant grew louder, and so quickened the force that compelled me. Surely I would die once I reached the moistureless plains of Moghra'yi, presumably as had my predecessors before me. Finally, I did reach them, where a salt-churned rivulet dried under a golden arch. The arch seems a thing born of my dementia but that I recall it so vividly, and that I know it's the least of the secrets kept by Moghra'yi. I edge closer to the grove of fungi and its preposterous promise. Are the whispers true? Could it be all so simple? I can feel the moistness of the Eater's flesh on my lips, though the memory is surely another's. If salvation awaits me, then I promise to take it, whether or not I deserve it. Tonight, though, the jungle shrouds me. I sit under a chrome belfry, a leech kneading at my skin. I feel no compulsion to bar its work. The companionship is welcome. Now to awaken the past once more. Moghra'yi. O, how the dromads know! How did I survive the Great Desert? I cannot say for sure. I owe something to the songs I sang at Perth, to the falconry I learned on Ettinspine, to the honing of my sinew under Oth. The fractured dream that was my voyage offers little elucidation. But I do remember chewing salt, and summoning gusts of wind to suck the moisture out of the dead air. Of all the riven memories that churn in my head, Moghra'yi only accounts for a coherent three. The first is a vision of ruin at the fabled Kubrisyeti, the seat of the Wrathchild empire. Stretched across the alabaster plain were the last of their wicked structures, toppled effigies of the manscorpions themselves. I reposed there, against my will and only out of exhaustion. Fixated on the night sky in fear and fatigue, I heard the echoes of a great saltback's steps. Even those gargantuans know to avoid the place. The second memory is the presence of the giants. Colossal skeletons in the forms of humanoids litter the desert. I had heard the accounts before, and indeed some of the specimens extend as far north as Odrum. Their history is wholly unknown to us or anyone else, but their bones provide some of the only shade on the salt pans, and so they grew to be welcome sights. So it was with horror that I discerned a truth, one night, while tucked into the rib cage of one of three cadavers in a small proximity. You see, all the carcasses shared a commonality: their orientation. East and south. East and south. The third memory is the rust. Rusted hulls and rusted spires. It took me weeks to realize what I beheld. For all my boyhood was rife with sketties and ships, but none were of this size. I envisioned what the past must have looked like, as little sense as it made to me. Moghra'yi as a vast sea, and monstrous vessels mounting its waves. Markings and etches on their hulls had no resonance for me then. Later, though, I recognized the figure of the Spindle, and I glimpsed the importance it held for these sea-faring Eaters. When I lurched to the ground and was righted by a brown-skinned man with a knife and glowing cat at his feet, I knew it was a dream. It must be a dream. I owe my life to =SEEKERENEMY=. In my haste to reach the fungal grove, I misstepped. My wandering and buoyant mind blinded itself to the presence of a chosen one. She neared me, set on enveloping me back into the fold, but she was intercepted by =SEEKERENEMY=. For what purpose did they intercede I do not know, but they have my thanks. So must they have the Elder's ire. I come now to Qud and the Seekers of the Sightless Way. The chanting compounded in my skull into a final ecstasy, wherein I joined an aggregate mind the likes of which I hadn't fathomed. I was primed for it, though. From my first joining on Ettinspine all those years ago, to the Rite of Mirth chaperoned by the Free Seer, I knew that sight guides us no more than salt does. There is ecstasy in the aggregate, indeed, and the Elder is as powerful a center as I've known. But a center is it still, and where there is a center, there is a hierarchy. Which piece of my past endowed me with the wisdom to see through the ruse? Was it the boyhood memory of crashing waves on the Shore of Songs, echoed later in my vision at Moghra'yi? Was it the jealousy in the eyes of the Free Seer at Ibunudr? Or was it a mere wrinkle in the mass mind, owing nothing to my own struggle? I cannot say, but while the other canters piped their songs to weaken the Elder's prison, unknowing thralls to a greater mind, I escaped. It must not be forgotten that energy is not a simple factor, but is always a product of two factors--a mass with a velocity, a mass with a temperature, a quantity of electricity into a pressure, and so on. One may sometimes meet the statement that matter and energy are the two realities; both are spoken of as entities. It is much more philosophical to speak of matter and motion, for in the absence of motion there is no energy, and the energy varies with the amount of motion; and furthermore, to understand any manifestation of energy one must inquire what kind of motion is involved. This we do when we speak of mechanical energy as the energy involved in a body having a translatory motion; also, when we speak of heat as a vibratory, and of light as a wave motion. To speak of energy without stating or implying these distinctions, is to speak loosely and to keep far within the bounds of actual knowledge. To speak thus of a body possessing energy, or expending energy, is to imply that the body possesses some kind of motion, and produces pressure upon another body because it has motion. Tait and others have pointed out the fact, that what is called potential energy must, in its nature, be kinetic. Tait says--Now it is impossible to conceive of a truly dormant form of energy, whose magnitude should depend, in any way, upon the unit of time; and we are forced to conclude that potential energy, like kinetic energy, depends(even if unexplained or unimagined) upon motion. All this means that it is now too late tostop with energy as a final factor in any phenomenon, that the _form of  motion_ which embodies the energy is the factor that determines _what_ happens, as distinguished from how _much_ happens. Here, then, are to be found the distinctions which have heretofore been called forces; here is embodied the proof that direct pressure of one body upon another is what causes the latter to move, and that the direction of movement depends on the point of application, with reference to the centre of mass.");
		}
		while (CorpusBuilder.Contains("\n\n\n"))
		{
			CorpusBuilder = CorpusBuilder.Replace("\n\n\n", "\n\n");
		}
		while (ObjectCorpusBuilder.Contains("\n\n\n"))
		{
			ObjectCorpusBuilder = ObjectCorpusBuilder.Replace("\n\n\n", "\n\n");
		}
		File.WriteAllText("Assets/StreamingAssets/Base/ " + GetSelectedFilename() + ".raw.txt", CorpusBuilder.ToString());
		File.AppendAllText("Assets/StreamingAssets/Base/ " + GetSelectedFilename() + ".raw.txt", ObjectCorpusBuilder.ToString());
		MarkovChainData Data = MarkovChain.BuildChain(CorpusBuilder.Replace("\n", " ").Replace("\r", "").Replace("  ", " ")
			.Replace("  ", " ")
			.Replace("  ", " ")
			.Replace("  ", " ")
			.Replace("~J211", "")
			.ToString(), 2);
		Data = MarkovChain.AppendCorpus(Data, ObjectCorpusBuilder.Replace("\n", " ").Replace("\r", "").Replace("  ", " ")
			.Replace("  ", " ")
			.Replace("  ", " ")
			.Replace("  ", " ")
			.Replace("~J211", "")
			.ToString(), addOpeningWords: false);
		ProgressUpdate("Saving corpus...");
		yield return new WaitForEndOfFrame();
		Data.SaveToFile("Assets/StreamingAssets/Base/" + GetSelectedFilename());
		Popup.Suppress = false;
		MessageQueue.Suppress = false;
		MinEvent.SuppressThreadWarning = false;
		Generating = false;
		CloseProgress();
	}

	public static void StripTextInBrackets(StringBuilder input)
	{
		Match match = Regex.Match(input.ToString(), "\\[\\[[^\\0]*?\\]\\]");
		while (match != null && !string.IsNullOrEmpty(match.Value))
		{
			input.Replace(match.Groups[0].Value, "");
			match = match.NextMatch();
		}
	}

	public static void StripSentenceQuotes(StringBuilder Input)
	{
		int i = 0;
		int num = -1;
		for (; i < Input.Length; i++)
		{
			char c = Input[i];
			if (c == '"')
			{
				num = ((num != -1) ? (-1) : i);
			}
			else if (char.IsWhiteSpace(c) && num != -1)
			{
				Input.Remove(num, 1);
				num = -1;
			}
		}
	}

	public string GetSelectedFilename()
	{
		return rootObject.transform.Find("Include/OutputFilename").GetComponent<InputField>().text;
	}

	public bool GetCheck(string id)
	{
		return rootObject.transform.Find("Include/" + id).gameObject.GetComponent<Toggle>().isOn;
	}

	public void ProgressUpdate(string s)
	{
		rootObject.transform.Find("ProgressPanel").gameObject.SetActive(value: true);
		rootObject.transform.Find("ProgressPanel/ProgressLabel").gameObject.GetComponent<UnityEngine.UI.Text>().text = s;
	}

	public void CloseProgress()
	{
		rootObject.transform.Find("ProgressPanel").gameObject.SetActive(value: false);
	}

	public void Generate()
	{
		StartCoroutine("Generate_");
	}
}
