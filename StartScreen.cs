using Godot;
using System;

public class StartScreen : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.GetNode<Button>("VBoxContainer/SinglePlayerButton").GrabFocus();
    }

    public void OnSinglePlayerButtonPressed()
    {

    }

    public void OnMultiPlayerButtonPressed()
    {

    }


    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
