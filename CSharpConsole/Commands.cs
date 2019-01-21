using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpConsole {
  public static class Commands {
    public static void Exec(string input) {
      string command = input;

      if (command == "help") {
        // Show interactive interface help

        Console.WriteLine("Evaluation;");
        Console.WriteLine("  e (code)           -- Evaluate statement and show output");
        Console.WriteLine("  (code)             -- Execute code as written");
        Console.WriteLine("  time (code)        -- Execute code and output run duration");
        Console.WriteLine("  whileinput (code)  -- Execute code in a while loop, taking input");
        Console.WriteLine("                        from the console to the variable 'input'");
        Console.WriteLine("                        until input is blank");
        Console.WriteLine("  verbose (on|off)   -- Enable/disable verbose mode");
        Console.WriteLine("  typeinfo (on|off)  -- Enable/disable show type info");
        Console.WriteLine("");
        Console.WriteLine("Program;");
        Console.WriteLine("  (line-num) (line)  -- Set program line at line-num");
        Console.WriteLine("  list               -- List current program with line numbers");
        Console.WriteLine("  clear              -- Clear current program");
        Console.WriteLine("  run                -- Run current program");
        Console.WriteLine("");
        Console.WriteLine("Settings;");
        Console.WriteLine("  framework (version)  -- Compile using specified framework version (defaults to v4.0)");
        Console.WriteLine("  using (namespace)    -- Add namespace to using list");
        Console.WriteLine("  listusing            -- List currently used namespaces");
        Console.WriteLine("  clearusing           -- Clear current using list (resets to using System)");
        Console.WriteLine("  ref (assembly)       -- Add assembly reference");
        Console.WriteLine("  listref              -- List currently referenced assemblies");
        Console.WriteLine("  clearref             -- Clear current reference list (resets to referencing System.dll)");
      } else if (command.StartsWith("verbose")) {
        // Switch verbose mode

        string value = command.Substring(7).Trim().ToLower();

        State.Verbose = IsYesString(value);
      } else if (command.StartsWith("typeinfo")) {
        // Switch show type info

        string value = command.Substring(8).Trim().ToLower();

        State.ShowTypeInfo = IsYesString(value);
      } else if (command == "list") {
        // List line editor program

        foreach (KeyValuePair<int, string> line in State.ProgramSource)
          Console.WriteLine(string.Format("{0}\t{1}", line.Key, line.Value));
      } else if (command == "clear") {
        // Clear line editor program

        Console.Write("Clear program [yN]? ");
        string response = Console.ReadLine();

        if (response.ToLower() == "y") {
          State.ProgramSource.Clear();
          Console.WriteLine("Program Cleared");
        }
      } else if (command == "run") {
        // Run line editor program

        string programStr = "";

        foreach (KeyValuePair<int, string> line in State.ProgramSource)
          programStr += $"{line.Value}\n";

        Compiler.ExecCSharp(programStr);
      } else if (command.StartsWith("framework ")) {
        // Switch framework version

        string newVersion = command.Substring(command.IndexOf(' ') + 1);

        Console.Write($"Reset compiler settings and switch to framework {newVersion} [yN]? ");
        string response = Console.ReadLine();

        if (response.ToLower() == "y") {
          State.SetFrameworkVersion(newVersion);
          Console.WriteLine($"Compiling using .Net Framework {newVersion}");
        }
      } else if (command.StartsWith("using ")) {
        // Add using clause

        string usingEntry = command.Substring(command.IndexOf(' ') + 1);
        State.ProgramUsing.Add(usingEntry);
        Console.WriteLine($"Using {usingEntry}");
      } else if (command == "listusing") {
        // Show list of used namespaces

        foreach (string usingItem in State.ProgramUsing)
          Console.WriteLine(usingItem);
      } else if (command == "clearusing") {
        // Clear used namespace, reset to just System

        Console.Write("Clear using [yN]? ");
        string response = Console.ReadLine();

        if (response.ToLower() == "y") {
          State.ProgramUsing.Clear();
          State.ProgramUsing.Add("System");
          Console.WriteLine("Using Cleared");
        }
      } else if (command.StartsWith("ref ")) {
        // Add reference to assembly

        string refEntry = command.Substring(command.IndexOf(' ') + 1);
        State.ProgramReferences.Add(refEntry);
        Console.WriteLine($"Referenced {refEntry}");
      } else if (command == "listref") {
        // List referenced assemblies

        foreach (string refItem in State.ProgramReferences)
          Console.WriteLine(refItem);
      } else if (command == "clearref") {
        // Clear referenced assemblies, reset to just System.dll

        Console.Write("Clear ref [yN]? ");
        string response = Console.ReadLine();

        if (response.ToLower() == "y") {
          State.ProgramReferences.Clear();
          State.ProgramReferences.Add("System.dll");
          Console.WriteLine("References Cleared");
        }
      } else if (command.StartsWith("time ")) {
        // Show duration to execute of specified expression

        DateTime start = DateTime.Now;

        string programStr = command.Substring(5);
        Compiler.ExecCSharp(programStr);

        Console.WriteLine($"Time {DateTime.Now - start}");
      } else if (command.StartsWith("whileinput ")) {
        // Begin a while loop that takes input and runs the input through the specified expression

        DateTime start = DateTime.Now;

        string programStr = command.Substring(11);
        programStr = $"string input; Console.Write(\"while> \"); while ((input = Console.ReadLine()) != \"\") {{ {programStr}; Console.Write(\"while> \"); }}";
        Compiler.ExecCSharp(programStr);
      } else if (command == "exit") {
        Program.Exited = true;
      } else {
        Compiler.ExecCSharp(command);
      }
    }

    private static bool IsYesString(string value) {
      return value == "yes" || value == "true" || value == "on" || value == "1";
    }
  }
}
