using System.Collections.Generic;
using System.Text.Json;
using Godot;

public class TransitionAnimation
{
  /*
- Calcular todas as transições entre glosas
						- Fazer LoadGloss() pra cada dupla adjacente
							- Inicialmente pegar o último dicionário (nome: quat) do primeiro, dar LoadGloss() pro segundo e pegar o primeiro frame do segundo
							- Após isso, converte os dois para Quaternions, e armazena o slerp entre os dois em uma List<> ou array
  */
	private List<FrameInfo> transitionFrames = new();
	private bool playingTransition = false;
	private int transitionFrame = 0;
	private Quaternion angularVelocity;
	SkeletalAnimationPlayer skeletalAnimationPlayer;
	void LoadTransition(FrameInfo prevLastFrame, FrameInfo nextFirstFrame)
	{
	/*
		TODO:
			- Implementar preservação de momentum (slerp(q1 * momentum, q2))

   */
		transitionFrames.Clear();

		int numOfInterpolationFrames = 5;
		
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

        int id = skeletalAnimationPlayer.skeleton.FindBone(boneName);

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

        Quaternion rest = skeletalAnimationPlayer.skeleton.GetBoneRest(id).Basis.GetRotationQuaternion();

        skeletalAnimationPlayer.skeleton.SetBonePoseRotation(id, rest * quat);
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