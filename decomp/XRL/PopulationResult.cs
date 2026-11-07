namespace XRL;

public class PopulationResult
{
	public string Blueprint;

	public string Hint;

	public string Builder;

	public int Number;

	public PopulationResult(string Blueprint, int Number = 1, string Hint = null, string Builder = null)
	{
		this.Blueprint = Blueprint;
		this.Number = Number;
		this.Hint = Hint;
		this.Builder = Builder;
	}
}
