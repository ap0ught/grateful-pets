using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using XRL;
using XRL.UI;

namespace Qud.UI;

[HasDebugCommand]
[UIView("DebugConsole", false, false, false, "Menu", "Console", false, 0, false, UICanvasHost = 1)]
public static class DebugConsole
{
	[HasDebugCommand]
	public class CallCommands
	{
		private Type selectedType;

		[DebugCommand]
		public void select(string[] arguments)
		{
			if (arguments.Length == 0)
			{
				selectedType = null;
			}
			selectedType = ModManager.ResolveType(arguments[0]);
			if (selectedType == null)
			{
				WriteLine("unknown type, clearing selection");
			}
			else
			{
				WriteLine("selected " + selectedType.Name);
			}
		}

		[DebugCommand]
		public void list(string[] arguments)
		{
			if (selectedType == null)
			{
				WriteLine("nothing is selected");
			}
			WriteLine("[methods]");
			MethodInfo[] methods = selectedType.GetMethods();
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.IsStatic)
				{
					WriteLine("static " + methodInfo.Name);
				}
			}
		}

		public static object ParseMethodParameter(string input, ParameterInfo param)
		{
			if (param.ParameterType == typeof(string))
			{
				return input;
			}
			if (param.ParameterType == typeof(int))
			{
				return int.Parse(input);
			}
			if (param.ParameterType == typeof(float))
			{
				return float.Parse(input);
			}
			if (param.ParameterType == typeof(double))
			{
				return double.Parse(input);
			}
			return input;
		}

		[DebugCommand]
		public void call(params string[] arguments)
		{
			if (selectedType == null)
			{
				WriteLine("nothing is selected");
				return;
			}
			MethodInfo method = selectedType.GetMethod(arguments[0]);
			List<object> list = new List<object>();
			List<ParameterInfo> list2 = method.GetParameters().ToList();
			string text = "";
			for (int i = 0; i < list2.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				list.Add(ParseMethodParameter(arguments[i + 1], list2[i]));
				text += arguments[i + 1];
			}
			if (method.IsStatic)
			{
				WriteLine("calling static method " + selectedType.Name + "::" + method.Name + "(" + text + ")");
				method.Invoke(null, list.ToArray());
			}
		}
	}

	public class Command
	{
		public string name;

		public MethodInfo method;

		public void Invoke(List<string> arguments)
		{
			object obj = null;
			if (!method.IsStatic)
			{
				if (!commandObjects.ContainsKey(method.DeclaringType))
				{
					commandObjects.Add(method.DeclaringType, Activator.CreateInstance(method.DeclaringType));
				}
				obj = commandObjects[method.DeclaringType];
			}
			if (method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string[]))
			{
				method.Invoke(obj, new object[1] { arguments.ToArray() });
				return;
			}
			if (method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(List<string>))
			{
				method.Invoke(obj, new object[1] { arguments });
				return;
			}
			MethodInfo methodInfo = method;
			object obj2 = obj;
			object[] parameters = arguments.ToArray();
			methodInfo.Invoke(obj2, parameters);
		}
	}

	public static bool dirty = false;

	public static Dictionary<string, Command> commands = new Dictionary<string, Command>();

	public static Dictionary<Type, object> commandObjects = new Dictionary<Type, object>();

	public static void claimCommandInstance(Type type, object claimant)
	{
		if (commandObjects.ContainsKey(type))
		{
			commandObjects[type] = claimant;
		}
		else
		{
			commandObjects.Add(type, claimant);
		}
	}

	public static void Initialize()
	{
		if (!commands.IsNullOrEmpty())
		{
			return;
		}
		WriteLine("Initializing debug console commands...");
		foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeof(DebugCommand), typeof(HasDebugCommand)))
		{
			Command command = new Command();
			command.name = item.Name.ToLower();
			command.method = item;
			if (commands.ContainsKey(command.name))
			{
				MetricsManager.LogWarning("Multiple debug commands defined for: " + command.name);
			}
			else
			{
				commands.Add(command.name, command);
			}
		}
	}

	public static void Show()
	{
		Initialize();
		SingletonWindowBase<ConsoleWindow>.instance.Show();
	}

	[DebugCommand]
	public static void Hide()
	{
		SingletonWindowBase<ConsoleWindow>.instance.Hide();
	}

	[DebugCommand]
	public static void Clear()
	{
		dirty = true;
	}

	[DebugCommand]
	public static void Write(string value)
	{
		dirty = true;
	}

	[DebugCommand]
	public static void WriteLine(string value = "")
	{
		Write(value);
		Write("\n");
		dirty = true;
	}

	[DebugCommand]
	public static void Help()
	{
		Initialize();
		foreach (KeyValuePair<string, Command> command in commands)
		{
			WriteLine(command.Key);
		}
	}

	public static List<string> ParseArguments(string command)
	{
		List<string> list = (from Match m in Regex.Matches(command, "[\\\"].+?[\\\"]|[^ ]+")
			select m.Value).ToList();
		if (list.Count > 0)
		{
			list.RemoveAt(0);
		}
		return list;
	}

	public static void Execute(string command)
	{
		Initialize();
		WriteLine();
		WriteLine("> " + command);
		WriteLine();
		List<string> arguments = ParseArguments(command);
		command = command.Split(' ')[0].ToLower();
		if (commands.ContainsKey(command))
		{
			commands[command.ToLower()].Invoke(arguments);
			return;
		}
		Help();
		WriteLine("unknown command: " + command);
	}

	public static void ScrollToBottom()
	{
		SingletonWindowBase<ConsoleWindow>.instance.ScrollToBottom();
	}
}
