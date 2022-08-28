
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class DialogueEditor : A_GraphEditorView
{
    public DialogueEditor() : base() { }

    protected override void SetUpContextMenu()
    {
        //base.SetUpContextMenu();

        var addNodeMenu = new ContextualMenuManipulator(menuEvent =>
            menuEvent.menu.AppendAction("Add New Dialogue Node", actionEvent =>
                AddDialogueNode(), DropdownMenuAction.AlwaysEnabled));

        this.AddManipulator(addNodeMenu);
    }

    private void AddDialogueNode()
    {
        var newNode = CreateDefaultNode(position: MousePosition);

        newNode.title = "Dialogue Node";

        // Add text input
        var textField = new TextField("") { multiline = true };
        textField.RegisterValueChangedCallback(evt =>
        {
            newNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(newNode.title);
        newNode.mainContainer.Add(textField);

        AddInputPort(newNode);
        AddOutputPort(newNode);
        AddElement(newNode);
    }
}