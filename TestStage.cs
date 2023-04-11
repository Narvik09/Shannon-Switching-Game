using Godot;
using System;
using System.Collections;
using Rubjerg.Graphviz;
using System.Drawing;
using System.Collections.Generic;

public class TestStage : Node2D
{
    PackedScene Rope = (PackedScene)ResourceLoader.Load("res://Rope.tscn");
    PackedScene SegmentEnd = (PackedScene)ResourceLoader.Load("res://SegmentEnd.tscn");
    Vector2 nodePos = Vector2.Zero;
    Vector2 endPos = Vector2.Zero;

    Dictionary<string, SegmentEnd> nodeDictionary = new Dictionary<string, SegmentEnd>();
    Dictionary<string, HashSet<Tuple<string, bool>>> adjList = new Dictionary<string, HashSet<Tuple<string, bool>>>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // reading from dot file
        RootGraph root = RootGraph.FromDotFile("C:\\Users\\narvi\\Documents\\Godot\\GraphTesting\\out.dot");
        // using graphviz to compute a dot layout for setting node positions
        root.ComputeLayout(LayoutEngines.Neato);
        root.ToSvgFile("C:\\Users\\narvi\\Documents\\Godot\\GraphTesting\\dot_out.svg");

        var nodes = root.Nodes();
        var edges = root.Edges();

        // coordinates corresponding to the center of the screen 
        Vector2 screenCenter = GetViewportRect().Size / 2;
        foreach (var node in nodes)
        {

            PointF position = node.Position();
            nodePos = new Vector2(position.X, position.Y);

            // translating graph to center of the screen
            // TODO : Center and scale the graph accordingly
            nodePos += screenCenter;

            SegmentEnd segmentEnd = (SegmentEnd)SegmentEnd.Instance();
            segmentEnd.GlobalPosition = nodePos;
            AddChild(segmentEnd);

            nodeDictionary.Add(node.GetName(), segmentEnd);
            GD.Print(nodeDictionary.Count);
        }

        int edgeNumber = 0;
        foreach (var edge in root.Edges())
        {
            string u = edge.Head().GetName();
            string v = edge.Tail().GetName();

            Rope rope = (Rope)Rope.Instance();
            AddChild(rope);

            rope.EdgeNumber = edgeNumber;
            rope.SpawnRope(nodeDictionary[u], nodeDictionary[v]);

            if (!adjList.ContainsKey(u))
            {
                adjList.Add(u, new HashSet<Tuple<string, bool>>());
            }
            adjList[u].Add(new Tuple<string, bool>(v, false));

            if (!adjList.ContainsKey(v))
            {
                adjList.Add(v, new HashSet<Tuple<string, bool>>());
            }
            adjList[v].Add(new Tuple<string, bool>(u, false));

            edgeNumber++;
        }

        root.FreeLayout();
    }

    // Input function overriding
    // public override void _Input(InputEvent @event)
    // {
    //     if (@event is InputEventMouseButton && !(@event.IsPressed()))
    //     {
    //         // get start position
    //         if (startPos == Vector2.Zero)
    //         {
    //             startPos = GetGlobalMousePosition();
    //         }
    //         // get end position
    //         else if (endPos == Vector2.Zero)
    //         {
    //             endPos = GetGlobalMousePosition();
    //             Rope rope = (Rope)Rope.Instance();
    //             AddChild(rope);
    //             // spawn rope after end position is set
    //             rope.SpawnRope(startPos, endPos);
    //             // reset start and end position
    //             startPos = Vector2.Zero;
    //             endPos = Vector2.Zero;
    //         }

    //     }
    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
