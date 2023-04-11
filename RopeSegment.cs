using Godot;
using System;

public class RopeSegment : RigidBody2D
{

    // Each rope segment has an Id and a parent rope segment.
    public int ID = 0;
    public RigidBody2D Parent = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    private void RopeSegmentClicked()
    {
        // TODO: Maybe highlight the entire rope.
        // If Cut plays, remove the edge (delete from the adjacency list too somehow).
        // If Short plays, reinforce the edge (change the sprites, update the adjacency list too somehow).
        Rope rope = this.GetParent<Rope>();
        GD.Print("Selected rope with number: " + rope.EdgeNumber + "!");
        this.QueueFree();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
