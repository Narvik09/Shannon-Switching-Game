using Godot;
using System;

public class SceneTransition : CanvasLayer
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public async void ChangeScene(string target)
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Dissolve");
        await ToSignal(GetTree().CreateTimer((float)0.5), "timeout");
        GetTree().ChangeScene(target);
        GetNode<AnimationPlayer>("AnimationPlayer").PlayBackwards("Dissolve");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
