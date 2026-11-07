using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Platform.IO;

public static class TestGenerator
{
	public delegate Snapshot TestDelegate();

	public class Snapshot
	{
		public Dictionary<string, string> tests = new Dictionary<string, string>();

		public DateTime lastSaved;
	}

	public class TestBuilder
	{
		private StringBuilder builder = new StringBuilder();

		public void Append(string txt)
		{
			builder.Append(txt);
		}

		public void AppendFilePath(string path, bool addLineBreakAtEnd = true)
		{
			if (System.IO.Path.GetExtension(path) == ".meta")
			{
				return;
			}
			string text = "IOParityTesting";
			if (!path.Contains(path))
			{
				Debug.LogError("Path " + path + " does not contain folder IOParityTesting and therefore cannot be serialized without breaking the tests. Will not be part of the serialized snapshot state.");
				Append(path + " (error appending)");
			}
			ReadOnlySpan<char> readOnlySpan = path.AsSpan(0);
			for (int i = 1; i < path.Length; i++)
			{
				int length = i;
				int start = path.Length - i;
				ReadOnlySpan<char> readOnlySpan2 = path.AsSpan(start, length);
				if (readOnlySpan2.Contains(text.AsSpan(0), StringComparison.Ordinal))
				{
					readOnlySpan = readOnlySpan2;
					break;
				}
			}
			string txt = SanitizePath(readOnlySpan.ToString());
			Append(txt);
			if (addLineBreakAtEnd)
			{
				Append("\n");
			}
		}

		public void Append(byte[] bytes)
		{
			foreach (byte b in bytes)
			{
				builder.Append(b.ToString("X2"));
			}
		}

		public void Append(bool boolean)
		{
			if (boolean)
			{
				builder.Append("true");
			}
			else
			{
				builder.Append("false");
			}
		}

		public string Serialize()
		{
			return builder.ToString();
		}
	}

	private static bool useRomPaths;

	private const string TestDirectoryName = "IOParityTesting";

	public static Snapshot GenerateSnapshotBaseline()
	{
		Snapshot snapshot = new Snapshot();
		snapshot.tests = new Dictionary<string, string>();
		snapshot.lastSaved = DateTime.Now;
		byte[] byteContent = new byte[20]
		{
			0, 0, 0, 0, 1, 2, 3, 4, 5, 6,
			7, 8, 9, 10, 0, 1, 2, 3, 4, 5
		};
		string txtContent = "abcdeefghei&*nc.a7d7^&*^)@$%^d/wqi321//312asrj\\n23";
		Utils.OnScopeExit onScopeExit = new Utils.OnScopeExit(() =>
		{
			useRomPaths = false;
			deleteTestDirectory("cleanup");
		});
		try
		{
			runTestsRwAndRom("ReadAllText", (TestBuilder blder) =>
			{
				blder.Append(System.IO.File.ReadAllText(PathPresent()));
			});
			runTestsRwAndRom("ReadAllText -- Missing Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(System.IO.File.ReadAllText(PathMissing()));
				});
			});
			runTestsRwAndRom("ReadAllText -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(System.IO.File.ReadAllText(PathInvalid()));
				});
			});
			runTestsRwAndRom("ReadAllBytes", (TestBuilder blder) =>
			{
				blder.Append(System.IO.File.ReadAllBytes(PathPresent()));
			});
			runTestsRwAndRom("ReadAllBytes -- Missing Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(System.IO.File.ReadAllBytes(PathMissing()));
				});
			});
			runTestsRwAndRom("ReadAllBytes -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(System.IO.File.ReadAllBytes(PathInvalid()));
				});
			});
			runTestRW("WriteAllText", (TestBuilder hashRef) =>
			{
				System.IO.File.WriteAllText(PathAvailableText(), txtContent);
			});
			runTestRW("WriteAllText -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.WriteAllText(PathInvalid(), txtContent);
				});
			});
			runTestRW("WriteAllText -- Occupied Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.WriteAllText(PathAvailableText(), "first text");
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.WriteAllText(PathAvailableText(), txtContent);
				});
			});
			runTestRW("WriteAllBytes", (TestBuilder hashRef) =>
			{
				System.IO.File.WriteAllBytes(PathAvailableText(), byteContent);
			});
			runTestRW("WriteAllBytes -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.WriteAllBytes(PathInvalid(), byteContent);
				});
			});
			runTestRW("WriteAllBytes -- Occupied Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.WriteAllBytes(PathAvailableText(), new byte[3] { 0, 1, 2 });
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.WriteAllBytes(PathAvailableText(), byteContent);
				});
			});
			runTestsRwAndRom("FileExists", (TestBuilder blder) =>
			{
				blder.Append(System.IO.File.Exists(PathPresent()));
				blder.Append(System.IO.File.Exists(PathMissing()));
			});
			runTestRW("FileDelete -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: false, () =>
				{
					System.IO.File.Delete(PathInvalid());
				});
			});
			runTestRW("FileDelete -- Missing Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: false, () =>
				{
					System.IO.File.Delete(PathMissing());
				});
			});
			runTestRW("Copy -- overwrite: true", (TestBuilder hashRef) =>
			{
				System.IO.File.WriteAllText(GetPath("CopySource01.txt"), "copy source content 01");
				System.IO.File.WriteAllText(GetPath("CopySource02.txt"), "copy source content 02");
				System.IO.File.Copy(GetPath("CopySource01.txt"), GetPath("CopySource02.txt"), overwrite: true);
			});
			runTestRW("Copy -- overwrite: false", (TestBuilder blder) =>
			{
				System.IO.File.WriteAllText(GetPath("CopySource01.txt"), "copy source content 01");
				System.IO.File.WriteAllText(GetPath("CopySource02.txt"), "copy source content 02");
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.Copy(GetPath("CopySource01.txt"), GetPath("CopySource02.txt"), overwrite: false);
				});
			});
			runTestRW("Copy -- missing source", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.Copy(PathMissing(), PathAvailableText(), overwrite: false);
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.Copy(PathMissing(), PathAvailableText(), overwrite: true);
				});
			});
			runTestRW("Copy -- folder source", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.Copy(PathRootDirectory(), PathAvailableText(), overwrite: false);
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.Copy(PathRootDirectory(), PathAvailableText(), overwrite: true);
				});
			});
			runTestsRwAndRom("File Info", (TestBuilder blder) =>
			{
				System.IO.FileInfo fileInfo = new System.IO.FileInfo(PathPresent());
				serializeFileSystemInfo(fileInfo, blder);
				blder.Append(fileInfo.Length.ToString());
			});
			runTestsRwAndRom("Directory Info", (TestBuilder blder) =>
			{
				serializeFileSystemInfo(new System.IO.DirectoryInfo(PathRootDirectory()), blder);
			});
			runTestsRwAndRom("File Info -- Missing", (TestBuilder blder) =>
			{
				System.IO.FileInfo fileInfo = new System.IO.FileInfo(PathMissing());
				blder.Append(fileInfo.Exists);
			});
			runTestsRwAndRom("Directory Info -- Missing", (TestBuilder blder) =>
			{
				System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(PathMissing());
				blder.Append(directoryInfo.Exists);
			});
			runTestsRwAndRom("File Info -- Missing Path", (TestBuilder hashRef) =>
			{
				SerializeAnyThrows(hashRef, careAboutExceptionType: true, () =>
				{
					new System.IO.FileInfo(PathMissing());
				});
				SerializeAnyThrows(hashRef, careAboutExceptionType: true, () =>
				{
					new System.IO.FileInfo(PathParentMissing());
				});
			});
			runTestRW("DirectoryCreate -- Directory Parent Present", (TestBuilder blder) =>
			{
				System.IO.Directory.CreateDirectory(PathAvailableFolder());
			});
			runTestRW("DirectoryCreate -- Directory Parent Missing", (TestBuilder blder) =>
			{
				System.IO.Directory.CreateDirectory(PathAvailableFolderNoParent());
			});
			runTestRW("DirectoryCreate -- File Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.Directory.CreateDirectory(PathAvailableByte());
				});
			});
			runTestRW("DirectoryCreate -- Already Exists", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.Directory.CreateDirectory(PathRootDirectory());
				});
			});
			runTestRW("DirectoryExists", (TestBuilder blder) =>
			{
				blder.Append(System.IO.Directory.Exists(PathRootDirectory()));
				blder.Append(System.IO.Directory.Exists(PathMissing()));
			});
			runTestRW("DirectoryDelete -- Recursive: true", (TestBuilder blder) =>
			{
				System.IO.Directory.Delete(PathRootDirectory(), recursive: true);
			});
			runTestRW("DirectoryDelete -- Recursive: false", (TestBuilder blder) =>
			{
				System.IO.Directory.Delete(PathRootDirectory(), recursive: false);
			});
			runTestsRwAndRom("EnumerateDirectories -- Search Option Top Level", (TestBuilder blder) =>
			{
				foreach (string item in System.IO.Directory.EnumerateDirectories(PathRootDirectory()))
				{
					blder.AppendFilePath(item);
				}
			});
			runTestsRwAndRom("Enumerate Files -- (no other arguments)", (TestBuilder blder) =>
			{
				foreach (string item2 in System.IO.Directory.EnumerateFiles(PathRootDirectory()))
				{
					blder.AppendFilePath(item2);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Search: Shallow", (TestBuilder blder) =>
			{
				foreach (string item3 in sortPaths(System.IO.Directory.EnumerateFiles(PathRootDirectory(), "*", System.IO.SearchOption.TopDirectoryOnly)))
				{
					blder.AppendFilePath(item3);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item4 in sortPaths(System.IO.Directory.EnumerateFiles(PathRootDirectory(), "*", System.IO.SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item4);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Pattern: *.txt -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item5 in sortPaths(System.IO.Directory.EnumerateFiles(PathRootDirectory(), "*.txt", System.IO.SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item5);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Pattern: *.asset -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item6 in sortPaths(System.IO.Directory.EnumerateFiles(PathRootDirectory(), "*.asset", System.IO.SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item6);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Pattern: *subFile* -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item7 in sortPaths(System.IO.Directory.EnumerateFiles(PathRootDirectory(), "*subFile*", System.IO.SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item7);
				}
			});
			runTestsRwAndRom("Enumerate File System Entries -- Pattern: * -- Search: All", (TestBuilder blder) =>
			{
				List<System.IO.FileSystemInfo> list = new System.IO.DirectoryInfo(PathRootDirectory()).EnumerateFileSystemInfos().ToList();
				list.Sort((System.IO.FileSystemInfo a, System.IO.FileSystemInfo b) => StringComparer.OrdinalIgnoreCase.Compare(a.FullName, b.FullName));
				foreach (System.IO.FileSystemInfo item8 in list)
				{
					blder.AppendFilePath(item8.FullName);
				}
			});
			runTestsRwAndRom("Stream -- Read -- Length", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenRead(PathPresent());
				blder.Append(fileStream.Length.ToString());
			});
			runTestsRwAndRom("Stream -- Read -- All", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenRead(PathPresent());
				byte[] array = new byte[fileStream.Length];
				blder.Append(fileStream.Read(array, 0, array.Length).ToString());
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- with Offset ", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenRead(PathPresent());
				byte[] array = new byte[4];
				blder.Append(fileStream.Read(array, 1, 2).ToString());
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- with Seek & Offset", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenRead(PathPresent());
				byte[] array = new byte[10];
				fileStream.Seek(1L, SeekOrigin.Begin);
				fileStream.Read(array, 0, 2);
				fileStream.Read(array, 2, 2);
				fileStream.Seek(-4L, SeekOrigin.End);
				fileStream.Read(array, 6, 2);
				fileStream.Seek(-8L, SeekOrigin.Current);
				fileStream.Read(array, 8, 2);
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- from stream end", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenRead(PathPresent());
				fileStream.Seek(0L, SeekOrigin.End);
				byte[] array = new byte[1];
				blder.Append(fileStream.Read(array, 0, 1).ToString());
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- Missing", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					using (System.IO.File.OpenRead(PathMissing()))
					{
					}
				});
			});
			runTestRW("Stream -- Write -- Open Missing", (TestBuilder blder) =>
			{
				using (System.IO.File.OpenWrite(PathAvailableByte()))
				{
				}
			});
			runTestRW("Stream -- Write -- Open & Flush Empty", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenWrite(PathAvailableByte());
				fileStream.Flush();
			});
			runTestRW("Stream -- Write -- Byte", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenWrite(PathAvailableByte());
				fileStream.WriteByte(30);
				fileStream.Flush();
			});
			runTestRW("Stream --  Write -- Buffer Offset & Length", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenWrite(PathAvailableByte());
				fileStream.Write(byteContent, 4, byteContent.Length - 8);
				fileStream.Flush();
			});
			runTestRW("Stream --  Write -- Buffer Offset & Length Seek", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenWrite(PathAvailableByte());
				fileStream.WriteByte(5);
				fileStream.WriteByte(3);
				fileStream.WriteByte(8);
				fileStream.Seek(1L, SeekOrigin.Begin);
				fileStream.WriteByte(9);
				fileStream.Flush();
			});
			runTestRW("Stream -- Write -- Open Existing Prepend & Append", (TestBuilder blder) =>
			{
				using FileStream fileStream = System.IO.File.OpenWrite(PathPresent());
				fileStream.WriteByte(9);
				fileStream.WriteByte(9);
				fileStream.Seek(0L, SeekOrigin.End);
				fileStream.WriteByte(9);
				fileStream.WriteByte(9);
				fileStream.Flush();
			});
			runTestRW("File -- AppendAllText", (TestBuilder blder) =>
			{
				System.IO.File.AppendAllText(PathPresent(), "appendText");
			});
			runTestRW("File -- AppendAllTextMissing", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					System.IO.File.AppendAllText(PathMissing(), "appendText");
				});
			});
			return snapshot;
		}
		finally
		{
			((IDisposable)onScopeExit/*cast due to constrained. prefix*/).Dispose();
		}
		static void deleteTestDirectory(string name)
		{
			try
			{
				if (System.IO.Directory.Exists(PathRootDirectory()))
				{
					System.IO.Directory.Delete(PathRootDirectory(), recursive: true);
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Could not delete test directory for test \"{name}\"\n. {arg}");
			}
		}
		static void getDirectoryStateRecursive(string currentDirPath, TestBuilder blder, int depth)
		{
			foreach (string item9 in sortPaths(System.IO.Directory.EnumerateFiles(currentDirPath)))
			{
				if (!(System.IO.Path.GetExtension(item9) == ".meta"))
				{
					appendFileHierarchyMarkers(">", depth);
					blder.AppendFilePath(item9, addLineBreakAtEnd: false);
					appendFileHierarchyMarkers("c", depth + 1);
					blder.Append(System.IO.File.ReadAllText(item9));
				}
			}
			List<string> list = System.IO.Directory.EnumerateDirectories(currentDirPath).ToList();
			list.Sort(StringComparer.OrdinalIgnoreCase);
			foreach (string item10 in list)
			{
				appendFileHierarchyMarkers("[", depth);
				blder.AppendFilePath(item10, addLineBreakAtEnd: false);
				getDirectoryStateRecursive(item10, blder, depth + 1);
			}
			void appendFileHierarchyMarkers(string arrowHead, int num)
			{
				blder.Append("\n");
				for (int i = 0; i < num; i++)
				{
					blder.Append("-");
				}
				blder.Append(arrowHead);
				blder.Append(" ");
			}
		}
		static void resetTestDirectory(string testName)
		{
			try
			{
				deleteTestDirectory(testName);
				System.IO.Directory.CreateDirectory(PathRootDirectory());
				System.IO.File.WriteAllText(PathPresent(), "present-path-string-content1234567*7d7^&*^\\)@$%^");
				string path = GetPath(Path.Combine("SubDirectory1", "SubDirectory2", "SubDirectory3", "SubDirectory4"));
				System.IO.Directory.CreateDirectory(path);
				System.IO.File.WriteAllText(Path.Combine(path, "subFile1.txt"), "present-path-string-content1234567*7d7^&*^\\)@$%^");
				System.IO.File.WriteAllText(Path.Combine(path, "subFile2.text"), "present-path-string-content1234567*7d7^&*^\\)@$%^");
				System.IO.File.WriteAllText(Path.Combine(path, "subFile3.note"), "present-path-string-content1234567*7d7^&*^\\)@$%^");
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("Could not {0}. So tests will fail. {1}", "resetTestDirectory", arg));
			}
		}
		void runTestROM(string testName, Action<TestBuilder> test)
		{
			useRomPaths = true;
			TestBuilder testBuilder = new TestBuilder();
			try
			{
				testBuilder.Append("Manual Hashes:\n");
				test(testBuilder);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(string.Format("test {0} threw an exception. This likely wasn't expected as the call wasn't wrapped in a {1}.\nMessage:\n{2}", testName, "SerializeAnyThrows", ex));
				testBuilder.Append(ex.GetType().FullName);
			}
			testBuilder.Append("\nDirectory State:");
			serializeDirectoryState(testBuilder);
			snapshot.tests.Add(testName, testBuilder.Serialize());
			useRomPaths = false;
		}
		void runTestRW(string testName, Action<TestBuilder> test)
		{
			useRomPaths = false;
			resetTestDirectory(testName);
			TestBuilder testBuilder = new TestBuilder();
			try
			{
				testBuilder.Append("Manual Hashes:\n");
				test(testBuilder);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(string.Format("test {0} threw an exception. This likely wasn't expected as the call wasn't wrapped in a {1}.\nMessage:\n{2}", testName, "SerializeAnyThrows", ex));
				testBuilder.Append(ex.GetType().FullName);
			}
			testBuilder.Append("\nDirectory State:");
			serializeDirectoryState(testBuilder);
			snapshot.tests.Add(testName, testBuilder.Serialize());
		}
		void runTestsRwAndRom(string testName, Action<TestBuilder> test)
		{
			runTestRW(testName + " -- RW", test);
			runTestROM(testName + " -- ROM", test);
		}
		static void serializeDirectoryState(TestBuilder blder)
		{
			if (!System.IO.Directory.Exists(PathRootDirectory()))
			{
				blder.Append("Test folder is missing");
			}
			else
			{
				getDirectoryStateRecursive(PathRootDirectory(), blder, 1);
			}
		}
		static void serializeFileSystemInfo(System.IO.FileSystemInfo info, TestBuilder blder)
		{
			blder.AppendFilePath(info.FullName ?? "");
			blder.Append(info.Name ?? "");
			blder.Append($"{info.Exists}");
			blder.Append(info.Extension ?? "");
			blder.Append($"{info.Attributes.HasFlag(System.IO.FileAttributes.Directory)}");
			blder.Append($"{info.Attributes.HasFlag(System.IO.FileAttributes.Hidden)}");
		}
		static List<string> sortPaths(IEnumerable<string> paths)
		{
			List<string> list = paths.ToList();
			list.Sort(StringComparer.OrdinalIgnoreCase);
			return list;
		}
	}

	public static Snapshot GenerateSnapshotCandidate__GENERATED()
	{
		Snapshot snapshot = new Snapshot();
		snapshot.tests = new Dictionary<string, string>();
		snapshot.lastSaved = DateTime.Now;
		byte[] byteContent = new byte[20]
		{
			0, 0, 0, 0, 1, 2, 3, 4, 5, 6,
			7, 8, 9, 10, 0, 1, 2, 3, 4, 5
		};
		string txtContent = "abcdeefghei&*nc.a7d7^&*^)@$%^d/wqi321//312asrj\\n23";
		Utils.OnScopeExit onScopeExit = new Utils.OnScopeExit(() =>
		{
			useRomPaths = false;
			deleteTestDirectory("cleanup");
		});
		try
		{
			runTestsRwAndRom("ReadAllText", (TestBuilder blder) =>
			{
				blder.Append(File.ReadAllText(PathPresent()));
			});
			runTestsRwAndRom("ReadAllText -- Missing Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(File.ReadAllText(PathMissing()));
				});
			});
			runTestsRwAndRom("ReadAllText -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(File.ReadAllText(PathInvalid()));
				});
			});
			runTestsRwAndRom("ReadAllBytes", (TestBuilder blder) =>
			{
				blder.Append(File.ReadAllBytes(PathPresent()));
			});
			runTestsRwAndRom("ReadAllBytes -- Missing Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(File.ReadAllBytes(PathMissing()));
				});
			});
			runTestsRwAndRom("ReadAllBytes -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					blder.Append(File.ReadAllBytes(PathInvalid()));
				});
			});
			runTestRW("WriteAllText", (TestBuilder hashRef) =>
			{
				File.WriteAllText(PathAvailableText(), txtContent);
			});
			runTestRW("WriteAllText -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.WriteAllText(PathInvalid(), txtContent);
				});
			});
			runTestRW("WriteAllText -- Occupied Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.WriteAllText(PathAvailableText(), "first text");
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.WriteAllText(PathAvailableText(), txtContent);
				});
			});
			runTestRW("WriteAllBytes", (TestBuilder hashRef) =>
			{
				File.WriteAllBytes(PathAvailableText(), byteContent);
			});
			runTestRW("WriteAllBytes -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.WriteAllBytes(PathInvalid(), byteContent);
				});
			});
			runTestRW("WriteAllBytes -- Occupied Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.WriteAllBytes(PathAvailableText(), new byte[3] { 0, 1, 2 });
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.WriteAllBytes(PathAvailableText(), byteContent);
				});
			});
			runTestsRwAndRom("FileExists", (TestBuilder blder) =>
			{
				blder.Append(File.Exists(PathPresent()));
				blder.Append(File.Exists(PathMissing()));
			});
			runTestRW("FileDelete -- Invalid Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: false, () =>
				{
					File.Delete(PathInvalid());
				});
			});
			runTestRW("FileDelete -- Missing Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: false, () =>
				{
					File.Delete(PathMissing());
				});
			});
			runTestRW("Copy -- overwrite: true", (TestBuilder hashRef) =>
			{
				File.WriteAllText(GetPath("CopySource01.txt"), "copy source content 01");
				File.WriteAllText(GetPath("CopySource02.txt"), "copy source content 02");
				File.Copy(GetPath("CopySource01.txt"), GetPath("CopySource02.txt"), overwrite: true);
			});
			runTestRW("Copy -- overwrite: false", (TestBuilder blder) =>
			{
				File.WriteAllText(GetPath("CopySource01.txt"), "copy source content 01");
				File.WriteAllText(GetPath("CopySource02.txt"), "copy source content 02");
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.Copy(GetPath("CopySource01.txt"), GetPath("CopySource02.txt"));
				});
			});
			runTestRW("Copy -- missing source", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.Copy(PathMissing(), PathAvailableText());
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.Copy(PathMissing(), PathAvailableText(), overwrite: true);
				});
			});
			runTestRW("Copy -- folder source", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.Copy(PathRootDirectory(), PathAvailableText());
				});
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.Copy(PathRootDirectory(), PathAvailableText(), overwrite: true);
				});
			});
			runTestsRwAndRom("File Info", (TestBuilder blder) =>
			{
				FileInfo fileInfo = new FileInfo(PathPresent());
				serializeFileSystemInfo(fileInfo, blder);
				blder.Append(fileInfo.Length.ToString());
			});
			runTestsRwAndRom("Directory Info", (TestBuilder blder) =>
			{
				serializeFileSystemInfo(new DirectoryInfo(PathRootDirectory()), blder);
			});
			runTestsRwAndRom("File Info -- Missing", (TestBuilder blder) =>
			{
				FileInfo fileInfo = new FileInfo(PathMissing());
				blder.Append(fileInfo.Exists);
			});
			runTestsRwAndRom("Directory Info -- Missing", (TestBuilder blder) =>
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(PathMissing());
				blder.Append(directoryInfo.Exists);
			});
			runTestsRwAndRom("File Info -- Missing Path", (TestBuilder hashRef) =>
			{
				SerializeAnyThrows(hashRef, careAboutExceptionType: true, () =>
				{
					new FileInfo(PathMissing());
				});
				SerializeAnyThrows(hashRef, careAboutExceptionType: true, () =>
				{
					new FileInfo(PathParentMissing());
				});
			});
			runTestRW("DirectoryCreate -- Directory Parent Present", (TestBuilder blder) =>
			{
				Directory.CreateDirectory(PathAvailableFolder());
			});
			runTestRW("DirectoryCreate -- Directory Parent Missing", (TestBuilder blder) =>
			{
				Directory.CreateDirectory(PathAvailableFolderNoParent());
			});
			runTestRW("DirectoryCreate -- File Path", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					Directory.CreateDirectory(PathAvailableByte());
				});
			});
			runTestRW("DirectoryCreate -- Already Exists", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					Directory.CreateDirectory(PathRootDirectory());
				});
			});
			runTestRW("DirectoryExists", (TestBuilder blder) =>
			{
				blder.Append(Directory.Exists(PathRootDirectory()));
				blder.Append(Directory.Exists(PathMissing()));
			});
			runTestRW("DirectoryDelete -- Recursive: true", (TestBuilder blder) =>
			{
				Directory.Delete(PathRootDirectory());
			});
			runTestRW("DirectoryDelete -- Recursive: false", (TestBuilder blder) =>
			{
				Directory.Delete(PathRootDirectory(), recursive: false);
			});
			runTestsRwAndRom("EnumerateDirectories -- Search Option Top Level", (TestBuilder blder) =>
			{
				foreach (string item in Directory.EnumerateDirectories(PathRootDirectory()))
				{
					blder.AppendFilePath(item);
				}
			});
			runTestsRwAndRom("Enumerate Files -- (no other arguments)", (TestBuilder blder) =>
			{
				foreach (string item2 in Directory.EnumerateFiles(PathRootDirectory()))
				{
					blder.AppendFilePath(item2);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Search: Shallow", (TestBuilder blder) =>
			{
				foreach (string item3 in sortPaths(Directory.EnumerateFiles(PathRootDirectory(), "*", SearchOption.TopDirectoryOnly)))
				{
					blder.AppendFilePath(item3);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item4 in sortPaths(Directory.EnumerateFiles(PathRootDirectory(), "*", SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item4);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Pattern: *.txt -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item5 in sortPaths(Directory.EnumerateFiles(PathRootDirectory(), "*.txt", SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item5);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Pattern: *.asset -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item6 in sortPaths(Directory.EnumerateFiles(PathRootDirectory(), "*.asset", SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item6);
				}
			});
			runTestsRwAndRom("Enumerate Files -- Pattern: *subFile* -- Search: All", (TestBuilder blder) =>
			{
				foreach (string item7 in sortPaths(Directory.EnumerateFiles(PathRootDirectory(), "*subFile*", SearchOption.AllDirectories)))
				{
					blder.AppendFilePath(item7);
				}
			});
			runTestsRwAndRom("Enumerate File System Entries -- Pattern: * -- Search: All", (TestBuilder blder) =>
			{
				List<FileSystemInfo> list = new DirectoryInfo(PathRootDirectory()).EnumerateFileSystemInfos().ToList();
				list.Sort((FileSystemInfo a, FileSystemInfo b) => StringComparer.OrdinalIgnoreCase.Compare(a.FullName, b.FullName));
				foreach (FileSystemInfo item8 in list)
				{
					blder.AppendFilePath(item8.FullName);
				}
			});
			runTestsRwAndRom("Stream -- Read -- Length", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenRead(PathPresent());
				blder.Append(stream.Length.ToString());
			});
			runTestsRwAndRom("Stream -- Read -- All", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenRead(PathPresent());
				byte[] array = new byte[stream.Length];
				blder.Append(stream.Read(array, 0, array.Length).ToString());
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- with Offset ", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenRead(PathPresent());
				byte[] array = new byte[4];
				blder.Append(stream.Read(array, 1, 2).ToString());
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- with Seek & Offset", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenRead(PathPresent());
				byte[] array = new byte[10];
				stream.Seek(1L, SeekOrigin.Begin);
				stream.Read(array, 0, 2);
				stream.Read(array, 2, 2);
				stream.Seek(-4L, SeekOrigin.End);
				stream.Read(array, 6, 2);
				stream.Seek(-8L, SeekOrigin.Current);
				stream.Read(array, 8, 2);
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- from stream end", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenRead(PathPresent());
				stream.Seek(0L, SeekOrigin.End);
				byte[] array = new byte[1];
				blder.Append(stream.Read(array, 0, 1).ToString());
				blder.Append(array);
			});
			runTestsRwAndRom("Stream -- Read -- Missing", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					using (File.OpenRead(PathMissing()))
					{
					}
				});
			});
			runTestRW("Stream -- Write -- Open Missing", (TestBuilder blder) =>
			{
				using (File.OpenWrite(PathAvailableByte()))
				{
				}
			});
			runTestRW("Stream -- Write -- Open & Flush Empty", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenWrite(PathAvailableByte());
				stream.Flush();
			});
			runTestRW("Stream -- Write -- Byte", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenWrite(PathAvailableByte());
				stream.WriteByte(30);
				stream.Flush();
			});
			runTestRW("Stream --  Write -- Buffer Offset & Length", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenWrite(PathAvailableByte());
				stream.Write(byteContent, 4, byteContent.Length - 8);
				stream.Flush();
			});
			runTestRW("Stream --  Write -- Buffer Offset & Length Seek", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenWrite(PathAvailableByte());
				stream.WriteByte(5);
				stream.WriteByte(3);
				stream.WriteByte(8);
				stream.Seek(1L, SeekOrigin.Begin);
				stream.WriteByte(9);
				stream.Flush();
			});
			runTestRW("Stream -- Write -- Open Existing Prepend & Append", (TestBuilder blder) =>
			{
				using Stream stream = File.OpenWrite(PathPresent());
				stream.WriteByte(9);
				stream.WriteByte(9);
				stream.Seek(0L, SeekOrigin.End);
				stream.WriteByte(9);
				stream.WriteByte(9);
				stream.Flush();
			});
			runTestRW("File -- AppendAllText", (TestBuilder blder) =>
			{
				File.AppendAllText(PathPresent(), "appendText");
			});
			runTestRW("File -- AppendAllTextMissing", (TestBuilder blder) =>
			{
				SerializeAnyThrows(blder, careAboutExceptionType: true, () =>
				{
					File.AppendAllText(PathMissing(), "appendText");
				});
			});
			return snapshot;
		}
		finally
		{
			((IDisposable)onScopeExit/*cast due to constrained. prefix*/).Dispose();
		}
		static void deleteTestDirectory(string name)
		{
			try
			{
				if (Directory.Exists(PathRootDirectory()))
				{
					Directory.Delete(PathRootDirectory());
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Could not delete test directory for test \"{name}\"\n. {arg}");
			}
		}
		static void getDirectoryStateRecursive(string currentDirPath, TestBuilder blder, int depth)
		{
			foreach (string item9 in sortPaths(Directory.EnumerateFiles(currentDirPath)))
			{
				if (!(Path.GetExtension(item9) == ".meta"))
				{
					appendFileHierarchyMarkers(">", depth);
					blder.AppendFilePath(item9, addLineBreakAtEnd: false);
					appendFileHierarchyMarkers("c", depth + 1);
					blder.Append(File.ReadAllText(item9));
				}
			}
			List<string> list = Directory.EnumerateDirectories(currentDirPath).ToList();
			list.Sort(StringComparer.OrdinalIgnoreCase);
			foreach (string item10 in list)
			{
				appendFileHierarchyMarkers("[", depth);
				blder.AppendFilePath(item10, addLineBreakAtEnd: false);
				getDirectoryStateRecursive(item10, blder, depth + 1);
			}
			void appendFileHierarchyMarkers(string arrowHead, int num)
			{
				blder.Append("\n");
				for (int i = 0; i < num; i++)
				{
					blder.Append("-");
				}
				blder.Append(arrowHead);
				blder.Append(" ");
			}
		}
		static void resetTestDirectory(string testName)
		{
			try
			{
				deleteTestDirectory(testName);
				Directory.CreateDirectory(PathRootDirectory());
				File.WriteAllText(PathPresent(), "present-path-string-content1234567*7d7^&*^\\)@$%^");
				string path = GetPath(Path.Combine("SubDirectory1", "SubDirectory2", "SubDirectory3", "SubDirectory4"));
				Directory.CreateDirectory(path);
				File.WriteAllText(Path.Combine(path, "subFile1.txt"), "present-path-string-content1234567*7d7^&*^\\)@$%^");
				File.WriteAllText(Path.Combine(path, "subFile2.text"), "present-path-string-content1234567*7d7^&*^\\)@$%^");
				File.WriteAllText(Path.Combine(path, "subFile3.note"), "present-path-string-content1234567*7d7^&*^\\)@$%^");
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("Could not {0}. So tests will fail. {1}", "resetTestDirectory", arg));
			}
		}
		void runTestROM(string testName, Action<TestBuilder> test)
		{
			useRomPaths = true;
			TestBuilder testBuilder = new TestBuilder();
			try
			{
				testBuilder.Append("Manual Hashes:\n");
				test(testBuilder);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(string.Format("test {0} threw an exception. This likely wasn't expected as the call wasn't wrapped in a {1}.\nMessage:\n{2}", testName, "SerializeAnyThrows", ex));
				testBuilder.Append(ex.GetType().FullName);
			}
			testBuilder.Append("\nDirectory State:");
			serializeDirectoryState(testBuilder);
			snapshot.tests.Add(testName, testBuilder.Serialize());
			useRomPaths = false;
		}
		void runTestRW(string testName, Action<TestBuilder> test)
		{
			useRomPaths = false;
			resetTestDirectory(testName);
			TestBuilder testBuilder = new TestBuilder();
			try
			{
				testBuilder.Append("Manual Hashes:\n");
				test(testBuilder);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(string.Format("test {0} threw an exception. This likely wasn't expected as the call wasn't wrapped in a {1}.\nMessage:\n{2}", testName, "SerializeAnyThrows", ex));
				testBuilder.Append(ex.GetType().FullName);
			}
			testBuilder.Append("\nDirectory State:");
			serializeDirectoryState(testBuilder);
			snapshot.tests.Add(testName, testBuilder.Serialize());
		}
		void runTestsRwAndRom(string testName, Action<TestBuilder> test)
		{
			runTestRW(testName + " -- RW", test);
			runTestROM(testName + " -- ROM", test);
		}
		static void serializeDirectoryState(TestBuilder blder)
		{
			if (!Directory.Exists(PathRootDirectory()))
			{
				blder.Append("Test folder is missing");
			}
			else
			{
				getDirectoryStateRecursive(PathRootDirectory(), blder, 1);
			}
		}
		static void serializeFileSystemInfo(FileSystemInfo info, TestBuilder blder)
		{
			blder.AppendFilePath(info.FullName ?? "");
			blder.Append(info.Name ?? "");
			blder.Append($"{info.Exists}");
			blder.Append(info.Extension ?? "");
			blder.Append($"{info.Attributes.HasFlag(FileAttributes.Directory)}");
			blder.Append($"{info.Attributes.HasFlag(FileAttributes.Hidden)}");
		}
		static List<string> sortPaths(IEnumerable<string> paths)
		{
			List<string> list = paths.ToList();
			list.Sort(StringComparer.OrdinalIgnoreCase);
			return list;
		}
	}

	private static string SanitizePath(string path)
	{
		return path.Replace("\\", "/");
	}

	public static bool GenerateTestSnapshot(TestDelegate test, out Exception exOut, out Snapshot snapshot)
	{
		exOut = null;
		try
		{
			snapshot = test();
		}
		catch (Exception ex)
		{
			Exception ex2 = (exOut = ex);
			snapshot = new Snapshot();
			Debug.LogError("Unexpected exception occured while testing, invalidating test results. Exception: " + ex2.ToString());
		}
		return exOut == null;
	}

	private static string PathRootDirectory()
	{
		return GetPath(null);
	}

	private static string GetPath(string path)
	{
		string text = "IOParityTesting";
		if (path != null)
		{
			text = Path.Combine(text, path);
		}
		if (useRomPaths)
		{
			text = Path.Combine(Application.streamingAssetsPath, text);
		}
		return text;
	}

	private static string PathPresent()
	{
		return GetPath("AlwaysPresentFile.asset");
	}

	private static string PathMissing()
	{
		return GetPath("NeverPresentFilePath.asset");
	}

	private static string PathInvalid()
	{
		return GetPath("fqw\\e/&/(\\0.a.((sset");
	}

	private static string PathParentMissing()
	{
		return GetPath(Path.Combine("NeverPresentFolder", "NeverPresentChildFilePath.asset"));
	}

	private static string PathAvailableText()
	{
		return GetPath("FileIOTest.txt");
	}

	private static string PathAvailableByte()
	{
		return GetPath("FileIOTest.bytes");
	}

	private static string PathAvailableFolder()
	{
		return GetPath("AvailableFolder");
	}

	private static string PathAvailableFolderNoParent()
	{
		return GetPath(Path.Combine("AvailableFolderParent", "AvailableFolderChild"));
	}

	private static void SerializeAnyThrows(TestBuilder blder, bool careAboutExceptionType, Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			if (careAboutExceptionType)
			{
				blder.Append(ex.GetType().FullName);
			}
			else
			{
				blder.Append(typeof(Exception).FullName);
			}
		}
	}
}
