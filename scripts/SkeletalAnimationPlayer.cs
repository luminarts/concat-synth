using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
using System.Resources;
using System.Runtime.Serialization;
using System.Text.Json;

public partial class SkeletalAnimationPlayer : Node3D
{
	[Export] public NodePath SkeletonPath;
	[Export] public float AnimationSpeed = 1.0f;
		public List<string> Animations = new List<string>
	{
		"res://json_dictionary/SALADA_1.json",
		"res://json_dictionary/REPOLHO_1.json",
		"res://json_dictionary/JUNTO-COM_1.json",
		"res://json_dictionary/LARANJA_1.json"
	};

	private Skeleton3D skeleton;
	private AnimationData animation;
	private List<FrameInfo> transitionFrames = new();
	private bool playingTransition = false;
	private int transitionFrame = 0;
	private int currentFrame = 0;
	private int frameCount = 0;
	const float FRAME_TIME = 1f / 30f;
	private float timer = 0;
	private bool startPressed = false;
	private int currentAnimationIndex = 0;

	public override void _Ready()
	{
		GD.Print("INICIALIZANDO.......");
		// Button startButton = GetNode<Button>("CanvasLayer/Button");
		GD.Print("fafwkalfklw");

		// startButton.Pressed += OnButtonPressed;

		skeleton = GetNode<Skeleton3D>(SkeletonPath);

		GD.Print($"Skeleton loaded: {skeleton.Name}");
		
		LoadGloss();
	}

	/// <summary>
	/// Falta implementar:
	/// 	- UI com seleção da ordem de glosas para testes
	/// 	- Animação de repouso toda vez que tiver Idle
	/// 	
	/// </summary>
	/// <param name="delta"></param>
	public override void _Process(double delta)
	{
		// if (startPressed)
		// {
			if (animation == null)
			{
				GD.Print("Animation is NULL");
				return;
			}

			// isso aqui controla a velocidade de animação saindo do while mais rápido ou mais lento
			timer += (float)delta * AnimationSpeed;

			// aplica os dados de quaternion a cada osso naquele frame especificado, controlado pela variável timer
			while (timer >= FRAME_TIME)
			{
				if (playingTransition)
				{
					ApplyTransition();
				} else {
			
					GD.Print($"Playing frame {currentFrame}");
					
					ApplyFrame(currentFrame);
					currentFrame++;
					
					if (currentFrame >= animation.FrameData.Count)
					{
						
						FrameInfo prevlast = animation.FrameData[^1];
						
						LoadGloss();

						FrameInfo nextFirst = animation.FrameData[0];

						LoadTransition(prevlast, nextFirst);

						currentFrame = 0;
					}
				}

				timer -= FRAME_TIME;
			}
		// }
	}

	private void OnButtonPressed() 
	{
		startPressed = true;
		GD.Print("Started!");
	}

	void LoadGloss()
	{

		string animationPath = Animations[currentAnimationIndex];

		GD.Print("=== LOADING JSON ===");

		if (!FileAccess.FileExists(animationPath))
		{
			GD.PrintErr($"File not found: {animationPath}");
			return;
		}


		try
		{
			string json = FileAccess.GetFileAsString(animationPath);

			animation = JsonSerializer.Deserialize<AnimationData>(json);

			if (animation.FrameData == null)
			{
				GD.PrintErr("FrameData is NULL");
				return;
			}

			currentAnimationIndex++;

			if (currentAnimationIndex >= Animations.Count)
			{
				currentAnimationIndex = 0;
			}
		}
		catch (Exception e)
		{
			GD.PrintErr(e);
		}
	}

	void ApplyFrame(int FrameId)
	{
		GD.Print($"Applying frame {FrameId}");

		FrameInfo frame = animation.FrameData[FrameId];

		foreach (var bone in frame.Bones)
		{
			string boneName = bone.Key;

			int id = skeleton.FindBone(boneName);

			float[] q = bone.Value;

			// quaternion -> x, y , z, w
			Quaternion quat =
				new Quaternion(
					q[0],
					q[1],
					q[2],
					q[3]
				);

		Quaternion rest = skeleton.GetBoneRest(id).Basis.GetRotationQuaternion();

		// aplica o quaternion em relação à posição de referência (rest) do modelo
    skeleton.SetBonePoseRotation(id, rest * quat);

		}
	}
		void LoadTransition(FrameInfo prevLastFrame, FrameInfo nextFirstFrame)
	{
	/*
    1. Iterar a lista de animações escolhidas
		2. Pegar animationToBePlayed = lista[0]
		3. Fazer LoadGloss()
		4. Pegar o float[] animation.FrameData[animation.FrameCount - 1].Bones.Values
		5. Pegar 
  */
		transitionFrames.Clear();

		int numOfInterpolationFrames = 10;
		
		for (int i = 1; i <= numOfInterpolationFrames; i++)
		{
			float weight = i / (float) (numOfInterpolationFrames + 1);

			FrameInfo transitionFrame = new FrameInfo
			{
				FrameId = i,
				AdditionalData = new Dictionary<string, JsonElement>()
			};
		
			Dictionary<string, float[]> bones = new();

			foreach (var bone in prevLastFrame.Bones)
			{
				string boneName = bone.Key;

				float[] prev = bone.Value;
				float[] next = nextFirstFrame.Bones[boneName];

				Quaternion q1 = new Quaternion(
					prev[0],
					prev[1],
					prev[2],
					prev[3]
				);

				Quaternion q2 = new Quaternion(
					next[0],
					next[1],
					next[2],
					next[3]
				);

				Quaternion resultingQuat = q1.Slerp(q2, weight);

				bones[boneName] = [resultingQuat.X, resultingQuat.Y, resultingQuat.Z, resultingQuat.W];
			}

			transitionFrame.AdditionalData = ConvertToJsonElements(bones);
			transitionFrames.Add(transitionFrame);
		}

		transitionFrame = 0;
		playingTransition = true;
	}

	void ApplyTransition()
{
    if (transitionFrame >= transitionFrames.Count)
    {
        playingTransition = false;
        return;
    }

    FrameInfo frame = transitionFrames[transitionFrame];

    foreach (var bone in frame.Bones)
    {
        string boneName = bone.Key;

        int id = skeleton.FindBone(boneName);

        if (id == -1)
				{
					continue;
				}

        float[] q = bone.Value;

        Quaternion quat = new Quaternion(
                q[0],
                q[1],
                q[2],
                q[3]
            );

        Quaternion rest = skeleton.GetBoneRest(id).Basis.GetRotationQuaternion();

        skeleton.SetBonePoseRotation(id, rest * quat);
    }
    transitionFrame++;
}
	Dictionary<string, JsonElement> ConvertToJsonElements(Dictionary<string, float[]> bones)
	{
		var dict = new Dictionary<string, JsonElement>();

		foreach (var kv in bones)
		{
				dict[kv.Key] = JsonSerializer.SerializeToElement(kv.Value);
		}

		return dict;
	}
}