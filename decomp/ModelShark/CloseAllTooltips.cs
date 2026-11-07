using UnityEngine;
using UnityEngine.UI;

namespace ModelShark;

[RequireComponent(typeof(Button))]
public class CloseAllTooltips : MonoBehaviour
{
	private void Start()
	{
		gameObject.GetComponent<Button>().onClick.AddListener(() =>
		{
			TooltipManager.Instance.CloseAll();
		});
	}
}
