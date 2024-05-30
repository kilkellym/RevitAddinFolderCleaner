// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Threading;


class Program
{
	static bool Del = false;

	static void Main(string[] args)
	{
		string timeString = DateTime.Now.ToString("MMddyyyy_HHmmss");
		string scriptName = Process.GetCurrentProcess().MainModule.FileName;
		string scriptPath = Path.GetDirectoryName(scriptName);
		string log = Path.Combine(Path.GetTempPath(), $"Log_{Path.GetFileName(scriptName)}_{timeString}.txt");

		using (StreamWriter logWriter = new StreamWriter(log))
		{
			logWriter.WriteLine("Transcript started at: " + DateTime.Now);
		}

		if (!Del)
		{
			TestMode();
		}

		string revitAddinsCleanUpFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RevitAddinsCleanUp");
		string exclusionsFile = Path.Combine(revitAddinsCleanUpFolder, "RevitAddinsCleanUpExclusions.csv");

		Console.Clear();

		if (!Directory.Exists(revitAddinsCleanUpFolder))
		{
			Directory.CreateDirectory(revitAddinsCleanUpFolder);
		}

		if (!File.Exists(exclusionsFile))
		{
			ClearConsole();
			GetAllUserProfileAddinAndDllFiles().ToList().ForEach(file => File.AppendAllText(exclusionsFile, $"{file}{Environment.NewLine}"));

			Console.WriteLine("No previous exclusions file found.\nCreating Exclusions File...\n");
			Console.WriteLine("Update the exclusions file, save it, and close it.\nOnly leave the rows/lines of the files you do not want to delete.");
			Console.WriteLine("Then, run the script again to delete the addins and DLLs removed from the csv.");
			Thread.Sleep(3000);

			Process.Start(new ProcessStartInfo
			{
				FileName = exclusionsFile,
				UseShellExecute = true
			});
		}
		else
		{
			AskToUpdateFile(exclusionsFile);

			var exclusionsFileImported = File.ReadAllLines(exclusionsFile).ToList();

			var filesToDelete = GetAllUserProfileAddinAndDllFiles().Except(exclusionsFileImported).ToList();

			if (Del)
			{
				filesToDelete.ForEach(file =>
				{
					try
					{
						File.Delete(file);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error deleting file {file}: {ex.Message}");
					}
				});
			}
			else
			{
				filesToDelete.ForEach(file =>
				{
					Console.WriteLine($"Would delete: {file}");
				});
			}

			Console.WriteLine("\nRemaining Addin and DLL files:");
			filesToDelete.ForEach(file => Console.WriteLine(file));
		}

		Console.WriteLine("\nCleanUp Script Completed.");
		Console.ReadLine();
	}

	static void TestMode()
	{
		string question = "Do you want to Run in TestMode?\nNo files will be deleted, instead it Will do 'What If': to show what would have been deleted.\n[y|n]";

		while (true)
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(question);
			Console.ResetColor();
			string test = Console.ReadLine();

			if (test == "y")
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Running in Test mode!!!");
				Console.ResetColor();
				Del = false;
				Thread.Sleep(2000);
				break;
			}
			else if (test == "n")
			{
				Del = true;
				Thread.Sleep(2000);
				break;
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"You entered: {test}\nYou must answer: y or n");
			Console.ResetColor();
		}
	}

	static void AskToUpdateFile(string exclusionsFile)
	{
		string question = "Do you want to update the Exclusions File? [y|n]";
		Console.WriteLine(question);
		string updateFileYesOrNo = Console.ReadLine();

		while (updateFileYesOrNo != "y" && updateFileYesOrNo != "n")
		{
			Console.Clear();
			if (updateFileYesOrNo == "y")
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Please update the Exclusions file.\nSave and close before continuing!!!");
				Console.ResetColor();
				Process.Start(exclusionsFile);
				Console.ReadLine();
				break;
			}
			else if (updateFileYesOrNo == "n")
			{
				break;
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"You entered: {updateFileYesOrNo}\nYou must answer: y or n");
			Console.ResetColor();
			Console.WriteLine(question);
			updateFileYesOrNo = Console.ReadLine();
		}
	}

	static IEnumerable<string> GetAllUserProfileAddinAndDllFiles()
	{
		var filesToDelete = new List<string>();

		string[] excludeList = Array.Empty<string>();

		string exclusionsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RevitAddinsCleanUp", "RevitAddinsCleanUpExclusions.csv");
		if (File.Exists(exclusionsFile))
		{
			excludeList = File.ReadAllLines(exclusionsFile);
		}

		string[] addinPaths = {
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\Revit\Addins\")
		};
		
		// use this version to include addins in the ProgramData folder
		//string[] addinPaths = {
		//	Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\Revit\Addins\"),
		//	@"C:\programdata\Autodesk\Revit\Addins\"
		//};

		foreach (var addinPath in addinPaths)
		{
			if (Directory.Exists(addinPath))
			{
				filesToDelete.AddRange(Directory.GetFiles(addinPath, "*.dll", SearchOption.AllDirectories).Except(excludeList));
				filesToDelete.AddRange(Directory.GetFiles(addinPath, "*.addin", SearchOption.AllDirectories).Except(excludeList));
			}
		}

		return filesToDelete;
	}

	static void ClearConsole()
	{
		Console.Clear();
	}
}

