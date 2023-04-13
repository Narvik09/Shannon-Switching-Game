using Godot;
using System;
using Rubjerg.Graphviz;
using System.Collections.Generic;

using GNode = Rubjerg.Graphviz.Node;

public class RopeSegment : RigidBody2D
{

    // Each rope segment has an Id and a parent rope segment.
    public int ID = 0;
    public RigidBody2D Parent = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    private void RopeSegmentClicked()
    {
        // TODO: Maybe highlight the entire rope.
        Rope rope = this.GetParent<Rope>();
        TestStage testStage = (TestStage)rope.GetParent<TestStage>();
        if (testStage.cutPlaying)
        {
            // If Cut plays, remove the edge (delete from the graph).
            RemoveEdgeWithSegment();
            testStage.cutPlaying = false;
        }
        else
        {
            // If Short plays, reinforce the edge (change the sprites, update the graph).
            ReinforceEdgeWithSegment();
            testStage.cutPlaying = true;
        }

        if (DidCutWin(testStage.root))
        {
            GD.Print("Cut has won!");
            return;
        }
        if (DidShortWin(testStage.root))
        {
            GD.Print("Short has won!");
        }
    }

    private void ReinforceEdgeWithSegment()
    {
        Rope rope = this.GetParent<Rope>();

        string startNodeLabel = rope.startNodeLabel;
        string endNodeLabel = rope.endNodeLabel;
        string edgeName = rope.edgeName;

        TestStage testStage = (TestStage)rope.GetParent<TestStage>();
        Rubjerg.Graphviz.Node startNode = testStage.root.GetNode(startNodeLabel);
        Rubjerg.Graphviz.Node endNode = testStage.root.GetNode(endNodeLabel);

        Edge edge = testStage.root.GetEdge(endNode, startNode, edgeName);
        edge.SetAttribute("fixed", "true");

        foreach (RigidBody2D ropeSegment in rope.ropeSegments)
        {
            // Make button unclickable.
            ropeSegment.GetNode<Button>("Button").Disabled = true;
            // Change the sprite to chains.
            ropeSegment.GetNode<Sprite>("Chain-segment").Visible = true;
        }
    }

    private void RemoveEdgeWithSegment()
    {
        Rope rope = this.GetParent<Rope>();

        string startNodeLabel = rope.startNodeLabel;
        string endNodeLabel = rope.endNodeLabel;
        string edgeName = rope.edgeName;

        TestStage testStage = (TestStage)rope.GetParent<TestStage>();
        Rubjerg.Graphviz.Node startNode = testStage.root.GetNode(startNodeLabel);
        Rubjerg.Graphviz.Node endNode = testStage.root.GetNode(endNodeLabel);

        Edge edge = testStage.root.GetEdge(endNode, startNode, edgeName);
        testStage.root.Delete(edge);

        this.QueueFree();
        foreach (RigidBody2D ropeSegment in rope.ropeSegments)
        {
            ropeSegment.QueueFree();
        }
    }

    private Boolean DidCutWin(RootGraph graph)
    {
        Queue<GNode> q = new Queue<GNode>();
        HashSet<GNode> visited = new HashSet<GNode>();
        GNode u = graph.GetNode("Start");

        q.Enqueue(u);
        visited.Add(u);
        while (q.Count != 0)
        {
            GNode cur = q.Dequeue();
            foreach (GNode nxt in cur.Neighbors())
            {
                if (!visited.Contains(nxt))
                {
                    visited.Add(nxt);
                    q.Enqueue(nxt);
                }
            }
        }

        return !visited.Contains(graph.GetNode("End"));
    }

    private Boolean DidShortWin(RootGraph graph)
    {
        Queue<GNode> q = new Queue<GNode>();
        HashSet<GNode> visited = new HashSet<GNode>();
        GNode u = graph.GetNode("Start");

        q.Enqueue(u);
        visited.Add(u);
        while (q.Count != 0)
        {
            GNode cur = q.Dequeue();
            foreach (Edge edge in cur.Edges())
            {
                if (edge.GetAttribute("fixed") == null || !edge.GetAttribute("fixed").Equals("true"))
                {
                    continue;
                }
                GNode nxt = edge.Tail();
                if (nxt == cur)
                {
                    nxt = edge.Head();
                }
                if (!visited.Contains(nxt))
                {
                    visited.Add(nxt);
                    q.Enqueue(nxt);
                }
            }
        }

        foreach (GNode x in visited)
        {
            GD.Print(x.GetName());
        }

        return visited.Contains(graph.GetNode("End"));
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
