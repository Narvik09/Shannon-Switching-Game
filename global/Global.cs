using Godot;
using System;
using Rubjerg.Graphviz;

public class Global : Godot.Node
{
    public Boolean isCutPlaying = true;

    public Boolean isShortPlaying = false;

    public Boolean isSinglePlayer = false;

    public Boolean isCutComputer = false;

    public Boolean isShortComputer = false;

    public Boolean isPlayingFirst = true;

    public RootGraph rootGraph, dualGraph;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public void SetBooleans()
    {
        isCutPlaying = !isShortPlaying;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
