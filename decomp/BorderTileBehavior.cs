using UnityEngine;

public class BorderTileBehavior : MonoBehaviour
{
	public int x;

	public int y;

	public string Direction;

	public void SetDirection(string d, int x, int y)
	{
		this.x = x;
		this.y = y;
		Direction = d;
	}
}
