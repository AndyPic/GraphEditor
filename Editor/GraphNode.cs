using UnityEditor.Experimental.GraphView;

public class GraphNode : Node
{
    public string GUID;

    public GraphNode(string GUID)
    {
        this.GUID = GUID;
    }

    public Port InstantiateInputPort()
    {
        return base.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
    }

    public Port InstantiateOutputPort()
    {
        return base.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
    }
}