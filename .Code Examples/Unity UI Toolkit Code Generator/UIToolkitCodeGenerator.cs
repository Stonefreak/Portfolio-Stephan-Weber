using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stonefreak.Tooling
{
    /// <summary>
    /// This class provides a static function 'Generate' to produce and save a partial class 
    /// taking care of managing references to VisualElements from a Unity UI Toolkit Document
    /// </summary>
    public class UIToolkitCodeGenerator : EditorWindow
    {
        /// <summary>
        /// Generates, saves and lets Unity compile a partial class managing the querrying of 
        /// VisualElements filtered by the existance of a name and optionally by a specified style/class.
        /// </summary>
        /// <param name="doc">The UXML File to extract the data from.</param>
        /// <param name="className">The name of the partial class. Also used for a part of the file name.</param>
        /// <param name="fileDir">The directory the file should be saved in.</param>
        /// <param name="accessor">The accessor every reference to the VisualElements should have.<br/>'Protected' is recommended.</param>
        /// <param name="styleFilter">The style to filter for. Leave empty or whitespace to ignore.</param>
        /// <returns>Whether the operation could be completed successfully or not.</returns>
        public static bool Generate(VisualTreeAsset doc, string className, string fileDir, Accessor accessor = Accessor.Protected, string styleFilter = "")
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

            // Extract VisualElements
            var rElements = ExtractUIElements(doc, styleFilter);
            if (!rElements.success)
            {
                watch.Stop();
                var time = watch.ElapsedMilliseconds / 1000f;

                Debug.LogError($"Extraction of Visual Elements <color=red>failed</color> after {time:0.00}s -> The entered Directory does not exist");

                return false;
            }

            // Generate Code
            var rCode = GenerateCode(rElements.elements, className, accessor);
            if (!rCode.success)
            {
                watch.Stop();
                var time = watch.ElapsedMilliseconds / 1000f;

                Debug.LogError($"Code Generation <color=red>failed</color> after {time:0.00}s -> The entered Directory does not exist");

                return false;
            }

            // Save Code to file
            try
            {
                var result = SaveCodeAsFile(rCode.code, className, fileDir);
                if (result == false) throw new System.Exception();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Code Saving <color=red>failed</color> -> '{e}'");
                return false;
            }

            // Finish up + user feedback
            var generationTime = watch.ElapsedMilliseconds;
            AssetDatabase.Refresh();
            watch.Stop();
            Debug.Log($"Code Generation <color=green>completed</color> in {generationTime / 1000f:0.00}s (Refreshed AssetDatabase for {(watch.ElapsedMilliseconds - generationTime) / 1000f:0.00}s)");
            return true;
        }

        #region Element Extraction
        private static (bool success, List<VisualElement> elements) ExtractUIElements(VisualTreeAsset document, string styleFilter)
        {
            var elements = new List<VisualElement>();
            var prevNames = new HashSet<string>();

            var root = document.Instantiate();
            ExtractUIElementsRecursive(root, styleFilter, ref elements, ref prevNames);

            return (true, elements);
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
        #endregion

        #region CodeGen and IO
        private static (bool success, string code) GenerateCode(List<VisualElement> elements, string className, Accessor accessor)
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

            return (true, code.ToString());
        }

        private static bool SaveCodeAsFile(string code, string fileName, string directory)
        {
            var filePath = $"{directory}/{fileName}.generated.cs";

            if (!Directory.Exists(directory)) return false;

            File.WriteAllText(filePath, code);
            return true;
        }
        #endregion
    }
}