using Godot;
using System;
using Rubjerg.Graphviz;
using System.Collections.Generic;

using GNode = Rubjerg.Graphviz.Node;
using System.Threading.Tasks;

public class RopeSegment : RigidBody2D
{

    // Each rope segment has an Id and a parent rope segment.
    public int ID = 0;

    public RigidBody2D Parent = null;

    public Global global;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
    }

    private void cutPlay(Rope rope, TestStage testStage)
    {
        RemoveEdgeWithSegment();
    }

    private void shortPlay(Rope rope, TestStage testStage)
    {
        ReinforceEdgeWithSegment();
    }

    // Helper function to indicate the player that needs to make a move.
    public void ShiftPlayerHighlight(TestStage testStage)
    {
        // Highlight Cut if it's Cut's turn.
        if (global.isCutPlaying)
        {
            var material = testStage.startNodeSegmentEnd.GetNode<Sprite>("ShortPlayer/Sprite").Material;
            (material as ShaderMaterial).SetShaderParam("aura_width", 0);
            material = testStage.endNodeSegmentEnd.GetNode<Sprite>("CutPlayer/Sprite").Material;
            (material as ShaderMaterial).SetShaderParam("aura_width", 1);
        }
        else
        {
            var material = testStage.startNodeSegmentEnd.GetNode<Sprite>("ShortPlayer/Sprite").Material;
            (material as ShaderMaterial).SetShaderParam("aura_width", 1);
            material = testStage.endNodeSegmentEnd.GetNode<Sprite>("CutPlayer/Sprite").Material;
            (material as ShaderMaterial).SetShaderParam("aura_width", 0);
        }
    }

    private async void RopeSegmentClicked()
    {
        // TODO: Maybe highlight the entire rope.
        Rope rope = this.GetParent<Rope>();
        TestStage testStage = (TestStage)rope.GetParent<TestStage>();

        if (global.isCutPlaying)
        {
            cutPlay(rope, testStage);
        }
        else
        {
            shortPlay(rope, testStage);
        }

        if (CheckForWinners(testStage))
        {
            return;
        }

        global.isCutPlaying = !global.isCutPlaying;
        ShiftPlayerHighlight(testStage);

        // Here just to make the highlight visible for now.
        await Task.Delay(TimeSpan.FromMilliseconds(3000));

        // End if multiplayer.
        if (!global.isSinglePlayer)
        {
            return;
        }

        // Otherwise, the computer moves.
        if (!global.isCutPlaying && global.isShortComputer)
        {
            testStage.MoveShort();
        }
        else if (global.isCutPlaying && global.isCutComputer)
        {
            testStage.MoveCut();
        }

        if (CheckForWinners(testStage))
        {
            return;
        }

        global.isCutPlaying = !global.isCutPlaying;
        ShiftPlayerHighlight(testStage);
    }

    private Boolean CheckForWinners(TestStage testStage)
    {
        if (DidCutWin(testStage.root))
        {
            testStage.GetNode<Label>("Node2D/Label").Text = "Cut has won!";
            testStage.GetNode<Label>("Node2D/Label").Show();
            GD.Print("Cut has won!");
            return true;
        }
        if (DidShortWin(testStage.root))
        {
            testStage.GetNode<Label>("Node2D/Label").Text = "Short has won!";
            testStage.GetNode<Label>("Node2D/Label").Show();
            GD.Print("Short has won!");
            return true;
        }
        return false;
    }

    private async void ReinforceEdgeWithSegment()
    {
        Rope rope = this.GetParent<Rope>();

        string startNodeLabel = rope.startNodeLabel;
        string endNodeLabel = rope.endNodeLabel;
        string edgeName = rope.edgeName;

        TestStage testStage = (TestStage)rope.GetParent<TestStage>();
        Rubjerg.Graphviz.Node startNode = testStage.root.GetNode(startNodeLabel);
        Rubjerg.Graphviz.Node endNode = testStage.root.GetNode(endNodeLabel);

        Edge edge = testStage.root.GetEdge(endNode, startNode, edgeName);
        edge.SafeSetAttribute("fixed", "true", "false");

        var animationPlayer = testStage.startNodeSegmentEnd.GetNode<AnimationPlayer>("ShortPlayer/Sprite/AnimationPlayer");
        animationPlayer.Play("Chain");
        await Task.Delay(TimeSpan.FromMilliseconds(1000));

        foreach (RigidBody2D ropeSegment in rope.ropeSegments)
        {
            ropeSegment.GetNode<Sprite>("WoodenBridge/Smoke").Visible = true;
            animationPlayer = ropeSegment.GetNode<AnimationPlayer>("WoodenBridge/Smoke/AnimationPlayer");
            animationPlayer.Play("Change");
        }
        await Task.Delay(TimeSpan.FromMilliseconds(400));

        foreach (RigidBody2D ropeSegment in rope.ropeSegments)
        {
            // Hide wooden bridge
            ropeSegment.GetNode<Sprite>("WoodenBridge").Visible = false;
            // Make button unclickable.
            ropeSegment.GetNode<Button>("Button").Disabled = true;
            // Change the sprite to chains.
            ropeSegment.GetNode<Sprite>("MetalBridge").Visible = true;
            // ropeSegment.Mode = ModeEnum.Static;
        }
    }

    private async void RemoveEdgeWithSegment()
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

        for (int i = 0; i < rope.ropeSegments.Count; i++)
        {
            if (rope.ropeSegments[i] == this)
            {
                this.QueueFree();
                rope.ropeSegments[i] = null;
                break;
            }
        }

        var animationPlayer = testStage.endNodeSegmentEnd.GetNode<AnimationPlayer>("CutPlayer/Sprite/AnimationPlayer");
        animationPlayer.Play("Fire");
        await Task.Delay(TimeSpan.FromMilliseconds(1000));

        // TODO : figure out delay in animation player
        foreach (RigidBody2D ropeSegment in rope.ropeSegments)
        {
            if (ropeSegment == null)
            {
                continue;
            }
            ropeSegment.GetNode<Sprite>("WoodenBridge/Fire").Visible = true;
            animationPlayer = ropeSegment.GetNode<AnimationPlayer>("WoodenBridge/Fire/AnimationPlayer");
            animationPlayer.Play("Burn");
        }
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        foreach (RigidBody2D ropeSegment in rope.ropeSegments)
        {
            if (ropeSegment == null)
            {
                continue;
            }
            animationPlayer = ropeSegment.GetNode<AnimationPlayer>("WoodenBridge/Fire/AnimationPlayer");
            animationPlayer.Play("Fade");
        }
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        foreach (RigidBody2D ropeSegment in rope.ropeSegments)
        {
            if (ropeSegment == null)
            {
                continue;
            }
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
                // GD.Print(edge.Tail().GetName() + " " + edge.Head().GetName());
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

        // foreach (GNode x in visited)
        // {
        //     GD.Print(x.GetName());
        // }

        return visited.Contains(graph.GetNode("End"));
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
