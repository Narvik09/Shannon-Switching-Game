using Godot;
using System;
using Rubjerg.Graphviz;
using System.Drawing;
using System.Collections.Generic;

public class TestStage : Node2D
{
    PackedScene Rope = (PackedScene)ResourceLoader.Load("res://screens/gameScreen/bridge/Rope.tscn");
    PackedScene SegmentEnd = (PackedScene)ResourceLoader.Load("res://screens/gameScreen/bridge/SegmentEnd.tscn");
    public Vector2 nodePos = Vector2.Zero;
    Dictionary<string, SegmentEnd> nodeDictionary = new Dictionary<string, SegmentEnd>();

    public List<Rope> ropes = new List<Rope>();

    public RootGraph root = null;

    public RootGraph dual = null;

    private Random rand = new Random();

    public Global global;

    public SegmentEnd startNodeSegmentEnd = null, endNodeSegmentEnd = null;

    [Export(PropertyHint.File)]
    public string dotFilePath = "res://screens/gameScreen/assets/dotFiles/Level_1.dot";

    public int numNodes = 0, numEdges = 0;

    // public RootGraph[] shortForests = new RootGraph[3];

    public Dictionary<Rubjerg.Graphviz.Node, int> nodeToNum = new Dictionary<Rubjerg.Graphviz.Node, int>();

    public Dictionary<int, Rubjerg.Graphviz.Node> numToNode = new Dictionary<int, Rubjerg.Graphviz.Node>();

    public Dictionary<Edge, int> edgeToNum = new Dictionary<Edge, int>();

    public Dictionary<int, Edge> numToEdge = new Dictionary<int, Edge>();

    public List<Tuple<int, int>>[] adjList;

    public Boolean computerCanWin = false;

    public RootGraph[] forests = new RootGraph[2];

    public int mask;

    public string oppLastTailName, oppLastHeadName, oppLastName = null;

    public int oppLastNum = -1;

    public int[] index = null;

    public Boolean[] edgeFix = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        GD.Print(global.isPlayingFirst);

        dotFilePath = dotFilePath.Substring("res://".Length());
        // reading from dot file
        // RootGraph.FromDotFile("screens/gameScreen/assets/dotFiles/Level_4.dot");
        // RootGraph graph = RootGraph.FromDotFile(dotFilePath);
        // RootGraph graph = global.rootGraph;
        // ParseGraph(graph);
        root = global.rootGraph;
        dual = global.dualGraph;
        // dual = RootGraph.FromDotFile("dotFiles/Level_1_Dual.dot");

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
            segmentEnd.GetNode<Sprite>("Platform").Texture = (Texture)ResourceLoader.Load("res://screens/gameScreen/assets/nodePlatform_" + rand.Next(1, 4).ToString() + ".png");
            // coloring start and end nodes
            if (node == root.GetNode("Start"))
            {
                segmentEnd.GetNode<Sprite>("Platform").Visible = false;
                segmentEnd.GetNode<Sprite>("EndPlatform").Visible = true;
                segmentEnd.GetNode<Sprite>("EndPlatform").FlipH = true;
                segmentEnd.GetNode<KinematicBody2D>("ShortPlayer").Visible = true;
                startNodeSegmentEnd = segmentEnd;
                segmentEnd.PlayAnimations(global.isCutPlaying);
            }
            else if (node == root.GetNode("End"))
            {
                segmentEnd.GetNode<Sprite>("Platform").Visible = false;
                segmentEnd.GetNode<Sprite>("EndPlatform").Visible = true;
                segmentEnd.GetNode<KinematicBody2D>("CutPlayer").Visible = true;
                endNodeSegmentEnd = segmentEnd;
                segmentEnd.PlayAnimations(global.isCutPlaying);
            }
            AddChild(segmentEnd);

            nodeDictionary.Add(node.GetName(), segmentEnd);
        }

        count = 0;
        foreach (var edge in root.Edges())
        {
            string u = edge.Head().GetName();
            string v = edge.Tail().GetName();
            string edgeName = edge.GetName();

            Rope rope = (Rope)Rope.Instance();
            AddChild(rope);

            rope.SpawnRope(nodeDictionary[u], nodeDictionary[v], u, v, edgeName);

            ropes.Add(rope);
            count++;
        }

        numEdges = count;


        if (global.isShortComputer)
        {
            FreezeButtons();
            CreateListAndMaps(root);
            if (!global.isCutPlaying)
            {
                MoveShort();
                global.isCutPlaying = !global.isCutPlaying;
                ropes[0].allRopeSegments[0].CheckForWinners(this);
            }
            UnfreezeButtons();
        }
        if (global.isCutComputer)
        {
            FreezeButtons();
            CreateListAndMaps(dual);
            if (global.isCutPlaying)
            {
                MoveCut();
                global.isCutPlaying = !global.isCutPlaying;
                ropes[0].allRopeSegments[0].CheckForWinners(this);
            }
            UnfreezeButtons();
        }

        GD.Print("Done!!!");

        root.FreeLayout();
    }

    private Boolean isDual(Rubjerg.Graphviz.Node node)
    {
        return (node.GetName()[node.GetName().Length - 1].Equals('D'));
    }

    private void ParseGraph(RootGraph graph)
    {
        root = RootGraph.CreateNew("root", GraphType.Undirected);
        dual = RootGraph.CreateNew("dual", GraphType.Undirected);
        foreach (var node in graph.Nodes())
        {
            if (isDual(node))
            {
                dual.GetOrAddNode(node.GetName().Substr(0, node.GetName().Length - 1));
            }
            else
            {
                root.GetOrAddNode(node.GetName());
                root.GetNode(node.GetName()).SafeSetAttribute("pos", node.Position().ToString(), "");
                // GD.Print(node.Position().ToString());
            }
        }
        foreach (var edge in graph.Edges())
        {
            Rubjerg.Graphviz.Node x = edge.Tail(), y = edge.Head();
            if (isDual(x))
            {
                dual.GetOrAddEdge(
                    dual.GetNode(x.GetName().Substr(0, x.GetName().Length - 1)),
                    dual.GetNode(y.GetName().Substr(0, y.GetName().Length - 1)),
                    edge.GetName()
                );
            }
            else
            {
                root.GetOrAddEdge(
                    root.GetNode(x.GetName()),
                    root.GetNode(y.GetName()),
                    edge.GetName()
                );
            }
        }
    }

    // Maps the graph to a lighter adjacency list structure with associated maps.
    public void CreateListAndMaps(RootGraph gph)
    {
        nodeToNum.Clear();
        numToNode.Clear();
        edgeToNum.Clear();
        numToEdge.Clear();

        // Mapping nodes to numbers.
        int cur = 1;
        foreach (var node in gph.Nodes())
        {
            int val = cur;
            if (node.GetName().Equals("Start"))
            {
                val = 0;
            }
            else if (node.GetName().Equals("End"))
            {
                val = numNodes - 1;
            }
            else
            {
                cur++;
            }
            nodeToNum[node] = val;
            numToNode[val] = node;
        }

        // Mapping edges to numbers.
        cur = 0;
        foreach (var edge in gph.Edges())
        {
            edgeToNum[edge] = cur;
            numToEdge[cur] = edge;
            cur++;
        }

        // Marking fixed edges.
        edgeFix = new Boolean[cur];
        cur = 0;
        foreach (var edge in gph.Edges())
        {
            edgeFix[cur] = false;
            if (edge.GetAttribute("fixed") != null && edge.GetAttribute("fixed").Equals("true"))
            {
                edgeFix[cur] = true;
                cur++;
            }
        }

        // Creating the adjacency list.
        adjList = new List<Tuple<int, int>>[numNodes];
        for (int i = 0; i < numNodes; i++)
        {
            adjList[i] = new List<Tuple<int, int>>();
        }
        foreach (var entry in numToEdge)
        {
            int x = nodeToNum[entry.Value.Head()];
            int y = nodeToNum[entry.Value.Tail()];
            adjList[x].Add(new Tuple<int, int>(y, entry.Key));
            adjList[y].Add(new Tuple<int, int>(x, entry.Key));
        }
    }

    public void MoveCut()
    {
        // FreezeButtons();

        // Finding out if Cut can actually win.
        Edge pick = null;
        if (!computerCanWin)
        {
            // If there is something that Cut must play (to get the required structure)
            // or if there is something that is **favourable** for Short to play,
            // the edge to pick gets assigned here.
            CreateListAndMaps(dual);
            pick = SearchOverAllSubgraphs();
        }

        if (pick != null)
        {
            GD.Print("Calculated...");
        }

        // Finding a random edge if we have nothing.
        if (pick == null && (!computerCanWin || oppLastNum == -1 ||
        (computerCanWin && index[oppLastNum] == -1)))
        {
            GD.Print("Going random...");
            foreach (var edge in dual.Edges())
            {
                if (edge.GetAttribute("fixed") == null || !edge.GetAttribute("fixed").Equals("true"))
                {
                    pick = edge;
                    break;
                }
            }
        }
        else if (pick == null)
        {
            // Find out which edge Short played last, and which spanning tree it broke.
            // Then, iterate over edges and fix the one that crosses the cut.
            int i = index[oppLastNum];
            GD.Print("Short broke forest " + i + "...");
            var x = forests[i].GetOrAddNode(oppLastTailName);
            var y = forests[i].GetOrAddNode(oppLastHeadName);
            var edgeToDelete = forests[i].GetOrAddEdge(x, y, oppLastName);
            forests[i].Delete(edgeToDelete);

            int visNum = 1;
            Dictionary<Rubjerg.Graphviz.Node, int> searchOrder = new Dictionary<Rubjerg.Graphviz.Node, int>();
            foreach (var node in forests[i].Nodes())
            {
                if (!searchOrder.ContainsKey(node))
                {
                    Queue<Rubjerg.Graphviz.Node> q = new Queue<Rubjerg.Graphviz.Node>();
                    searchOrder[node] = visNum;
                    q.Enqueue(node);
                    while (q.Count > 0)
                    {
                        Rubjerg.Graphviz.Node u = q.Dequeue();
                        foreach (var v in u.Neighbors())
                        {
                            if (!searchOrder.ContainsKey(v))
                            {
                                searchOrder[v] = visNum;
                                q.Enqueue(v);
                            }
                        }
                    }
                    visNum++;
                }
            }
            // GD.Print("Forest 0 : ");

            // foreach (var edge in forests[0].Edges())
            // {
            //     GD.Print("Tail : " + edge.Tail().GetName() + "; Head : " + edge.Head().GetName());
            // }
            // GD.Print("Forest 1 : ");
            // foreach (var edge in forests[1].Edges())
            // {
            //     GD.Print("Tail : " + edge.Tail().GetName() + "; Head : " + edge.Head().GetName());
            // }

            GD.Print("Forest 1: ");
            foreach (var node in forests[i].Nodes())
            {
                GD.Print(node.GetName());
            }
            GD.Print("Forest 2: ");
            foreach (var node in forests[i ^ 1].Nodes())
            {
                GD.Print(node.GetName());
            }

            GD.Print("Number of components: " + (visNum - 1));
            foreach (var edge in forests[i ^ 1].Edges())
            {
                // GD.Print("1: " + searchOrder[forests[i].GetNode(edge.Tail().GetName())]);

                // GD.Print("2: " + searchOrder[forests[i].GetNode(edge.Head().GetName())]);
                if (forests[i].GetNode(edge.Tail().GetName()) == null)
                {
                    GD.Print("This is null for some reason. [Tail]");
                }
                if (forests[i].GetNode(edge.Head().GetName()) == null)
                {
                    GD.Print("This is null for some reason. [Head]");
                }
                if (searchOrder[forests[i].GetNode(edge.Tail().GetName())] !=
                searchOrder[forests[i].GetNode(edge.Head().GetName())])
                {
                    // GD.Print("OKAY!");
                    // We need the edge in the original dual.
                    var ox = dual.GetNode(edge.Tail().GetName());
                    var oy = dual.GetNode(edge.Head().GetName());
                    pick = dual.GetEdge(ox, oy, edge.GetName());
                    break;
                }
            }
        }

        // Now fix pick in the forest structure (if it's there)!
        var xName = pick.Tail().GetName();
        var yName = pick.Head().GetName();
        var eName = pick.GetName();
        if (computerCanWin && forests[0].GetNode(xName) != null && forests[0].GetNode(yName) != null)
        {
            for (int i = 0; i < 2; i++)
            {
                forests[i].GetOrAddEdge(
                    forests[i].GetNode(xName),
                    forests[i].GetNode(yName),
                    eName
                );
            }
        }

        // Rope with this string as its edgeName (in the original graph).
        foreach (var edge in root.Edges())
        {
            if (edge.GetName().Equals(pick.GetName()))
            {
                pick = edge;
                break;
            }
        }

        Rope pickedRope = null;
        foreach (Rope rope in ropes)
        {
            if (pick.GetName() == rope.edgeName)
            {
                pickedRope = rope;
                break;
            }
        }

        // Take any rope segment and call RemoveEdgeWithSegment() in it.
        pickedRope.allRopeSegments[0].RemoveEdgeWithSegment();
        GD.Print("Cut's move has been computed!");
        // UnfreezeButtons();
    }

    public void MoveShort()
    {
        // FreezeButtons();

        // Finding out if Short can actually win.
        Edge pick = null;
        if (!computerCanWin)
        {
            // If there is something that Short must play (to get the required structure)
            // or if there is something that is **favourable** for Short to play,
            // the edge to pick gets assigned here.
            CreateListAndMaps(root);
            pick = SearchOverAllSubgraphs();
        }

        if (pick != null)
        {
            GD.Print("Calculated...");
        }

        // Finding a random edge if we have nothing.
        if (pick == null && (!computerCanWin || oppLastNum == -1 ||
        (computerCanWin && index[oppLastNum] == -1)))
        {
            GD.Print("Going random...");
            foreach (var edge in root.Edges())
            {
                if (edge.GetAttribute("fixed") == null || !edge.GetAttribute("fixed").Equals("true"))
                {
                    pick = edge;
                    break;
                }
            }
        }
        else if (pick == null)
        {
            // Find out which edge Cut played last, and which spanning tree it broke.
            // Then, iterate over edges and fix the one that crosses the cut.
            int i = index[oppLastNum];
            GD.Print("Cut broke forest " + i + "...");
            var x = forests[i].GetOrAddNode(oppLastTailName);
            var y = forests[i].GetOrAddNode(oppLastHeadName);
            var edgeToDelete = forests[i].GetOrAddEdge(x, y, oppLastName);
            forests[i].Delete(edgeToDelete);

            int visNum = 1;
            Dictionary<Rubjerg.Graphviz.Node, int> searchOrder = new Dictionary<Rubjerg.Graphviz.Node, int>();
            foreach (var node in forests[i].Nodes())
            {
                if (!searchOrder.ContainsKey(node))
                {
                    Queue<Rubjerg.Graphviz.Node> q = new Queue<Rubjerg.Graphviz.Node>();
                    searchOrder[node] = visNum;
                    q.Enqueue(node);
                    while (q.Count > 0)
                    {
                        Rubjerg.Graphviz.Node u = q.Dequeue();
                        foreach (var v in u.Neighbors())
                        {
                            if (!searchOrder.ContainsKey(v))
                            {
                                searchOrder[v] = visNum;
                                q.Enqueue(v);
                            }
                        }
                    }
                    visNum++;
                }
            }

            foreach (var edge in forests[i ^ 1].Edges())
            {
                if (searchOrder[forests[i].GetNode(edge.Tail().GetName())] !=
                searchOrder[forests[i].GetNode(edge.Head().GetName())])
                {
                    GD.Print("Dang.");
                    // We need the edge in the original graph.
                    var ox = root.GetNode(edge.Tail().GetName());
                    var oy = root.GetNode(edge.Head().GetName());
                    pick = root.GetEdge(ox, oy, edge.GetName());
                    break;
                }
            }
        }

        // Now fix pick in the forest structure (if it's there)!
        var xName = pick.Tail().GetName();
        var yName = pick.Head().GetName();
        var eName = pick.GetName();
        if (computerCanWin && forests[0].GetNode(xName) != null && forests[0].GetNode(yName) != null)
        {
            for (int i = 0; i < 2; i++)
            {
                forests[i].GetOrAddEdge(
                    forests[i].GetNode(xName),
                    forests[i].GetNode(yName),
                    eName
                );
            }
        }

        Rope pickedRope = null;
        foreach (Rope rope in ropes)
        {
            if (pick.GetName() == rope.edgeName)
            {
                pickedRope = rope;
                break;
            }
        }
        // Rope with this string as its edgeName
        // Take any rope segment and call ReinforceEdgeWithSegment() in it.
        pickedRope.allRopeSegments[0].ReinforceEdgeWithSegment();
        GD.Print("Short's move has been computed!");

        // UnfreezeButtons();
    }

    // Helper function for getting subgraphs containing the starting and ending nodes.
    private Edge SearchOverAllSubgraphs()
    {
        int n = adjList.Length;
        int cnt = 0;
        for (int mask = 0; mask < (1 << (n - 2)) && !computerCanWin; mask++)
        {
            int actualMask = (1 << (n - 1)) | (mask << 1) | 1;
            int subNodes = 0, subEdges = 0;
            Boolean oneDegree = false;

            // Checking if the subgraph is connected (breadth-first search).
            // And if it contains enough edges to potentially have two spanning trees sharing at most one edge.
            int visitMask = 1;
            Queue<int> q = new Queue<int>();
            q.Enqueue(0);
            while (q.Count > 0)
            {
                int u = q.Dequeue();
                subNodes++;
                int degree = 0;
                foreach (var entry in adjList[u])
                {
                    int v = entry.Item1;
                    if ((actualMask & (1 << v)) > 0)
                    {
                        if (edgeFix[entry.Item2])
                        {
                            subEdges += 2;
                        }
                        else
                        {
                            subEdges++;
                        }
                        degree++;
                    }
                    if ((visitMask & (1 << v)) == 0)
                    {
                        visitMask |= (1 << v);
                        q.Enqueue(v);
                    }
                }

                // Can't have a spanning tree if one of the nodes has a degree of only one.
                if (degree == 1)
                {
                    oneDegree = true;
                }
            }

            subEdges /= 2;
            if (subEdges + 1 < 2 * (subNodes - 1) || visitMask != actualMask || oneDegree)
            {
                continue;
            }

            cnt++;

            Edge e = FindEdgeDisjointSpanningTreePair(actualMask);
            if (computerCanWin)
            {
                return e;
            }
        }

        GD.Print("Count: " + cnt);
        return null;
    }

    // Finds edge disjoint spanning trees.
    // Also may return an edge that needs to be chosen to maintain the structure.
    private Edge FindEdgeDisjointSpanningTreePair(int mask)
    {
        GD.Print("FindEdgeDisjointSpanningTreePair()...");
        DisjointSetUnion[] partitions = new DisjointSetUnion[3];
        for (int i = 0; i < 3; i++)
        {
            partitions[i] = new DisjointSetUnion(numNodes);
        }

        // Initialising the index array (indicating which edge is in which partition).
        // Double space is for the copies of the edge.
        int[] index = new int[2 * numToEdge.Count];
        for (int i = 0; i < 2 * numToEdge.Count; i++)
        {
            index[i] = -1;
        }

        int addedEdges = 0, addedNodes = 0, addedWeight = 0;
        for (int i = 0; i < 30; i++)
        {
            if ((mask & (1 << i)) > 0)
            {
                addedNodes++;
            }
        }

        List<Tuple<int, int>>[] edgeOrder = new List<Tuple<int, int>>[3];
        for (int i = 0; i < 3; i++)
        {
            edgeOrder[i] = new List<Tuple<int, int>>();
        }
        foreach (int edgeNum in numToEdge.Keys)
        {
            if (edgeFix[edgeNum])
            {
                // Both edges have weight 1.
                edgeOrder[1].Add(new Tuple<int, int>(edgeNum, 0));
                edgeOrder[1].Add(new Tuple<int, int>(edgeNum, 1));
            }
            else
            {
                // One edge has weight 1 and the other has weight 2.
                edgeOrder[1].Add(new Tuple<int, int>(edgeNum, 0));
                edgeOrder[2].Add(new Tuple<int, int>(edgeNum, 1));
            }
        }

        GD.Print("---Original-Graph---");
        for (int i = 0; i < index.Length; i++)
        {
            int j = i, xx = 0;
            if (j >= numToEdge.Count)
            {
                j -= numToEdge.Count;
                xx = 1;
            }
            GD.Print(numToEdge[j].Head().GetName() + " " + numToEdge[j].Tail().GetName() + " " + xx);
        }

        for (int weight = 1; weight <= 2; weight++)
        {
            foreach (var entry in edgeOrder[weight])
            {
                int edgeNum = entry.Item1;
                int isDup = entry.Item2;

                int x = nodeToNum[numToEdge[edgeNum].Head()];
                int y = nodeToNum[numToEdge[edgeNum].Tail()];

                // GD.Print("=> Adding Edge: " + numToEdge[edgeNum].Head().GetName() + " " + numToEdge[edgeNum].Tail().GetName() + " " + isDup);

                // Edge isn't in the subgraph, skip.
                if (((1 << x) & mask) == 0 || ((1 << y) & mask) == 0)
                {
                    continue;
                }

                // x and y belong to the same 'clump', skip.
                if (partitions[2].Find(x) == partitions[2].Find(y))
                {
                    continue;
                }

                // Initialising the label array.
                // Double space is for the copies of the edge.
                int[] label = new int[2 * numToEdge.Count];
                for (int i = 0; i < 2 * numToEdge.Count; i++)
                {
                    label[i] = -1;
                }

                int[] par0 = GetParents(mask, index, 0);
                int[] par1 = GetParents(mask, index, 1);

                int[] dep0 = GetDepths(mask, index, 0);
                int[] dep1 = GetDepths(mask, index, 1);

                GD.Print("Zero depth nodes for [" + 0 + "]:");
                for (int i = 0; i < numNodes; i++)
                {
                    GD.Print("Dep0: " + numToNode[i].GetName() + " " + dep0[i]);
                }

                GD.Print("Zero depth nodes for [" + 1 + "]:");
                for (int i = 0; i < numNodes; i++)
                {
                    GD.Print("Dep1: " + numToNode[i].GetName() + " " + dep1[i]);
                }

                Queue<Tuple<int, int>> q = new Queue<Tuple<int, int>>();
                q.Enqueue(new Tuple<int, int>(edgeNum, isDup));

                Boolean augSeqFound = false;

                while (q.Count > 0 && !augSeqFound)
                {
                    Tuple<int, int> f = q.Dequeue();

                    // GD.Print("=>=> Queue Popping Edge: " + numToEdge[f.Item1].Head().GetName() + " " + numToEdge[f.Item1].Tail().GetName() + " " + f.Item2);

                    int indexEntry = f.Item1 + f.Item2 * numToEdge.Count;
                    int tryIndex = 0;
                    if (index[indexEntry] != -1)
                    {
                        tryIndex = index[indexEntry] ^ 1;
                    }

                    Edge edge = numToEdge[f.Item1];
                    int u = nodeToNum[edge.Head()];
                    int v = nodeToNum[edge.Tail()];

                    // Go upwards from u and v while the edges aren't labelled.
                    // Push edges in a stack, then pop them and push them into the queue.
                    // Do this while labelling them with the current edge number.
                    if (partitions[tryIndex].Find(u) == partitions[tryIndex].Find(v))
                    {
                        GD.Print("Cycle!");
                        int[] endpoints = new int[] { u, v };
                        Stack<int> stk = new Stack<int>();
                        int[] par = (tryIndex == 0 ? par0 : par1);
                        int[] dep = (tryIndex == 0 ? dep0 : dep1);
                        while (endpoints[0] != endpoints[1])
                        {
                            int c = 0;
                            if (dep[endpoints[1]] > dep[endpoints[0]])
                            {
                                c = 1;
                            }
                            int anc = par[endpoints[c]];
                            if (label[anc] != -1)
                            {
                                int anco = par[endpoints[c ^ 1]];
                                while (anco != -1 && label[anco] == -1)
                                {
                                    // Pushing the edge onto the stack and moving up (and labelling).
                                    label[anco] = indexEntry;
                                    stk.Push(anco);

                                    int actualAnco = (anco >= numToEdge.Count ? anco - numToEdge.Count : anco);

                                    endpoints[c ^ 1] ^= nodeToNum[numToEdge[actualAnco].Head()]
                                ^ nodeToNum[numToEdge[actualAnco].Tail()];

                                    // // How was this working without this?!
                                    anco = par[endpoints[c ^ 1]];
                                }
                                break;
                            }

                            // Pushing the edge onto the stack and moving up (and labelling).
                            label[anc] = indexEntry;
                            stk.Push(anc);

                            int actualAnc = (anc >= numToEdge.Count ? anc - numToEdge.Count : anc);

                            endpoints[c] ^= nodeToNum[numToEdge[actualAnc].Head()]
                            ^ nodeToNum[numToEdge[actualAnc].Tail()];
                        }

                        while (stk.Count > 0)
                        {
                            int toPush = stk.Pop();
                            int blah = toPush;
                            if (blah >= numToEdge.Count)
                            {
                                blah -= toPush;
                            }
                            Edge qEdge = numToEdge[blah];
                            GD.Print("Queueing: " + qEdge.Tail().GetName() + " " + qEdge.Head().GetName());
                            if (toPush >= numToEdge.Count)
                            {
                                q.Enqueue(new Tuple<int, int>(toPush - numToEdge.Count, 1));
                            }
                            else
                            {
                                q.Enqueue(new Tuple<int, int>(toPush, 0));
                            }
                        }
                    }
                    else
                    {
                        // Add to the forest.
                        // Update the forests as per the augmenting swap sequence.
                        augSeqFound = true;
                        addedEdges++;
                        addedWeight += weight;
                        int curEdgeNum = indexEntry, addIndex = tryIndex;
                        while (curEdgeNum != -1)
                        {
                            // Add it into this forest.
                            index[curEdgeNum] = addIndex;
                            int sub = (curEdgeNum >= numToEdge.Count ? numToEdge.Count : 0);
                            partitions[addIndex].Unite(
                                nodeToNum[numToEdge[curEdgeNum - sub].Head()],
                                nodeToNum[numToEdge[curEdgeNum - sub].Tail()]
                            );

                            // Now go back on the swap sequence...
                            addIndex ^= 1;
                            curEdgeNum = label[curEdgeNum];
                        }
                    }
                }

                if (!augSeqFound)
                {
                    partitions[2].Unite(x, y);
                }
                GD.Print("---Forest-0---");
                for (int i = 0; i < index.Length; i++)
                {
                    if (index[i] == 0)
                    {
                        int j = i, xx = 0;
                        if (j >= numToEdge.Count)
                        {
                            xx = 1;
                            j -= numToEdge.Count;
                        }
                        GD.Print(numToEdge[j].Head().GetName() + " " + numToEdge[j].Tail().GetName() + " " + xx);
                    }
                }
                GD.Print("--------------");
                GD.Print("---Forest-1---");
                for (int i = 0; i < index.Length; i++)
                {
                    if (index[i] == 1)
                    {
                        int j = i, xx = 0;
                        if (j >= numToEdge.Count)
                        {
                            xx = 1;
                            j -= numToEdge.Count;
                        }
                        GD.Print(numToEdge[j].Head().GetName() + " " + numToEdge[j].Tail().GetName() + " " + xx);
                    }
                }
                GD.Print("==============");
            }
        }

        Edge returnEdge = null;
        if (addedEdges == 2 * (addedNodes - 1) && addedWeight <= 2 * (addedNodes - 1) + 1)
        {
            GD.Print("I can win!");
            computerCanWin = true;
            this.mask = mask;
            this.index = index;

            // Creating the forests.
            for (int i = 0; i < 2; i++)
            {
                forests[i] = RootGraph.CreateNew("Forest_" + i.ToString(), GraphType.Undirected);
                for (int j = 0; j < 2 * numToEdge.Count; j++)
                {
                    if (index[j] == i)
                    {
                        int safe_j = j - (j >= numToEdge.Count ? numToEdge.Count : 0);
                        Edge e = numToEdge[safe_j];
                        var x = forests[i].GetOrAddNode(e.Tail().GetName());
                        var y = forests[i].GetOrAddNode(e.Head().GetName());
                        forests[i].GetOrAddEdge(x, y, e.GetName());
                        if (j >= numToEdge.Count && !edgeFix[safe_j])
                        {
                            returnEdge = root.GetEdge(e.Tail(), e.Head(), e.GetName());
                        }
                    }
                }
            }
            int diff = 0;
            foreach (var node in forests[0].Nodes())
            {
                diff++;
            }
            foreach (var nod in forests[1].Nodes())
            {
                diff--;
            }
            GD.Print("Node count difference: " + diff);
        }

        return returnEdge;
    }

    private int[] GetParents(int mask, int[] index, int forestNum)
    {
        int[] par = new int[numNodes];
        for (int i = 0; i < numNodes; i++)
        {
            par[i] = -1;
        }
        for (int i = 0; i < numNodes; i++)
        {
            if (par[i] == -1 && ((1 << i) & mask) > 0)
            {
                DepthFirstSearch(i, par, mask, index, forestNum);
            }
        }
        return par;
    }

    private void DepthFirstSearch(int i, int[] par, int mask, int[] index, int forestNum)
    {
        foreach (var entry in adjList[i])
        {
            int j = entry.Item1;
            int edgeNum = entry.Item2;
            if (par[j] == -1 && ((1 << j) & mask) > 0)
            {
                if (index[edgeNum] == forestNum)
                {
                    par[j] = edgeNum;
                    DepthFirstSearch(j, par, mask, index, forestNum);
                }
                else if (index[edgeNum + numToEdge.Count] == forestNum)
                {
                    par[j] = edgeNum + numToEdge.Count;
                    DepthFirstSearch(j, par, mask, index, forestNum);
                }
            }
        }
    }

    private int[] GetDepths(int mask, int[] index, int forestNum)
    {
        int[] dep = new int[numNodes];
        for (int i = 0; i < numNodes; i++)
        {
            dep[i] = -1;
        }
        for (int i = 0; i < numNodes; i++)
        {
            if (dep[i] == -1 && ((1 << i) & mask) > 0)
            {
                dep[i] = 0;
                DepthFirstSearch2(i, dep, mask, index, forestNum);
            }
        }
        return dep;
    }

    private void DepthFirstSearch2(int i, int[] dep, int mask, int[] index, int forestNum)
    {
        foreach (var entry in adjList[i])
        {
            int j = entry.Item1;
            int edgeNum = entry.Item2;
            if (dep[j] == -1 && ((1 << j) & mask) > 0)
            {
                if (index[edgeNum] == forestNum)
                {
                    dep[j] = dep[i] + 1;
                    DepthFirstSearch2(j, dep, mask, index, forestNum);
                }
                else if (index[edgeNum + numToEdge.Count] == forestNum)
                {
                    dep[j] = dep[i] + 1;
                    DepthFirstSearch2(j, dep, mask, index, forestNum);
                }
            }
        }
    }

    private void FreezeButtons()
    {
        // GetTree().Root.SetProcessInput(false);
        // this.GetNode<Button>("BackButton").Disabled = true;
    }

    private void UnfreezeButtons()
    {
        // GetTree().Root.SetProcessInput(true);
        // this.GetNode<Button>("BackButton").Disabled = false;
    }

    class DisjointSetUnion
    {
        List<int> Parent = new List<int>(), Size = new List<int>();

        public DisjointSetUnion(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Parent.Add(i);
                Size.Add(1);
            }
        }

        public int Find(int x)
        {
            while (x != Parent[x])
            {
                x = Parent[x] = Parent[Parent[x]];
            }
            return x;
        }

        public void Unite(int x, int y)
        {
            x = Find(x);
            y = Find(y);
            if (x != y)
            {
                if (Size[x] < Size[y])
                {
                    int temp = y;
                    y = x;
                    x = temp;
                }
                Size[x] += Size[y];
                Parent[y] = x;
            }
        }

        public int ComponentSize(int x)
        {
            return Size[Find(x)];
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}