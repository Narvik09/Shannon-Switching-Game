using Godot;
using Rubjerg.Graphviz;
using System;

public class SetDualGraph : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public Global global;

    [Export(PropertyHint.File)]
    public string dualDotFilePath;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        dualDotFilePath = dualDotFilePath.Substring("res://".Length());
        GD.Print(dualDotFilePath);
        // RootGraph.FromDotFile("dotFiles/Level_4.dot");
        try
        {
            global.dualGraph = RootGraph.FromDotFile(dualDotFilePath);
        }
        catch (System.ArgumentException)
        {
            global.dualGraph = RootGraph.FromDotFile("dotFiles/Level_1_Dual.dot");
            // throw;
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
