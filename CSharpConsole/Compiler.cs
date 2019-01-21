using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace CSharpConsole {
  public class Compiler {
    public static void ExecCSharp(string input) {
      try {
        // Default to .Net Framework 4.0
        if (State.FrameworkVersion == null || State.FrameworkVersion == "")
          State.FrameworkVersion = "v4.0";

        if (input.StartsWith("e ")) {
          // Add return code if the `e` command is used to evaluate an expression.
          input = $"object __retobj = ({input.Substring(2)}); return __retobj;";
        }

        // Prepare compiler
        Dictionary<string, string> providerOptions = new Dictionary<string, string>();
        providerOptions.Add("CompilerVersion", State.FrameworkVersion);

        CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

        CompilerParameters param = new CompilerParameters();
        param.GenerateInMemory = true;
        param.IncludeDebugInformation = true;
        param.CompilerOptions = "/nostdlib";

        // Prepare framework references
        string frameworkRefRoot = "";

        switch (State.FrameworkVersion) {
          case "v2.0":
          case "v3.5": {
            frameworkRefRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.Net", "Framework", "v2.0.50727");
            param.ReferencedAssemblies.Add(Path.Combine(frameworkRefRoot, "mscorlib.dll"));
            break;
          }
          case "v4.0": {
            frameworkRefRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.Net", "Framework", "v4.0.30319");
            param.ReferencedAssemblies.Add(Path.Combine(frameworkRefRoot, "mscorlib.dll"));
            break;
          }
          default: {
            throw new Exception(string.Format("Unknown framework version '{0}'. Supports 'v2.0' and 'v4.0'", State.FrameworkVersion));
          }
        }

        // Prepare user-specified referenced assemblies
        foreach (string refEntry in State.ProgramReferences)
          param.ReferencedAssemblies.Add(Path.IsPathRooted(refEntry) ? refEntry : Path.Combine(frameworkRefRoot, refEntry));

        // Generate code
        string genGuid = Guid.NewGuid().ToString("n");
        string genClass = $"class{genGuid}";
        string genFunc = $"func{genGuid}";

        if (!input.Contains("return "))
          input += "; return null;";

        string usingSection = "";
        foreach (string usingEntry in State.ProgramUsing)
          usingSection += $"using {usingEntry};\n";

        string program = $@"
{usingSection}

namespace cscongen {{
  public static class {genClass} {{
    public static object {genFunc}() {{
      {input};
    }}
  }}
}}
        ";

        // Compile code
        CompilerResults results = provider.CompileAssemblyFromSource(param, program);

        if (results.Errors.Count > 0) {
          // Code compilation failed
          StringBuilder errorBuilder = new StringBuilder();

          errorBuilder.AppendLine("COMPILE ERRORS");
          foreach (CompilerError error in results.Errors)
            errorBuilder.AppendLine(error.ToString());

          throw new Exception(errorBuilder.ToString());
        } else {
          // Invoke compiled method
          Type type = results.CompiledAssembly.GetType($"cscongen.{genClass}");
          MethodInfo method = type.GetMethod(genFunc, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

          object result = method.Invoke(null, new object[0]);

          if (result != null) {
            if (State.ShowTypeInfo)
              Console.WriteLine(result.GetType().ToString());

            Console.WriteLine(DisplayValue(result, 0));
          } else
            Console.WriteLine("null");
        }
      } catch (TargetInvocationException tie) {
        Console.WriteLine(tie.InnerException.ToString());
        throw tie;
      } catch (Exception ex) {
        Console.WriteLine(ex.ToString());
        throw ex;
      }
    }

    private static string DisplayValue(object val, int level) {
      if (val == null)
        return "null";

      StringBuilder output = new StringBuilder();

      if (val.GetType().IsArray) {
        // Array, display array elements
        Array array = (Array)val;

        output.Append("[");

        for (int i = 0; i < array.Length; i++) {
          output.Append(DisplayValue(array.GetValue(i), level + 1));

          if (i < array.Length - 1)
            output.Append(", ");
        }

        output.Append("]");
      } else if (val.GetType() == typeof(string)) {
        // String, display value in double quotes
        output.Append('\"');
        output.Append(val.ToString());
        output.Append('\"');
      } else if (!val.GetType().IsValueType) {
        // Complex object
        string toString = val.ToString();

        output.Append(val.GetType().ToString());
        output.Append('(');

        if (string.Compare(toString, val.GetType().ToString()) == 0 || level == 0) {
          // Object does not override ToString, Need to build object output by hand
          int numOut = 0;

          foreach (PropertyInfo prop in val.GetType().GetProperties()) {
            MethodInfo getMethod = prop.GetGetMethod();

            if (getMethod != null && getMethod.GetParameters().Length == 0) {
              output.Append(prop.Name);
              output.Append('=');
              output.Append(DisplayValue(prop.GetGetMethod().Invoke(val, new object[0]), level + 1));
              output.Append(", ");
              numOut++;
            }
          }

          foreach (FieldInfo field in val.GetType().GetFields()) {
            output.Append(field.Name);
            output.Append('=');
            output.Append(DisplayValue(field.GetValue(val), level + 1));
            output.Append(", ");
            numOut++;
          }

          if (numOut > 0)
            output.Length -= 2;
        } else {
          // Object has its own ToString implementation, display the result of that
          output.Append(toString);
        }

        output.Append(')');
      } else {
        // Display default ToString conversion of value
        output.Append(val.ToString());
      }

      return output.ToString();
    }
  }
}
