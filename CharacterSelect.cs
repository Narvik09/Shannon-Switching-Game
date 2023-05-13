using Godot;
using System;

public class CharacterSelect : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public Global global;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
    }

    public void OnShortPlayerMouseEntered()
    {
        var animationPlayer = GetNode<AnimationPlayer>("MarginContainer/VBoxContainer/GridContainer/ShortPlayer/Sprite/AnimationPlayer");
        animationPlayer.Play("Chain");
    }

    public void OnPlayFirstPressed()
    {
        global.isPlayingFirst = GetNode<CheckButton>("PlayFirst").Pressed;
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
