using Godot;
using System;
using System.Linq;

public class UI : CanvasLayer
{
    private bool UserPaused;
    private bool TutorialActive;
    [Export] public NodePath PlayAreaNode;
    private Label MoneyLabel;
    private Label CropsLabel;
    private Label StatsLabel;
    private PlayArea PlayArea;
    private Shop Shop;
    private Tween Tween;
    private int OldCoinsCount;
    private float Horizontal;
    private bool Restart;
    private bool Restarting;
    private Sprite CurrentCursor;
    private bool SettingsActive;
    private int DEBUG_AddedGoldValue = 100;

    static string GetOrdinalNumber(int number)
    {
        if (number >= 11 && number <= 13)
        {
            return "th"; // Special case for 11th, 12th, and 13th
        }
        else
        {
            int lastDigit = number % 10;
            switch (lastDigit)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }
    }

    public override void _Ready()
    {
        MoneyLabel = GetNode<Label>("MoneyLabel");
        CropsLabel = GetNode<Label>("HarvestCounter");
        StatsLabel = GetNode<Label>("PlayerStats");
        PlayArea = GetNode<PlayArea>(PlayAreaNode);
        Shop = GetNode<Shop>("Shop");
        Tween = new Tween();
        AddChild(Tween);

        SetCursor(true, "CursorA");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Restart)
        {
            RestartMap();
            return;
        }

        if (TutorialActive && (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("ui_accept")))
        {
            CloseTutorial();
        }
        else if (SettingsActive && (Input.IsActionJustPressed("ui_cancel")))
        {
            CloseSettings();
        }
        else if (Input.IsActionJustPressed("ui_pause") && !GetTree().Paused)
        {
            Pause();
        }
        else if (Input.IsActionJustPressed("ui_pause") && UserPaused)
        {
            Resume();
        }

        if (Input.IsActionJustPressed("DEBUG_open_shop"))
        {
            Shop.OpenShop();
        }

        if (Input.IsActionJustPressed("DEBUG_give_gold"))
        {
            PlayArea.Player.GoldCoins += DEBUG_AddedGoldValue;
             DEBUG_AddedGoldValue += DEBUG_AddedGoldValue * 10;
        }

        UpdateUI(delta);
    }

    private void UpdateUI(float delta)
    {
        if (OldCoinsCount != PlayArea.Player.GoldCoins)
        {
            // MoneyLabel.Text = $"$ {PlayArea.Player.GoldCoins}";
            var grow = 1.3f;
            Tween.InterpolateProperty(MoneyLabel, "rect_scale", Vector2.One, Vector2.One * grow, .05f, Tween.TransitionType.Expo, Tween.EaseType.Out);
            Tween.InterpolateProperty(MoneyLabel, "rect_scale", Vector2.One * grow, Vector2.One, .05f, Tween.TransitionType.Linear, Tween.EaseType.Out, .1f);
            Tween.InterpolateMethod(this, nameof(UpdateMoneyLabel), OldCoinsCount, PlayArea.Player.GoldCoins, .5f);
            Tween.Start();

            OldCoinsCount = PlayArea.Player.GoldCoins;
        }

        GetNode<Label>("CalendarLabel").Text = $"{PlayArea.NumberOfDays}{GetOrdinalNumber(PlayArea.NumberOfDays)} day";
        CropsLabel.Text = $"C:{PlayArea.Player.HarvestedCropsCount.ToString("n0")}\nW:{PlayArea.Player.HavestedWeedCount.ToString("n0")}";

        StatsLabel.Text = $"SHOOT: {PlayArea.Player.ShootTime}s      DASH: {PlayArea.Player.DashCooldownTime}s    SIZE: {PlayArea.Player.Size}    STIM: {PlayArea.Player.ShurikenLifeTime}s";
        StatsLabel.Text += $"\n";
        StatsLabel.Text += $"SPEED: {PlayArea.Player.Speed}m/s    SHUR: {PlayArea.Player.ScythCount}       SSIZ: {PlayArea.Player.ShurikenSize}    THRN: {PlayArea.Player.ThornTimer}s";
    }

    private void UpdateMoneyLabel(int coins)
    {
        MoneyLabel.Text = $"{coins.ToString("n0")}$";
    }

    private void CloseTutorial()
    {
        TutorialActive = false;
        GetNode<TextureRect>("Tutorial").Visible = false;
        GetNode<PanelContainer>("Paused").Visible = true;
    }

    private void CloseSettings()
    {
        SettingsActive = false;
        GetNode<PanelContainer>("Settings").Visible = false;
        GetNode<PanelContainer>("Paused").Visible = true;
    }

    private void Pause()
    {
        if (Input.MouseMode == Input.MouseModeEnum.Confined)
        {
            Input.MouseMode = Input.MouseModeEnum.Hidden;
        }

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
        GetNode<TextureRect>("Tutorial").Visible = true;
        TutorialActive = true;
    }

    public void _on_Settings_pressed()
    {
        var masterIndex = AudioServer.GetBusIndex("Master");
        GetNode<HSlider>("Settings/2Cols/Col1/Audio/VBoxContainer/MasterVolume/MasterVolumeSlider").Value = GD.Db2Linear(AudioServer.GetBusVolumeDb(masterIndex));
        var musicIndex = AudioServer.GetBusIndex("Music");
        GetNode<HSlider>("Settings/2Cols/Col1/Audio/VBoxContainer/MusicVolume/MusicVolumeSlider").Value = GD.Db2Linear(AudioServer.GetBusVolumeDb(musicIndex));
        var sfxIndex = AudioServer.GetBusIndex("SFX");
        GetNode<HSlider>("Settings/2Cols/Col1/Audio/VBoxContainer/SFXVolume/SFXVolumeSlider").Value = GD.Db2Linear(AudioServer.GetBusVolumeDb(sfxIndex));

        GetNode<PanelContainer>("Paused").Visible = false;
        GetNode<PanelContainer>("Settings").Visible = true;
        SettingsActive = true;
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
        Restart = true;
    }

    public void RestartMap()
    {
        if (Restarting) return;

        Restarting = true;
        GetTree().Paused = false;
        GetNode<SceneTransition>("/root/SceneTransition").ChangeScene("Main.tscn");
    }

    public void _on_ColorPickerButton_color_changed(Color color)
    {
        // GetNode<HBoxContainer>("Settings/2Cols/Col1/Options/VBoxContainer/Cursor/Cursors").Modulate = color;
        GetNode<Sprite>("CursorA").Modulate = color;
        GetNode<Sprite>("CursorB").Modulate = color;
        GetNode<Sprite>("CursorC").Modulate = color;
        GetNode<Sprite>("CursorD").Modulate = color;
    }

    public void _on_CursorA_toggled(bool pressed)
    {
        SetCursor(pressed, "CursorA");
    }

    private void SetCursor(bool pressed, string name)
    {
        if (pressed)
        {
            if (CurrentCursor != null)
            {
                CurrentCursor.Visible = false;
            }

            CurrentCursor = GetNode<Sprite>(name);
            CurrentCursor.Visible = true;
        }
    }

    public void _on_CursorB_toggled(bool pressed)
    {
        SetCursor(pressed, "CursorB");
    }
    public void _on_CursorC_toggled(bool pressed)
    {
        SetCursor(pressed, "CursorC");
    }
    public void _on_CursorD_toggled(bool pressed)
    {
        SetCursor(pressed, "CursorD");
    }
    public void _on_LockCursor_toggled(bool pressed)
    {
        PlayArea.LockMouseToWindow = pressed;
    }
}
