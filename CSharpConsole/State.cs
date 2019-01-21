using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpConsole {
  public static class State {
    public static bool Verbose = false;
    public static bool ShowTypeInfo = false;

    public static string FrameworkVersion = "v4.0";

    public static List<string> ProgramUsing = new List<string>();
    public static List<string> ProgramReferences = new List<string>();

    public static SortedList<int, string> ProgramSource = new SortedList<int, string>();

    public static string ProcessArgs(string[] args) {
      for (int i = 0; i < args.Length; i++) {
        if (!args[i].StartsWith("-"))
          return string.Join(" ", args, i, args.Length - i);

        if (args[i].Length > 0) {
          if (args[i].Length > 1 && args[i][0] == '-' && args[i][1] != '-') {
            for (int j = 1; j < args[i].Length; j++) {
              switch (args[i][j]) {
                case 'v': {
                  Verbose = true;
                  break;
                }
                case 't': {
                  ShowTypeInfo = true;
                  break;
                }
              }
            }
          } else {
            switch (args[i]) {
              case "--version": {
                Console.WriteLine($"C# Console {Program.ProgramVersion}");
                Console.WriteLine("by Luke Arnold");
                Console.WriteLine("https://github.com/jiyuuma/csharp-console");
                Program.Exited = true;
                return null;
              }
              case "--help": {
                Console.WriteLine($"{Path.GetFileName(Program.ProgramPath)} [options] [c#-expression]");
                Console.WriteLine("  --help       Display help");
                Console.WriteLine("  --version    Display version");
                Console.WriteLine("  -v           Enable verbose mode");
                Console.WriteLine("  -t           Show Type information");
                Console.WriteLine("  -F|--framework (version)    Use framework version (2.0, 3.5, 4.0)");
                Console.WriteLine("  -R|--ref (ref)              Reference assembly (ref)");
                Console.WriteLine("  -U|--using (using)          Add using section for namespace (using)");

                Program.Exited = true;
                return null;
              }
              case "-F":
              case "--framework": {
                SetFrameworkVersion(args[++i]);
                break;
              }
              case "-R":
              case "--ref": {
                ProgramReferences.Add(args[++i]);
                break;
              }
              case "-U":
              case "--using": {
                ProgramUsing.Add(args[++i]);
                break;
              }
            }
          }
        }
      }

      return null;
    }

    public static void Defaults() {
      SetFrameworkVersion("v4.0");
      ProgramSource.Clear();
    }

    public static void SetFrameworkVersion(string version) {
      switch (version) {
        case "2":
        case "2.0":
        case "v2.0": {
          FrameworkVersion = version;

          ProgramUsing.Clear();
          ProgramUsing.Add("System");
          ProgramUsing.Add("System.IO");
          ProgramUsing.Add("System.Net");
          ProgramUsing.Add("System.Collections.Generic");

          ProgramReferences.Clear();
          ProgramReferences.Add("System.dll");
          break;
        }
        case "3":
        case "3.5":
        case "v3.5": {
          FrameworkVersion = version;

          ProgramUsing.Clear();
          ProgramUsing.Add("System");
          ProgramUsing.Add("System.IO");
          ProgramUsing.Add("System.Net");
          ProgramUsing.Add("System.Collections.Generic");

          ProgramReferences.Clear();
          ProgramReferences.Add("System.dll");
          break;
        }
        case "4":
        case "4.0":
        case "v4.0":
        case "4.6":
        case "v4.6": {
          FrameworkVersion = version;

          ProgramUsing.Clear();
          ProgramUsing.Add("System");
          ProgramUsing.Add("System.IO");
          ProgramUsing.Add("System.Net");
          ProgramUsing.Add("System.Collections.Generic");

          ProgramReferences.Clear();
          ProgramReferences.Add("System.dll");
          ProgramReferences.Add("System.Collections.dll");
          break;
        }
        default: {
          throw new Exception($"Framework version '{version}' not known.");
        }
      }
    }
  }
}
