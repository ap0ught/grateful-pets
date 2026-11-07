using NUnit.Framework;
using XRL.World;

namespace XRL;

public class GameTextTest
{
	[SetUp]
	public void Load()
	{
		if (Gender.Genders.IsNullOrEmpty())
		{
			Gender.Init();
		}
	}

	[TestCase(new object[] { "=subject.T= =verb:go= beep.", "robot", false, null, false, "The robot goes beep." })]
	[TestCase(new object[] { "=subject.An= =verb:go= beep.", "robot", false, null, false, "A robot goes beep." })]
	[TestCase(new object[] { "=subject.An= =verb:go= beep.", "pair of robots", true, null, false, "A pair of robots go beep." })]
	[TestCase(new object[] { "=subject.An= =verb:fly= overhead!", "eagle", false, null, false, "An eagle flies overhead!" })]
	[TestCase(new object[] { "Sadly, =subject.an= =verb:die=.", "snapjaw", false, null, false, "Sadly, a snapjaw dies." })]
	[TestCase(new object[] { "A spot appears on the =bodypart:Hand= of =subject.an=.", "snapjaw", false, null, false, "A spot appears on the body of a snapjaw." })]
	[TestCase(new object[] { "A trio of tongues vegetate from =subject.t's= =bodypart:Face=.", "snapjaw", false, null, false, "A trio of tongues vegetate from the snapjaw's body." })]
	public void VariableReplace(string Message, string ExplicitSubject, bool ExplicitSubjectPlural, string ExplicitObject, bool ExplicitObjectPlural, string Expected)
	{
		Assert.AreEqual(Expected, GameText.VariableReplace(Message, ExplicitSubject, ExplicitSubjectPlural, ExplicitObject, ExplicitObjectPlural));
	}
}
