using Godot;
using System;
using System.Collections;
using Rubjerg.Graphviz;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestStage : Node2D
{
    PackedScene Rope = (PackedScene)ResourceLoader.Load("res://Rope.tscn");
    PackedScene SegmentEnd = (PackedScene)ResourceLoader.Load("res://SegmentEnd.tscn");
    public Vector2 nodePos = Vector2.Zero;
    Dictionary<string, SegmentEnd> nodeDictionary = new Dictionary<string, SegmentEnd>();
    // Dictionary<string, HashSet<Tuple<string, bool>>> adjList = new Dictionary<string, HashSet<Tuple<string, bool>>>();

    public RootGraph root = null;

    private Random rand = new Random();

    public Global global;

    public SegmentEnd startNodeSegmentEnd = null, endNodeSegmentEnd = null;

    [Export(PropertyHint.File)]
    public string dotFilePath;

    public int numNodes;

    public RootGraph[] shortForests = new RootGraph[2];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        Global global = (Global)GetNode("/root/Global");
        dotFilePath = dotFilePath.Substring("res://".Length());
        // string temp = dotFilePath.Substr(0, dotFilePath.LastIndexOf("/"));
        // reading from dot file
        root = RootGraph.FromDotFile(dotFilePath);
        // using graphviz to compute a dot layout for setting node positions
        root.ComputeLayout(LayoutEngines.Neato);

        var nodes = root.Nodes();
        var edges = root.Edges();

        // coordinates corresponding to the center of the screen 
        Vector2 screenCenter = GetViewportRect().Size / 2;
        Vector2 screenSize = GetViewportRect().Size;
        Vector2 center = Vector2.Zero;

        double minX = double.PositiveInfinity, maxX = double.NegativeInfinity;
        double minY = double.PositiveInfinity, maxY = double.NegativeInfinity;
        double width = screenSize.x;
        double height = screenSize.y;
        int count = 0;

        // computing centroid 
        foreach (var node in nodes)
        {
            PointF position = node.Position();
            nodePos = new Vector2(position.X, position.Y);
            center += nodePos;
            count++;
        }
        center /= count;
        screenCenter -= center;

        numNodes = count;

        string minNode = "", maxNode = "";
        // computing max and min coords
        foreach (var node in nodes)
        {
            PointF position = node.Position();
            nodePos = new Vector2(position.X, position.Y);
            nodePos += screenCenter;
            if (minX > nodePos.x)
            {
                minNode = node.GetName();
            }
            if (maxX < nodePos.x)
            {
                maxNode = node.GetName();
            }
            minX = Math.Min(minX, nodePos.x);
            maxX = Math.Max(maxX, nodePos.x);
            minY = Math.Min(minY, nodePos.y);
            maxY = Math.Max(maxY, nodePos.y);
        }

        // padding
        minX -= 0.05 * width;
        maxX += 0.05 * width;
        minY -= 0.05 * height;
        maxY += 0.05 * height;

        foreach (var node in nodes)
        {
            PointF position = node.Position();
            nodePos = new Vector2(position.X, position.Y);
            // centering
            nodePos += screenCenter;
            // scaling
            nodePos.x = (float)((nodePos.x - minX) * width / (maxX - minX + 1));
            nodePos.y = (float)((nodePos.y - minY) * height / (maxY - minY + 1));
            // GD.Print(nodePos);
            SegmentEnd segmentEnd = (SegmentEnd)SegmentEnd.Instance();
            segmentEnd.GlobalPosition = nodePos;
            segmentEnd.GetNode<Sprite>("Platform").Texture = (Texture)ResourceLoader.Load("res://nodePlatform_" + rand.Next(1, 4).ToString() + ".png");
            // coloring start and end nodes
            if (node == root.GetNode("Start"))
            {
                segmentEnd.GetNode<Sprite>("Platform").Visible = false;
                segmentEnd.GetNode<Sprite>("EndPlatform").Visible = true;
                segmentEnd.GetNode<Sprite>("EndPlatform").FlipH = true;
                segmentEnd.GetNode<KinematicBody2D>("ShortPlayer").Visible = true;
                startNodeSegmentEnd = segmentEnd;
            }
            else if (node == root.GetNode("End"))
            {
                segmentEnd.GetNode<Sprite>("Platform").Visible = false;
                segmentEnd.GetNode<Sprite>("EndPlatform").Visible = true;
                segmentEnd.GetNode<KinematicBody2D>("CutPlayer").Visible = true;
                endNodeSegmentEnd = segmentEnd;
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

    public void MoveCut()
    {
        FreezeButtons();
        GD.Print("Cut's move has been computed!");
        UnfreezeButtons();
    }

    public void MoveShort()
    {
        FreezeButtons();

        // Finding out if Short can actually win.
        if (shortForests[0] == null)
        {
            List<RootGraph> subGraphs = GetSubgraphs(root);
            foreach (RootGraph subGraph in subGraphs)
            {
                FindEdgeDisjointSpanningTreePair(subGraph, shortForests);
                if (shortForests[0] != null)
                {
                    break;
                }
            }
        }

        // Finding the edge that needs to be chosen
        if (shortForests[0] != null)
        {
            // Find out which edge Cut played last, and which spanning tree it broke.
            // Then, iterate over edges and fix the one that crosses the cut.
        }
        else
        {
            // Choose a random edge (maybe add heuristics for now).
        }

        GD.Print("Short's move has been computed!");

        UnfreezeButtons();
    }

    // Helper function for getting subgraphs containing the starting and ending nodes.
    private List<RootGraph> GetSubgraphs(RootGraph root)
    {
        List<RootGraph> subGraphs = new List<RootGraph>();

        int index = 0;
        Dictionary<int, string> nodeMap = new Dictionary<int, string>();
        foreach (var node in root.Nodes())
        {
            if (node.GetName().Equals("Start") || node.GetName().Equals("End"))
            {
                continue;
            }
            nodeMap[index] = node.GetName();
            index++;
        }

        int x = numNodes - 2;
        for (int mask = 0; mask < (1 << x); mask++)
        {
            RootGraph subGraph = RootGraph.CreateNew("Trial", GraphType.Undirected);
            subGraph.GetOrAddNode("Start");
            subGraph.GetOrAddNode("End");
            for (int bit = 0; bit < x; bit++)
            {
                if (((1 << bit) & mask) > 0)
                {
                    subGraph.GetOrAddNode(nodeMap[bit]);
                }
            }

            foreach (var edge in root.Edges())
            {
                var u = edge.Head();
                var v = edge.Tail();
                if (subGraph.Contains(u) && subGraph.Contains(v))
                {
                    subGraph.GetOrAddEdge(
                        subGraph.GetNode(u.GetName()),
                        subGraph.GetNode(v.GetName()),
                        edge.GetName() + "_1");
                    // Adding a double edge here.
                    if (edge.GetAttribute("fixed") != null && edge.GetAttribute("fixed").Equals("true"))
                    {
                        subGraph.GetOrAddEdge(
                            subGraph.GetNode(u.GetName()),
                            subGraph.GetNode(v.GetName()),
                            edge.GetName() + "_2");
                    }
                }
            }

            subGraphs.Add(subGraph);
        }

        return subGraphs;
    }

    private void FindEdgeDisjointSpanningTreePair(RootGraph graph, RootGraph[] forests)
    {
        // Do stuff here?
    }

    private void FreezeButtons()
    {

    }

    private void UnfreezeButtons()
    {

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
