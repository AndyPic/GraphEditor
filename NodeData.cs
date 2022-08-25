using System;
using UnityEngine;

[Serializable]
public class NodeData
{
    [SerializeField] private string guid;
    public string GUID => guid;

    [SerializeField] private OutputPortData[] outputPorts;
    public OutputPortData[] OutputPorts => outputPorts;

#if UNITY_EDITOR
    [SerializeField] private Vector2 position;
    public Vector2 Position => position;
#endif

    public NodeData(string guid, OutputPortData[] outputPorts, Vector2 position)
    {
        this.guid = guid;
        this.outputPorts = outputPorts;
#if UNITY_EDITOR
        this.position = position;
#endif
    }
}