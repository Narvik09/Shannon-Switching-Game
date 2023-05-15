using Godot;
using Rubjerg.Graphviz;
using System;

public class SetRootGraph : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public Global global;

    [Export(PropertyHint.File)]
    public string rootDotFilePath;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        rootDotFilePath = rootDotFilePath.Substring("res://".Length());
        global.rootGraph = RootGraph.FromDotFile(rootDotFilePath);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
