using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace XRL.UI.Framework;

[TestFixture]
public class FrameworkTests
{
	public NavigationController controller => NavigationController.instance;

	public Event currentEvent => NavigationController.currentEvent;

	[SetUp]
	public void Setup()
	{
		NavigationController.instance = new NavigationController();
	}

	[TearDown]
	public void Teardown()
	{
		NavigationController.instance.activeContext = null;
	}

	[Test]
	public void TestSimpleButtons()
	{
		int testPhase = 0;
		int asserts = 0;
		NavigationContext navigationContext = new NavigationContext();
		navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>
		{
			{
				InputButtonTypes.AcceptButton,
				() =>
				{
					Assert.AreEqual(1, testPhase);
					asserts++;
				}
			},
			{
				InputButtonTypes.CancelButton,
				() =>
				{
					Assert.AreEqual(2, testPhase);
					asserts++;
				}
			}
		};
		navigationContext.Activate();
		testPhase = 1;
		controller.FireInputButtonEvent(InputButtonTypes.AcceptButton);
		testPhase = 2;
		controller.FireInputButtonEvent(InputButtonTypes.CancelButton);
		Assert.AreEqual(2, asserts);
	}

	[Test]
	public void TestEnterExit()
	{
		int testPhase = 0;
		int asserts = 0;
		NavigationContext navigationContext = null;
		NavigationContext firstContext = null;
		NavigationContext secondContext = null;
		List<Action> list = new List<Action>
		{
			() =>
			{
				if (testPhase == 1)
				{
					Assert.AreEqual(firstContext, currentEvent.data["to"]);
					Assert.AreEqual(null, currentEvent.data["from"]);
					asserts++;
				}
				else if (testPhase == 3)
				{
					Assert.AreEqual(secondContext, currentEvent.data["to"]);
					Assert.AreEqual(firstContext, currentEvent.data["from"]);
					asserts++;
				}
				else
				{
					Assert.Fail("Should not be called: testPhase " + testPhase);
				}
			},
			() =>
			{
				if (testPhase == 2)
				{
					Assert.AreEqual(secondContext, currentEvent.data["to"]);
					Assert.AreEqual(firstContext, currentEvent.data["from"]);
					asserts++;
					testPhase++;
				}
				else if (testPhase == 4)
				{
					Assert.AreEqual(null, currentEvent.data["to"]);
					Assert.AreEqual(secondContext, currentEvent.data["from"]);
					asserts++;
					testPhase++;
				}
				else
				{
					Assert.Fail("Should not be called");
				}
			},
			() =>
			{
				Assert.AreEqual(1, testPhase);
				Assert.AreEqual(firstContext, currentEvent.data["to"]);
				Assert.AreEqual(null, currentEvent.data["from"]);
				asserts++;
			},
			() =>
			{
				Assert.AreEqual(2, testPhase);
				Assert.AreEqual(secondContext, currentEvent.data["to"]);
				Assert.AreEqual(firstContext, currentEvent.data["from"]);
				asserts++;
			},
			() =>
			{
				Assert.AreEqual(3, testPhase);
				Assert.AreEqual(secondContext, currentEvent.data["to"]);
				Assert.AreEqual(firstContext, currentEvent.data["from"]);
				asserts++;
			},
			() =>
			{
				Assert.AreEqual(4, testPhase);
				Assert.AreEqual(null, currentEvent.data["to"]);
				Assert.AreEqual(secondContext, currentEvent.data["from"]);
				asserts++;
			}
		};
		navigationContext = new NavigationContext
		{
			enterHandler = list[0],
			exitHandler = list[1]
		};
		firstContext = new NavigationContext
		{
			enterHandler = list[2],
			exitHandler = list[3],
			parentContext = navigationContext
		};
		secondContext = new NavigationContext
		{
			enterHandler = list[4],
			exitHandler = list[5],
			parentContext = navigationContext
		};
		testPhase = 1;
		firstContext.Activate();
		testPhase = 2;
		secondContext.Activate();
		testPhase = 4;
		controller.activeContext = null;
		Assert.AreEqual(8, asserts);
	}

	[Test]
	public void TestSimpleAxis()
	{
		int testPhase = 0;
		int asserts = 0;
		NavigationContext activeContext = new NavigationContext
		{
			axisHandlers = new Dictionary<InputAxisTypes, Action>
			{
				{
					InputAxisTypes.NavigationXAxis,
					() =>
					{
						Assert.AreEqual(1, testPhase);
						asserts++;
					}
				},
				{
					InputAxisTypes.NavigationYAxis,
					() =>
					{
						Assert.AreEqual(2, testPhase);
						asserts++;
					}
				}
			}
		};
		controller.activeContext = activeContext;
		testPhase = 1;
		controller.FireInputAxisEvent(InputAxisTypes.NavigationXAxis);
		testPhase = 2;
		controller.FireInputAxisEvent(InputAxisTypes.NavigationYAxis);
		testPhase = 3;
		controller.FireInputAxisEvent(InputAxisTypes.NavigationUAxis);
		Assert.AreEqual(2, asserts);
	}

	[Test]
	public void TestParentContextsWithCancel()
	{
		int testPhase = 0;
		int asserts = 0;
		NavigationContext parentContext = new NavigationContext
		{
			buttonHandlers = new Dictionary<InputButtonTypes, Action>
			{
				{
					InputButtonTypes.AcceptButton,
					() =>
					{
						Assert.AreEqual(1, testPhase);
						asserts++;
					}
				},
				{
					InputButtonTypes.CancelButton,
					() =>
					{
						Assert.Fail("Should not be called!");
					}
				}
			}
		};
		NavigationContext activeContext = new NavigationContext
		{
			parentContext = parentContext,
			buttonHandlers = new Dictionary<InputButtonTypes, Action>
			{
				{
					InputButtonTypes.AcceptButton,
					() =>
					{
						Assert.AreEqual(1, testPhase);
						asserts++;
					}
				},
				{
					InputButtonTypes.CancelButton,
					() =>
					{
						Assert.AreEqual(2, testPhase);
						asserts++;
						currentEvent.Cancel();
					}
				}
			}
		};
		controller.activeContext = activeContext;
		testPhase = 1;
		controller.FireInputButtonEvent(InputButtonTypes.AcceptButton);
		testPhase = 2;
		controller.FireInputButtonEvent(InputButtonTypes.CancelButton);
		Assert.AreEqual(3, asserts);
	}

	[Test]
	public void TestIsActive()
	{
		NavigationContext navigationContext = new NavigationContext();
		NavigationContext navigationContext2 = new NavigationContext
		{
			parentContext = navigationContext
		};
		Assert.AreEqual(false, navigationContext.IsActive());
		Assert.AreEqual(false, navigationContext2.IsActive());
		Assert.AreEqual(false, navigationContext.IsActive(checkParents: false));
		Assert.AreEqual(false, navigationContext2.IsActive(checkParents: false));
		navigationContext.Activate();
		Assert.AreEqual(true, navigationContext.IsActive());
		Assert.AreEqual(true, navigationContext.IsActive(checkParents: false));
		Assert.AreEqual(false, navigationContext2.IsActive());
		Assert.AreEqual(false, navigationContext2.IsActive(checkParents: false));
		navigationContext2.Activate();
		Assert.AreEqual(true, navigationContext2.IsActive());
		Assert.AreEqual(true, navigationContext2.IsActive());
		Assert.AreEqual(true, navigationContext.IsActive());
		Assert.AreEqual(false, navigationContext.IsActive(checkParents: false));
	}
}
