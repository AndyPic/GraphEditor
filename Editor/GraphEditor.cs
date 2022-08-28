using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphEditor : EditorWindow, ISerializationCallbackReceiver
{
    private GraphNodeStorage currentlyEditing;

    private A_GraphEditorView graphView;

    [SerializeField] private bool reloadNeeded;
    [SerializeField] private string serialisedGraphTypeName;
    [SerializeField] private GraphNodeStorage serialisedStorage;

    public void OnBeforeSerialize()
    {
        if (graphView == null || serialisedStorage == default || serialisedGraphTypeName == default)
        {
            serialisedStorage = default;
            serialisedGraphTypeName = default;
            reloadNeeded = false;
            return;
        }

        graphView.SaveGraph(serialisedStorage);
        serialisedGraphTypeName = graphView.GetType().AssemblyQualifiedName;
        reloadNeeded = true;
    }

    public void OnAfterDeserialize(){ }

    private void Reload()
    {
        // Load correct graph type
        ConstructGraphView(serialisedGraphTypeName);

        rootVisualElement.Remove(rootVisualElement.Q<TemplateContainer>());
        rootVisualElement.Q<Toolbar>().Q<ObjectField>().SetEnabled(true);

        // Load from storage
        graphView.LoadGraph(serialisedStorage);
    }

    [MenuItem("Tools/Dialogue Editor")]
    public static void CreateEditorWindow()
    {
        var window = GetWindow<GraphEditor>();
        window.titleContent = new GUIContent("Graph Editor");
    }

    private void OnEnable()
    {
        NewGraph();

        if (reloadNeeded)
            Reload();

        if (serialisedStorage == null)
            serialisedStorage = CreateInstance<GraphNodeStorage>();
    }

    private void NewGraph()
    {
        if (graphView != null)
        {
            // Ask to save
            if (currentlyEditing != null && SavePrompt())
                graphView.SaveGraph(currentlyEditing);

            // Reset everything
            graphView = null;
            currentlyEditing = null;
        }

        rootVisualElement.Clear();

        GenerateToolbars();
        GenerateGraphTypePicker();
    }

    private void GenerateToolbars()
    {
        var toolbar = new Toolbar();

        // Create the save button
        var saveButton = new Button(() => graphView.SaveGraph(currentlyEditing)) { text = "Save" };
        saveButton.SetEnabled(false);

        //loadSaveToolbar.Add(new Button(() => graphView.Load()) { text = "Load" });

        // Create field for the object to store the graph in
        var graphNodeStorage = new ObjectField("Currently Edditing:");
        graphNodeStorage.objectType = typeof(GraphNodeStorage);
        graphNodeStorage.SetEnabled(false);
        graphNodeStorage.RegisterValueChangedCallback(evt =>
        {
            // Pop up if want to save old one
            if (currentlyEditing != null && SavePrompt())
                graphView.SaveGraph(currentlyEditing);

            // TODO: Clear old graph out
            graphView.ResetGraph();

            // Guard clause if switching to none
            if (evt.newValue == null)
            {
                currentlyEditing = null;
                saveButton.SetEnabled(false);
                return;
            }

            // Update currently editting
            currentlyEditing = (GraphNodeStorage)evt.newValue;

            // Enable save button 
            saveButton.SetEnabled(true);

            // Load new asset
            graphView.LoadGraph(currentlyEditing);
        });

        var newGraphButton = new Button(() => NewGraph()) { text = "New Graph" };

        // Add the elements (In order)
        toolbar.Add(graphNodeStorage);
        toolbar.Add(saveButton);
        toolbar.Add(newGraphButton);

        rootVisualElement.Add(toolbar);
    }

    private void GenerateGraphTypePicker()
    {
        // Check in all assemblies (Cant be sure which assembly will be in due to assembly refs)
        var graphTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => p.BaseType == typeof(A_GraphEditorView)).ToList();

        var typePickerContainer = new TemplateContainer();

        rootVisualElement.Add(typePickerContainer);

        graphTypes.ForEach(graphType =>
        {
            var button = new Button(() =>
            {
                ConstructGraphView(graphType);
                rootVisualElement.Remove(typePickerContainer);
                rootVisualElement.Q<Toolbar>().Q<ObjectField>().SetEnabled(true);
            })
            { text = $"{graphType}" };

            typePickerContainer.Add(button);
        });
    }

    private void ConstructGraphView(Type type)
    {
        graphView = Activator.CreateInstance(type) as A_GraphEditorView;
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
        graphView.SendToBack();
    }

    private void ConstructGraphView(string stringType)
    {
        var type = Type.GetType(stringType);

        ConstructGraphView(type);
    }

    private bool SavePrompt()
    {
        return EditorUtility.DisplayDialog("Save", "Save current graph?", "Yes", "No");
    }
}