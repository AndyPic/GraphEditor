using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "GraphNodeStorage", menuName = "ScriptableObjects/GraphNodeStorage", order = 2)]
public class GraphNodeStorage : ScriptableObject
{
    // TODO: Change to Dict<GUID, node> if Unity ever make dict serializable
    [SerializeField] private List<NodeData> nodes;
    public List<NodeData> Nodes { get { return nodes; } set { nodes = value; } }

    public const string StartNodeGUID = "startnode";
    public const string EmptyPortGUID = "";

    /// <summary>
    /// Gets node data indexed by GUID
    /// </summary>
    /// <param name="guid"></param>
    /// <returns>
    /// The NodeData with <paramref name="guid"/>, Null if not found.
    /// </returns>
    public NodeData this[string guid] => nodes.FirstOrDefault(node => node.GUID == guid);

    /// <summary>
    /// The starting node of the graph.
    /// </summary>
    public NodeData StartNode
    {
        get
        {
            return nodes.FirstOrDefault(node => node.GUID == StartNodeGUID);
        }
    }

    /// <summary>
    /// Base class for all implementation specific additional node data to inherit from
    /// </summary>
    [Serializable]
    public abstract class A_AditionalNodeData { }

    public class DialogueNodeInfo : A_AditionalNodeData
    {
        public string dialogue;
    }
}