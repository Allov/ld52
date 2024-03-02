using Godot;
using System;

public class Main : Node2D
{
	public override void _Ready()
	{
		OS.WindowMaximized = true;
	}
}
