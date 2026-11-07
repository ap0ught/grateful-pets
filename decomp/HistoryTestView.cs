using System.IO;
using HistoryKit;
using Qud.UI;
using UnityEngine.UI;
using XRL.Annals;
using XRL.UI;

[UIView("HistoryTest", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "HistoryTest", UICanvasHost = 1)]
public class HistoryTestView : SingletonWindowBase<HistoryTestView>
{
	public void OnCommand(string Command)
	{
		if (Command == "Back")
		{
			UIManager.getWindow("HistoryTest").Hide();
			UIManager.showWindow("MainMenu");
		}
		if (Command == "Generate")
		{
			History history = QudHistoryFactory.GenerateNewSultanHistory();
			GetChildComponent<UnityEngine.UI.Text>("Controls/Scroll View/Viewport/Content/Output").text = history.Dump(bVerbose: false);
			GetChildComponent<ScrollRect>("Controls/Scroll View").verticalNormalizedPosition = 1f;
			File.WriteAllText("history_log.txt", GetChildComponent<UnityEngine.UI.Text>("Controls/Scroll View/Viewport/Content/Output").text);
		}
		_ = Command == "FailureRedirect";
		if (Command == "TestVar")
		{
			History history2 = QudHistoryFactory.GenerateNewSultanHistory();
			HistoricEntity historicEntity = history2.CreateEntity(0L);
			historicEntity.ApplyEvent(new SetEntityProperty("organizingPrincipleType", "glassblower"));
			GetChildComponent<UnityEngine.UI.Text>("Controls/Scroll View/Viewport/Content/Output").text = HistoricStringExpander.ExpandString("<$prof=entity.organizingPrincipleType><spice.professions.$prof.plural>", historicEntity.GetCurrentSnapshot(), history2);
		}
	}
}
