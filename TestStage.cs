using Godot;
using System;



public class TestStage : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    PackedScene Rope = (PackedScene)ResourceLoader.Load("res://Rope.tscn");
    Vector2 startPos = Vector2.Zero;
    Vector2 endPos = Vector2.Zero;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Input function overriding
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton && !(@event.IsPressed()))
        {
            // get start position
            if (startPos == Vector2.Zero)
            {
                startPos = GetGlobalMousePosition();
            }
            // get end position
            else if (endPos == Vector2.Zero)
            {
                endPos = GetGlobalMousePosition();
                Rope rope = (Rope)Rope.Instance();
                AddChild(rope);
                // spawn rope after end position is set
                rope.SpawnRope(startPos, endPos);
                // reset start and end position
                startPos = Vector2.Zero;
                endPos = Vector2.Zero;
            }

        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
