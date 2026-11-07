using System.Text;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Help;
using XRL.UI;
using XRL.World;
using XRL.World.Conversations;

namespace XRL;

public static class The
{
	private static StringBuilder _stringBuilder = new StringBuilder(4096);

	public static XRLCore Core => XRLCore.Core;

	/// you just lost
	public static XRLGame Game => Core?.Game;

	public static long CurrentTurn => XRLCore.CurrentTurn;

	public static GameObject Player => Game?.Player?.Body;

	public static Cell PlayerCell => Player?.CurrentCell;

	public static ParticleManager ParticleManager => XRLCore.ParticleManager;

	public static ActionManager ActionManager => Game?.ActionManager;

	public static ZoneManager ZoneManager => Game?.ZoneManager;

	public static Zone ActiveZone => ZoneManager?.ActiveZone;

	public static Graveyard Graveyard => ZoneManager?.Graveyard;

	public static ColorUtility.ColorCollection Color => ColorUtility.Colors;

	public static SynchronizationContext CurrentContext => SynchronizationContext.Current;

	public static SynchronizationContext UiContext => GameManager.Instance.uiSynchronizationContext;

	public static SynchronizationContext GameContext => GameManager.Instance.gameThreadSynchronizationContext;

	public static GameObject Speaker => ConversationUI.Speaker;

	public static GameObject Listener => ConversationUI.Listener;

	public static Conversation Conversation => ConversationUI.CurrentConversation;

	public static XRLManual Manual => XRLCore.Manual;

	public static StringBuilder StringBuilder
	{
		get
		{
			_stringBuilder.Clear();
			return _stringBuilder;
		}
	}
}
