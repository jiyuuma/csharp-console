using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSharpConsole {
  class Program {
    public static bool Exited = false;

    public static string ProgramVersion;
    public static string ProgramPath;

    static int Main(string[] args) {
      int exitcode = 0;

      // Get program information
      ProgramPath = typeof(Program).Assembly.Location;
      var version = FileVersionInfo.GetVersionInfo(ProgramPath);
      ProgramVersion = $"{version.FileMajorPart}.{version.FileMinorPart}.{version.FileBuildPart}";

      // Load defaults
      State.Defaults();

      string argCommand = State.ProcessArgs(args);

      if (argCommand != null) {
        // Expression passed on command line, execute it and exit

        try {
          Commands.Exec(argCommand);
        } catch {
          exitcode = 1;
        }
      } else {
        // Begin interactive console
        State.Verbose = true;

        string command = "";

        while (!Exited) {
          Console.Write("# ");
          string input = Console.ReadLine();

          command += input;

          try {
            if (input != "") {
              if (char.IsDigit(input.Trim()[0])) {
                // Set line in line editor

                string lineNumStr = input.Trim();
                string lineContent = input.Substring(input.IndexOf(' ') + 1);
                lineNumStr = input.Substring(0, input.IndexOf(' '));
                int lineNum = int.Parse(lineNumStr);

                State.ProgramSource[lineNum] = lineContent;

                command = "";
              } else {
                if (!input.EndsWith("\\")) {
                  Commands.Exec(command);
                  command = "";
                } else {
                  command = command.Trim('\\');
                }
              }
            }
          } catch (Exception ex) {
            Console.WriteLine($"Error: {ex}");
            command = "";
          }
        }
      }

      return exitcode;
    }
  }
}
