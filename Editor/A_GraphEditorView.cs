using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// Base class for graph editor views
/// </summary>
public abstract class A_GraphEditorView : GraphView
{
    protected Vector2 MousePosition { get { return contentViewContainer.WorldToLocal(Mouse.current.position.ReadValue()); } }

    protected readonly Vector2 defaultNodeSize = new(250, 100);

    public A_GraphEditorView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("NarrativeGraph"));
        SetupZoom(0.1f, 2f);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());

        SetUpContextMenu();

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // Add the starting node
        AddStartNode();
    }

    /// <summary>
    /// Adds options to the (right-click) context menu of the graph view
    /// </summary>
    protected virtual void SetUpContextMenu()
    {
        var addNodeMenu = new ContextualMenuManipulator(menuEvent =>
            menuEvent.menu.AppendAction("Add New Node", actionEvent =>
                AddNewNode("New Node"), DropdownMenuAction.AlwaysEnabled));

        this.AddManipulator(addNodeMenu);
    }

    public void SaveGraph(GraphNodeStorage saveTarget)
    {
        List<NodeData> nodes = new();

        foreach (GraphNode node in this.nodes.Cast<GraphNode>())
        {
            var nodeGUID = node.GUID;

            var nodePosition = node.GetPosition().position;

            var outputPorts = node.outputContainer.Query().Children<Port>().ToList();

            List<OutputPortData> outputPortData = new();
            outputPorts.ForEach(port =>
            {
                // TODO: Clean this up a bit!
                string connectedGUID;

                Edge holder = port.connections.ToList().FirstOrDefault();

                if (holder != default)
                    connectedGUID = (holder.input.node as GraphNode).GUID;
                else
                    connectedGUID = GraphNodeStorage.EmptyPortGUID;

                outputPortData.Add(new OutputPortData(port.portName, connectedGUID));
            });

            nodes.Add(new NodeData(nodeGUID, outputPortData.ToArray(), nodePosition));
        }

        saveTarget.Nodes = nodes;
    }

    public void LoadGraph(GraphNodeStorage loadTarget)
    {
        // First pass to build nodes
        foreach (var nodeData in loadTarget.Nodes)
        {
            GraphNode newNode;

            if (nodeData.GUID != GraphNodeStorage.StartNodeGUID)
            {
                newNode = CreateDefaultNode(nodeData.GUID, nodeData.Position);
                AddInputPort(newNode);
                AddElement(newNode);
            }
            else
            {
                newNode = (GraphNode)nodes.First(node => (node as GraphNode).GUID == GraphNodeStorage.StartNodeGUID);
            }

            nodeData.OutputPorts.ToList().ForEach(port => AddOutputPort(newNode, port.Name));
        }

        // Second pass to build edges
        foreach (var nodeData in loadTarget.Nodes)
        {
            for (int portIndex = 0; portIndex < nodeData.OutputPorts.Length; portIndex++)
            {
                var portData = nodeData.OutputPorts[portIndex];

                // Skip ports with no connection
                if (portData.ConnectedGUID == GraphNodeStorage.EmptyPortGUID)
                    continue;

                var edge = new Edge()
                {
                    input = nodes.First(node => (node as GraphNode).GUID == portData.ConnectedGUID).inputContainer.Q<Port>(),
                    output = nodes.First(node => (node as GraphNode).GUID == nodeData.GUID).outputContainer.Query().Children<Port>().ToList().ElementAt(portIndex),
                };

                edge?.input.Connect(edge);
                edge?.output.Connect(edge);
                Add(edge);
            }
        }
    }

    public void ResetGraph()
    {
        nodes.ForEach(node => RemoveElement(node));
        edges.ForEach(edge => RemoveElement(edge));

        AddStartNode();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
        {
            // Dont allow to connect to same node, and not to same direction
            if (startPort.node != port.node && startPort.direction != port.direction)
                compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    protected void AddNewNode(string title = "", string tooltip = "")
    {
        var newNode = CreateDefaultNode(position: MousePosition);

        newNode.title = title;
        newNode.tooltip = tooltip;

        AddInputPort(newNode);
        AddOutputPort(newNode);
        AddElement(newNode);
    }

    protected GraphNode CreateDefaultNode(string GUID = "", Vector2 position = default)
    {
        if (string.IsNullOrEmpty(GUID))
            GUID = Guid.NewGuid().ToString();

        var newNode = new GraphNode(GUID);

        // Add button to add new output ports
        newNode.outputContainer.Add(new Button(() => AddOutputPort(newNode)) { text = "+", tooltip = "Add Port" });

        newNode.SetPosition(new Rect(position, defaultNodeSize));

        return newNode;
    }

    private void AddStartNode()
    {
        // Check if already have a 'start' node
        if (nodes.FirstOrDefault(node => (node as GraphNode).GUID == GraphNodeStorage.StartNodeGUID) != default)
            return;

        var startNode = CreateDefaultNode(GraphNodeStorage.StartNodeGUID, new Vector2(40, 40));
        startNode.title = "START";

        startNode.capabilities &= ~Capabilities.Deletable;

        startNode.RefreshPorts();
        startNode.RefreshExpandedState();

        AddElement(startNode);
    }

    protected void AddOutputPort(GraphNode target, string portName = "Out")
    {
        var newPort = target.InstantiateOutputPort();
        newPort.portName = portName;

        var portNameField = new TextField() { value = newPort.portName };
        portNameField.RegisterValueChangedCallback(evt =>
        {
            newPort.portName = evt.newValue;
        });

        newPort.Add(portNameField);

        newPort.Add(new Button(() => RemoveOutputPort(target, newPort)) { text = "-", tooltip = "Delete Port" });
        target.outputContainer.Add(newPort);

        target.RefreshPorts();
        target.RefreshExpandedState();
    }

    protected void AddInputPort(GraphNode target, string portName = "In")
    {
        var inputPort = target.InstantiateInputPort();
        inputPort.portName = portName;
        target.inputContainer.Add(inputPort);

        target.RefreshPorts();
        target.RefreshExpandedState();
    }

    protected void RemoveOutputPort(GraphNode target, Port port)
    {
        // Loop over all edges
        edges.ToList().ForEach(edge =>
        {
            // If the edge is connected to the port, remove it
            if (edge.output == port)
            {
                edge.input.Disconnect(edge);
                RemoveElement(edge);
            }
        });

        // Remove the port
        target.outputContainer.Remove(port);

        target.RefreshPorts();
        target.RefreshExpandedState();
    }
}