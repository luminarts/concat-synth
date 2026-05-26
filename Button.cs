using Godot;
using System;

public partial class Button : Godot.Button
{
	// Called when the node enters the scene tree for the first time.
	public Button button = new Button();
	public override void _Ready()
	{
		button.Pressed += ButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	private void ButtonPressed()
	{

	}
}
