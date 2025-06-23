using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Stonefreak.Tooling
{
    public enum Accessor : byte { Private, Protected, Internal, Public }

    /// <summary>
    /// This class acts as a View, making the user of the Code Generator able to access its logic via a custom Editor Window. 
    /// Default way to open the Window is through 'Tools/Stonefreak/UITK Code Generator'.
    /// </summary>
    public class UIToolkitCodeGeneratorView : EditorWindow
    {
        private ObjectField _documentInput = null;
        private TextField _classNameInput = null;
        private Label _filePathDisplay = null;
        private EnumField _accessorInput = null;
        private Toggle _shouldFilterByStyleToggle = null;
        private TextField _filterStyleInput = null;

        [SerializeField] private VisualTreeAsset _selectedDocument = null;
        [SerializeField] private string _selectedName = string.Empty;
        [SerializeField] private string _selectedDir = string.Empty;
        [SerializeField] private Accessor _selectedAccessor = (Accessor)0;
        [SerializeField] private string _selectedFilter = string.Empty;

        #region UI
        [MenuItem("Tools/Stonefreak/UITK Code Generator")]
        public static void CreateWindow()
        {
            var wnd = GetWindow<UIToolkitCodeGeneratorView>();
            wnd.titleContent = new GUIContent("UITK Code Generator");
            wnd.minSize = new Vector2(300, 200);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;

            // Document Object Field
            _documentInput = new("Document")
            { objectType = typeof(VisualTreeAsset) };
            _documentInput.tooltip = "The Document (UXML File) to generate code for.";
            _documentInput.value = _selectedDocument;
            root.Add(_documentInput);

            AddVerticalSpacer(ref root);

            // Class Name (& File Name) Input
            _classNameInput = new("Class Name");
            _classNameInput.tooltip = "The name of the class that is going to be generated. (This will also be used as part of the filename)";
            _classNameInput.value = _selectedName;
            root.Add(_classNameInput);

            // Directory Selction
            AddDirectoryRow(ref root, OnDirectoryBtnClicked);

            AddVerticalSpacer(ref root);

            // Accessor Dropdown
            _accessorInput = new("Accessor", Accessor.Protected);
            _accessorInput.tooltip = "What Accessor the references to the Visual Elements should have";
            _accessorInput.value = _selectedAccessor;
            root.Add(_accessorInput);

            // Style Filter Toggle
            _shouldFilterByStyleToggle = new("Filter by Style");
            _shouldFilterByStyleToggle.tooltip = "Whether only elements with a specific style should be added to the code.";
            _shouldFilterByStyleToggle.value = !string.IsNullOrWhiteSpace(_selectedFilter);
            root.RegisterCallback<ChangeEvent<bool>>(OnFilterStyleToggled);
            root.Add(_shouldFilterByStyleToggle);

            // Style Filter input
            _filterStyleInput = new("Filter name");
            _filterStyleInput.tooltip = "The Style to filter for when generating.";
            _filterStyleInput.value = _selectedFilter;
            _filterStyleInput.style.display = _shouldFilterByStyleToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            root.Add(_filterStyleInput);

            AddVerticalSpacer(ref root, -1);

            // Generate Button
            var generateBtn = new Button(OnGenerateBtnClicked)
            { text = "Generate" };
            generateBtn.tooltip = "Tries to generate and save the final code file with the specified parameters";
            root.Add(generateBtn);
        }

        private void AddVerticalSpacer(ref VisualElement parent, float verticalSize = 10)
        {
            if (verticalSize < 0)
            {
                var spacer = new VisualElement();
                spacer.style.width = Length.Percent(100);
                spacer.style.flexGrow = 1;
                parent.Add(spacer);
            }
            else
            {
                var spacer = new VisualElement();
                spacer.style.width = Length.Percent(100);
                spacer.style.height = verticalSize;
                parent.Add(spacer);
            }
        }

        private void AddDirectoryRow(ref VisualElement parent, System.Action onDirectoryBtnClicked)
        {
            // Root container (horizontal layout)
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center; // Optional for vertical alignment
            row.style.marginLeft = 3;
            parent.Add(row);

            // Label with fixed width
            Label label = new("File Directory");
            label.tooltip = "The folder the generated file should be saved in.";
            label.style.width = 120;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.overflow = Overflow.Hidden;
            label.style.flexShrink = 0;
            label.style.paddingBottom = 0;  // copied from default Unity Styling
            label.style.paddingLeft = 1;    // copied from default Unity Styling
            label.style.paddingRight = 2;   // copied from default Unity Styling
            label.style.paddingTop = 2;     // copied from default Unity Styling
            row.Add(label);

            // Button aligned right
            Button dirBtn = new(onDirectoryBtnClicked);
            dirBtn.text = "...";
            dirBtn.style.flexShrink = 0;
            row.Add(dirBtn);

            // Middle content (Label or TextField depending on usage)
            _filePathDisplay = new Label(_selectedDir.Replace(Application.dataPath, "*"));
            _filePathDisplay.style.flexGrow = 1;
            _filePathDisplay.style.paddingLeft = 3;
            row.Add(_filePathDisplay);
        }
        #endregion

        private void ExtractValues()
        {
            _selectedDocument = _documentInput.value as VisualTreeAsset;
            _selectedName = _classNameInput.value;
            _selectedAccessor = (Accessor)_accessorInput.value;
            _selectedFilter = _shouldFilterByStyleToggle.value ? _filterStyleInput.value : string.Empty;
        }
        private void OnDisable() => ExtractValues();

        #region Event Listener
        private void OnDirectoryBtnClicked()
        {
            var newDir = EditorUtility.OpenFolderPanel("File Directory", _selectedDir, string.Empty);
            if (!string.IsNullOrEmpty(newDir)) _selectedDir = newDir;

            _filePathDisplay.text = _selectedDir.Replace(Application.dataPath, "*");
        }

        private void OnGenerateBtnClicked()
        {
            ExtractValues();
            UIToolkitCodeGenerator.Generate(_selectedDocument, _selectedName, _selectedDir, _selectedAccessor, _selectedFilter);
        }

        private void OnFilterStyleToggled(ChangeEvent<bool> e) => _filterStyleInput.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        #endregion
    }
}