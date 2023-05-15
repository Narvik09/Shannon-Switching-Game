using Godot;
using System;

public class SegmentEnd : RigidBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public void PlayAnimations(bool isCutPlaying)
    {
        if (isCutPlaying)
        {
            this.GetNode<AnimatedSprite>("CutPlayer/AnimatedSprite").Visible = true;
            this.GetNode<Sprite>("CutPlayer/Sprite").Visible = false;
        }
        else
        {
            var animatedSprite = this.GetNode<AnimatedSprite>("ShortPlayer/Sprite");
            animatedSprite.Animation = "idle";
            var animationPlayer = this.GetNode<AnimationPlayer>("ShortPlayer/Sprite/AnimationPlayer");
            animationPlayer.Play("Idle");
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
