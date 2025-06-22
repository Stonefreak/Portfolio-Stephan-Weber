using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stonefreak.Tooling
{
    public class UIToolkitCodeGenerator : EditorWindow
    {
        public static bool Generate(VisualTreeAsset doc, string className, string fileDir, Accessor accessor, string styleFilter = "")
        {
            // Error Checks
            if (doc == null)
            {
                Debug.LogError("Code Generation <color=red>failed</color> -> There was no Document selected.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(className) || className.Contains(' '))
            {
                Debug.LogError("Code Generation <color=red>failed</color> -> The entered Class Name was empty or contains spaces");
                return false;
            }
            if (string.IsNullOrWhiteSpace(fileDir) || !Directory.Exists(fileDir))
            {
                Debug.LogError("Code Generation <color=red>failed</color> -> The entered Directory does not exist");
                return false;
            }

            // Start Stopwatch
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Generate and Save Code
            var elements = ExtractUIElements(doc, styleFilter);
            var code = GenerateCodeFile(elements, className, accessor);
            try
            {
                SaveCodeAsFile(code, className, fileDir);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Code Generation <color=red>failed</color> -> '{e}'");
                return false;
            }

            // Finish up + user feedback
            var generationTime = watch.ElapsedMilliseconds;
            AssetDatabase.Refresh();
            watch.Stop();
            Debug.Log($"Code Generation <color=green>completed</color> in {generationTime / 1000f:0.00}s (Refreshed AssetDatabase for {(watch.ElapsedMilliseconds - generationTime) / 1000f:0.00}s)");
            return true;
        }

        private static List<VisualElement> ExtractUIElements(VisualTreeAsset document, string styleFilter)
        {
            var elements = new List<VisualElement>();
            var prevNames = new HashSet<string>();

            var root = document.Instantiate();
            ExtractUIElementsRecursive(root, styleFilter, ref elements, ref prevNames);

            return elements;
        }

        private static void ExtractUIElementsRecursive(VisualElement element, string styleFilter, ref List<VisualElement> elements, ref HashSet<string> prevNames)
        {
            if (element == null) return;
            if (element.name.Contains("unity")) return;

            var hasName = !string.IsNullOrWhiteSpace(element.name);
            var hasFilter = string.IsNullOrWhiteSpace(styleFilter) || element.ClassListContains(styleFilter);

            if (hasName && hasFilter)
            {
                if (prevNames.Add(element.name)) elements.Add(element);
                else Debug.LogWarning($"One Element was skipped because of a repeating name: (name: '{element.name}'; type: '{element.GetType().FullName}')");
            }

            foreach (var child in element.Children())
            {
                ExtractUIElementsRecursive(child, styleFilter, ref elements, ref prevNames);
            }
        }

        private static string GenerateCodeFile(List<VisualElement> elements, string className, Accessor accessor)
        {
            var code = new StringBuilder();

            // using Directives
            code.AppendLine("using UnityEngine;");
            code.AppendLine("using UnityEngine.UIElements;");
            code.AppendLine();

            // Open Class
            code.AppendLine($"[UnityEngine.RequireComponent(typeof(UIDocument))]");
            code.AppendLine($"public partial class {className} : MonoBehaviour");
            code.AppendLine("{");

            // UI Element Variables
            foreach (var elem in elements)
            {
                var acc = accessor.ToString().ToLower();
                var type = elem.GetType().FullName.Replace('+', '.');
                var name = elem.name.Replace('-', '_');

                code.AppendLine($"\t{acc} {type} {name} = null;");
            }
            code.AppendLine();

            // UI Element Initialize Function
            code.AppendLine("\t/// <summary>");
            code.AppendLine("\t/// This sets the references to the Generated variables.<br/>");
            code.AppendLine("\t/// It is recommended to place call it at the beginning of the Awake or Start Method.");
            code.AppendLine("\t/// </summary>");
            code.AppendLine("\tprotected void InitComponents()");
            code.AppendLine("\t{");
            code.AppendLine("\t\tvar root = GetComponent<UIDocument>().rootVisualElement;");
            code.AppendLine();

            foreach (var elem in elements)
            {
                if (string.IsNullOrWhiteSpace(elem.name)) continue;

                var name = elem.name.Replace('-', '_');
                var type = elem.GetType().FullName.Replace('+', '.');

                code.AppendLine($"\t\t{name} = root.Q<{type}>(\"{elem.name}\");");
            }

            // Close Class
            code.AppendLine("\t}");
            code.AppendLine("}");

            return code.ToString();
        }

        private static bool SaveCodeAsFile(string code, string fileName, string directory)
        {
            var filePath = $"{directory}/{fileName}.generated.cs";

            if (!Directory.Exists(directory)) return false;

            File.WriteAllText(filePath, code);
            return true;
        }
    }
}