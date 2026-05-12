using Godot;
using System;

public partial class AnimationPlayer : Node3D
{

	[Export] public NodePath SkeletonPath;
	[Export] public string AnimationToBePlayed;
	private Skeleton3D skeleton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
