using Godot;
using System;

public class Global : Node
{
    public Boolean isCutPlaying = true;

    public Boolean isSinglePlayer = false;

    public Boolean isCutComputer = false;

    public Boolean isShortComputer = false;

    public Boolean isPlayingFirst = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
