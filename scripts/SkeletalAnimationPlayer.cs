using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SkeletalAnimationPlayer : Node3D
{
	[Export] public NodePath SkeletonPath;

	[Export(PropertyHint.File, ".json")]
	public string AnimationToBePlayed = "";
	private Skeleton3D skeleton;
	private AnimationData animation;
	private int currentFrame = 0;
	const float FRAME_TIME = 1f / 30f;
	private float timer = 0;

	public override void _Ready()
	{
		GD.Print("=== READY ===");

		skeleton = GetNode<Skeleton3D>(SkeletonPath);

		if (skeleton == null)
		{
			GD.PrintErr("Skeleton NOT FOUND");
			return;
		}

		GD.Print($"Skeleton loaded: {skeleton.Name}");

		LoadAnimation();
	}

	public override void _Process(double delta)
	{
		if (animation == null)
		{
			GD.Print("Animation is NULL");
			return;
		}

		timer += (float)delta;

		while (timer >= FRAME_TIME)
		{
			GD.Print($"Playing frame {currentFrame}");

			ApplyFrame(currentFrame);

			currentFrame++;

			if (currentFrame >= animation.FrameData.Count)
			{
				GD.Print("Looping animation");

				currentFrame = 0;
			}

			timer -= FRAME_TIME;
		}
	}

	void LoadAnimation()
	{
		GD.Print("=== LOADING JSON ===");

		if (!FileAccess.FileExists(AnimationToBePlayed))
		{
			GD.PrintErr($"File not found: {AnimationToBePlayed}");
			return;
		}

		string json =
			FileAccess.GetFileAsString(
				AnimationToBePlayed
			);

		GD.Print($"JSON length: {json.Length}");

		try
		{
			animation =
				JsonSerializer.Deserialize<AnimationData>(
					json
				);

			if (animation == null)
			{
				GD.PrintErr("Deserialize returned NULL");
				return;
			}

			GD.Print($"FrameCount={animation.FrameCount}");

			if (animation.FrameData == null)
			{
				GD.PrintErr("FrameData is NULL");
				return;
			}

			GD.Print(
				$"Loaded {animation.FrameData.Count} frames"
			);
		}
		catch (Exception e)
		{
			GD.PrintErr(e);
		}
	}

	void ApplyFrame(int FrameId)
	{
		GD.Print($"Applying frame {FrameId}");

		if (FrameId >= animation.FrameData.Count)
		{
			GD.PrintErr("Frame out of range");
			return;
		}

		FrameInfo frame = animation.FrameData[FrameId];

		// GD.Print($"Bone List: {string.Join(", ", frame.Bones.Keys)}");

		// GD.Print($"Bone Count: {frame.Bones.Count}");

		if (frame.Bones == null)
		{
			GD.PrintErr("Frame has NULL bones");
			return;
		}

		int applied = 0;

		foreach (var bone in frame.Bones)
		{
			string boneName =
				bone.Key;

			int id =
				skeleton.FindBone(
					boneName
				);

			if (id == -1)
			{
				GD.Print(
					$"Bone not found: {boneName}"
				);

				continue;
			}

			float[] q =
				bone.Value;

			if (q.Length != 4)
			{
				GD.PrintErr(
					$"Invalid quaternion {boneName}"
				);

				continue;
			}

			// GD.Print(
			// 	$"{boneName} -> [{q[0]}, {q[1]}, {q[2]}, {q[3]}]"
			// );

			Quaternion quat =
				new Quaternion(
					q[0],
					q[1],
					q[2],
					q[3]
				);

			Transform3D pose =
				skeleton.GetBonePose(
					id
				);

			pose.Basis =
				new Basis(quat);

			skeleton.SetBonePose(
				id,
				pose
			);

			applied++;
		}

		GD.Print(
			$"Applied {applied} bones"
		);
	}
}