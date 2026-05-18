using Godot;
using System;

/* Implementações:
	- Pegar dados dos json gerados a partir do código python
	- Colocar dados obtidos no quaternion do esqueleto do modelo
	- Tocar animationplayer3d
	
	Após:
	-Tentar concatenar 2 animações no bruto
		_dando certo, começar a implementar b-spline (mudar pra slerp depois)
	-Incrementar script python no fluxo de execução do programa

*/


public partial class AnimationPlayer : Node3D
{

	[Export] public NodePath SkeletonPath;
	[Export(PropertyHint.File, ".json")] public string AnimationToBePlayed = ""; 
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
