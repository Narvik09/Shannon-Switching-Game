using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Rope : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // list to store all rope segments
    List<RigidBody2D> ropeSegments = new List<RigidBody2D>();
    // each segment length
    const double segmentLength = 4.0;

    PackedScene RopeSegment = (PackedScene)ResourceLoader.Load("res://RopeSegment.tscn");

    public RigidBody2D startSegment;
    public RigidBody2D endSegment;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        startSegment = GetNode<RigidBody2D>("StartSegment");
        endSegment = GetNode<RigidBody2D>("EndSegment");
    }

    // function to spawn a rope between two positions
    public void SpawnRope(Vector2 startPos, Vector2 endPos)
    {
        // set start and end position
        startSegment.GlobalPosition = startPos;
        endSegment.GlobalPosition = endPos;
        // get exact position of the joints
        startPos = startSegment.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").GlobalPosition;
        endPos = endSegment.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").GlobalPosition;
        // compute distance, number of segments and spawn angle
        double distance = startPos.DistanceTo(endPos);
        int numberOfSegments = (int)Math.Round(distance / segmentLength);
        float spawnAngle = (endPos - startPos).Angle() - (float)(Math.PI / 2);
        // create rope
        CreateRope(numberOfSegments, startSegment, endPos, spawnAngle);
        // test
        GD.Print(numberOfSegments);
        GD.Print(ropeSegments.Count);
        // for (int i = 0; i < ropeSegments.Count; i++)
        // {
        //     RemoveChild(ropeSegments[i]);
        // }
    }

    // function to create a rope made of rope segments
    public void CreateRope(int numberOfSegments, RigidBody2D parent, Vector2 endPos, float spawnAngle)
    {
        for (int i = 0; i < numberOfSegments; i++)
        {
            // add segment to the rope
            parent = AddSegment(parent, i, spawnAngle);
            parent.Name = "RopeSegment_" + i.ToString();
            ropeSegments.Add(parent);
            // to avoid rope segment going out of end position
            var jointPos = parent.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").GlobalPosition;
            if (jointPos.DistanceTo(endPos) < segmentLength)
            {
                break;
            }
        }
        // set end segment position
        endSegment.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").NodeA = endSegment.GetPath();
        endSegment.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").NodeB = ropeSegments.Last().GetPath();
    }

    // function to add a rope segment to the rope
    public RopeSegment AddSegment(RigidBody2D parent, int id, float spawnAngle)
    {
        var joint = parent.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D");
        var segment = (RopeSegment)RopeSegment.Instance();
        segment.GlobalPosition = joint.GlobalPosition;
        segment.Rotation = spawnAngle;
        segment.Parent = parent;
        segment.ID = id;
        AddChild(segment);
        joint.NodeA = parent.GetPath();
        joint.NodeB = segment.GetPath();
        return segment;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
