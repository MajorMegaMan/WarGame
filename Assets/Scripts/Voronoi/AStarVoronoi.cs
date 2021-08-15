using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi;

public class AStarVoronoi
{
    List<AStarNode> nodes = new List<AStarNode>();

    public void FillNodeList(List<VoronoiPoint> vPoints)
    {
        foreach(var point in vPoints)
        {
            nodes.Add(new AStarNode(point));
        }

        for(int i = 0; i < nodes.Count; i++)
        {
            foreach(int index in vPoints[i].connectedPoints)
            {
                nodes[i].connections.Add(nodes[index]);
            }
        }
    }

    public void DoAStar(AStarNode start, AStarNode end)
    {
        if(start == null || end == null)
        {
            return;
        }

        // initalise start node
        start.gScore = 0;
        start.previous = null;
    }
}

public class AStarNode
{
    VoronoiPoint vPoint;

    public float gScore;
    public float hScore;
    public float fScore;
    public AStarNode previous = null;

    public List<AStarNode> connections;
    public int index { get { return vPoint.index; } }

    public AStarNode(VoronoiPoint target)
    {
        this.vPoint = target;
        connections = new List<AStarNode>();
    }
}
