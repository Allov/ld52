using Godot;
using System;
using System.Linq;

public class UI : CanvasLayer
{
	private bool UserPaused;
	private bool TutorialActive;
	[Export] public NodePath PlayAreaNode;
	private Label MoneyLabel;
	private PlayArea PlayArea;
	private Tween Tween;
	private int OldCoinsCount;
	private float Horizontal;

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MoneyLabel = GetNode<Label>("MoneyLabel");
		PlayArea = GetNode<PlayArea>(PlayAreaNode);
		Tween = new Tween();
		AddChild(Tween);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (TutorialActive && (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("ui_accept")))
		{
			CloseTutorial();
		}
		else if (Input.IsActionJustPressed("ui_cancel") && !GetTree().Paused)
		{
			Pause();
		}
		else if (Input.IsActionJustPressed("ui_cancel") && UserPaused)
		{
			Resume();
		}

		UpdateUI(delta);
	}

	private void UpdateUI(float delta)
	{
		if (OldCoinsCount != PlayArea.Player.GoldCoins)
		{
			OldCoinsCount = PlayArea.Player.GoldCoins;
			MoneyLabel.Text = $"$ {PlayArea.Player.GoldCoins}";
			Tween.InterpolateProperty(MoneyLabel, "rect_scale", Vector2.One, Vector2.One * 1.1f, .05f, Tween.TransitionType.Expo, Tween.EaseType.Out);
			Tween.InterpolateProperty(MoneyLabel, "rect_scale", Vector2.One * 1.1f, Vector2.One, .05f, Tween.TransitionType.Linear, Tween.EaseType.Out, .1f);
			Tween.Start();
		}

		Horizontal += delta;
		GetNode<ScrollContainer>("CalendarScroll").ScrollHorizontal = (int)Horizontal;
	}

	private void CloseTutorial()
	{
		TutorialActive = false;
		GetNode<Sprite>("Tutorial").Visible = false;
		GetNode<PanelContainer>("Paused").Visible = true;
	}

	private void Pause()
	{

		GetTree().Paused = true;
		UserPaused = true;
		GetNode<PanelContainer>("Paused").Visible = true;
		GetNode<AnimationPlayer>("Paused/AnimationPlayer").Play("fadein");
	}

	private void Resume()
	{
		GetTree().Paused = false;
		UserPaused = false;
		GetNode<PanelContainer>("Paused").Visible = false;
		GetNode<PanelContainer>("Paused").Modulate = Colors.Transparent;
	}

	public void _on_HowToPlay_pressed()
	{
		GetNode<PanelContainer>("Paused").Visible = false;
		GetNode<Sprite>("Tutorial").Visible = true;
		TutorialActive = true;
	}

	public void _on_Quit_pressed()
	{
		GetTree().Quit();
	}
	public void _on_Resume_pressed()
	{
		Resume();
	}
	
	public void _on_Restart_pressed()
	{
		GetTree().Paused = false;
		var angries = GetTree()
			.GetNodesInGroup("AngryPlants")
			.Cast<BaseAngry>();

		foreach (var angry in angries)
		{
			angry.QueueFree();
		}

		GetTree().ChangeScene("SplashScreen.tscn");
	}
}
