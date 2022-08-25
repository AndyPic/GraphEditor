using System;
using UnityEngine;

[Serializable]
public class OutputPortData
{
    [SerializeField] private string name;
    public string Name => name;

    [SerializeField] private string connectedGUID;
    public string ConnectedGUID => connectedGUID;

    public OutputPortData(string name, string connectedGUID)
    {
        this.name = name;
        this.connectedGUID = connectedGUID;
    }
}