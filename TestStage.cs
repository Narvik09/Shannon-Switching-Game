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
    public Vector2 nodePos = Vector2.Zero;
    Dictionary<string, SegmentEnd> nodeDictionary = new Dictionary<string, SegmentEnd>();
    // Dictionary<string, HashSet<Tuple<string, bool>>> adjList = new Dictionary<string, HashSet<Tuple<string, bool>>>();

    public RootGraph root = null;

    public Boolean cutPlaying = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // reading from dot file
        root = RootGraph.FromDotFile("C:\\Users\\narvi\\Documents\\Godot\\GraphTesting\\out.dot");

        // using graphviz to compute a dot layout for setting node positions
        root.ComputeLayout(LayoutEngines.Neato);
        root.ToSvgFile("C:\\Users\\narvi\\Documents\\Godot\\GraphTesting\\dot_out.svg");

        var nodes = root.Nodes();
        var edges = root.Edges();
        // coordinates corresponding to the center of the screen 
        Vector2 screenCenter = GetViewportRect().Size / 2;
        Vector2 center = Vector2.Zero;
        int count = 0;
        foreach (var node in nodes)
        {
            PointF position = node.Position();
            nodePos = new Vector2(position.X, position.Y);
            center += nodePos;
            count++;
        }
        center /= count;
        screenCenter -= center;

        foreach (var node in nodes)
        {
            PointF position = node.Position();
            nodePos = new Vector2(position.X, position.Y);
            // translating graph to center of the screen
            // TODO : Center and scale the graph accordingly
            nodePos += screenCenter;
            GD.Print(nodePos);
            SegmentEnd segmentEnd = (SegmentEnd)SegmentEnd.Instance();
            segmentEnd.GlobalPosition = nodePos;
            // coloring start and end nodes
            if (node == root.GetNode("Start"))
            {
                segmentEnd.GetNode<Sprite>("Platform").Texture = (Texture)ResourceLoader.Load("res://mainPlatform.png");
                segmentEnd.GetNode<KinematicBody2D>("ShortPlayer").Visible = true;

            }
            else if (node == root.GetNode("End"))
            {
                segmentEnd.GetNode<Sprite>("Platform").Texture = (Texture)ResourceLoader.Load("res://mainPlatform.png");
                segmentEnd.GetNode<KinematicBody2D>("CutPlayer").Visible = true;
            }
            AddChild(segmentEnd);

            nodeDictionary.Add(node.GetName(), segmentEnd);
        }

        foreach (var edge in root.Edges())
        {
            string u = edge.Head().GetName();
            string v = edge.Tail().GetName();
            string edgeName = edge.GetName();

            Rope rope = (Rope)Rope.Instance();
            AddChild(rope);

            rope.SpawnRope(nodeDictionary[u], nodeDictionary[v], u, v, edgeName);

            // if (!adjList.ContainsKey(u))
            // {
            //     adjList.Add(u, new HashSet<Tuple<string, bool>>());
            // }
            // adjList[u].Add(new Tuple<string, bool>(v, false));

            // if (!adjList.ContainsKey(v))
            // {
            //     adjList.Add(v, new HashSet<Tuple<string, bool>>());
            // }
            // adjList[v].Add(new Tuple<string, bool>(u, false));

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
