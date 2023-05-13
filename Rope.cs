using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// The literal edge / rope
public class Rope : Node
{
    // list to store all rope segments
    public List<RigidBody2D> ropeSegments = new List<RigidBody2D>();

    public List<RopeSegment> allRopeSegments = new List<RopeSegment>();
    // each segment length
    const double segmentLength = 16.0;

    PackedScene RopeSegment = (PackedScene)ResourceLoader.Load("res://RopeSegment.tscn");

    public RigidBody2D endingSegmentEnd = null;

    public Random rand = new Random();

    public string startNodeLabel = null, endNodeLabel = null, edgeName = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // function to spawn a rope between two pinjoints
    // Pass in pinjoints of starting and ending nodes.
    public void SpawnRope(RigidBody2D startingSegmentEnd, RigidBody2D endingSegmentEnd, string startNodeLabel, string endNodeLabel, string edgeName)
    {
        Vector2 startPos = startingSegmentEnd.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").GlobalPosition;
        Vector2 endPos = endingSegmentEnd.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").GlobalPosition;

        this.endingSegmentEnd = endingSegmentEnd;
        this.startNodeLabel = startNodeLabel;
        this.endNodeLabel = endNodeLabel;
        this.edgeName = edgeName;

        // compute distance, number of segments and spawn angle
        double distance = startPos.DistanceTo(endPos);
        double randDistance = rand.NextDouble() * ((Math.PI / 2 - 1) * distance) + distance;
        // GD.Print("Distance  : " + randDistance.ToString());
        int numberOfSegments = (int)Math.Round(distance / segmentLength);
        // GD.Print("NumberOfSegments : " + numberOfSegments.ToString());
        float spawnAngle = (endPos - startPos).Angle() - (float)(Math.PI / 2);
        // GD.Print(spawnAngle);
        // create rope
        CreateRope(numberOfSegments, startingSegmentEnd, endPos, spawnAngle);
    }

    // function to create a rope made of rope segments
    public void CreateRope(int numberOfSegments, RigidBody2D parent, Vector2 endPos, float spawnAngle)
    {
        for (int i = 0; i < numberOfSegments; i++)
        {
            // add segment to the rope
            parent = AddSegment(parent, i, spawnAngle);
            parent.Name = "RopeSegment_" + i.ToString();
            // GD.Print(parent.Name, spawnAngle);
            ropeSegments.Add(parent);
            // to avoid rope segment going out of end position
            var jointPos = parent.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").GlobalPosition;
            if (jointPos.DistanceTo(endPos) < segmentLength)
            {
                break;
            }
        }
        // set end segment position
        var joint = new PinJoint2D();
        joint.NodeA = endingSegmentEnd.GetPath();
        joint.NodeB = ropeSegments.Last().GetPath();
        joint.DisableCollision = true;
        joint.Bias = (float)0.1;
        joint.Softness = (float)0.1;
        endingSegmentEnd.GetNode<CollisionShape2D>("CollisionShape2D").AddChild(joint);
        // endingSegmentEnd.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").NodeA = endingSegmentEnd.GetPath();
        // endingSegmentEnd.GetNode<PinJoint2D>("CollisionShape2D/PinJoint2D").NodeB = ropeSegments.Last().GetPath();
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
        allRopeSegments.Add(segment);
        var newJoint = new PinJoint2D();
        newJoint.NodeA = parent.GetPath();
        newJoint.NodeB = segment.GetPath();
        newJoint.DisableCollision = true;
        newJoint.Bias = (float)0.1;
        newJoint.Softness = (float)0.1;
        parent.GetNode<CollisionShape2D>("CollisionShape2D").AddChild(newJoint);
        return segment;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
