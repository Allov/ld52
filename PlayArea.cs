using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayArea : Node2D
{
    [Export] public int Width = 384 / 2;
    [Export] public int Height = 216 / 2;
    [Export] public int TileSize = 2;
    [Export] public PackedScene TileScene;
    private Cooldown GrowCooldown;
    [Export] public float GrowTime = 5f;

    public Dictionary<int, Tile> Field = new Dictionary<int, Tile>();
    private List<Tile> GrassTiles;
    private List<Tile> FertilzedTiles;
    private List<Tile> DirtTiles;
    private List<Tile> CornTiles;
    private List<Tile> WeedTiles;
    private int NumberOfDays;
    [Export] public NodePath DaysLabelNodePath;
    public Label DaysLabel;
    private Player Player;
    [Export] public float SpawnFactor = 2f / 75f; // about 33% of the field would spawn as weed at day 60
    [Export] public int MaxNumberOfDays = 60;

    [Export] public NodePath PlayerNodePath;
    [Export] public NodePath ShopNodePath;
    public Shop Shop;

    [Export] public PackedScene AngryPlantScene;
    [Export] public PackedScene AngryCornScene;
    private int MadnessLevel;
    [Export] public PackedScene AngryTomatoScene;
    [Export] public PackedScene AngryEggplantScene;
    private bool Day15Done;
    private bool TriggerEndOfGame;
    [Export] public NodePath GameOverNodePath;
    public PanelContainer GameOverPanel;
    private bool UserPaused;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DaysLabel = GetNode<Label>(DaysLabelNodePath);
        Player = GetNode<Player>(PlayerNodePath);
        Shop = GetNode<Shop>(ShopNodePath);
        GameOverPanel = GetNode<PanelContainer>(GameOverNodePath);

        var tween = new Tween();
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                // if (RandomHelpers.DrawResult(10)) continue;
                var tile = TileScene.Instance<Tile>();
                tile.GlobalPosition = new Vector2(x * TileSize, y * TileSize);
                tile.Scale = Vector2.Zero;

                tween.InterpolateProperty(tile, "scale", Vector2.Zero, Vector2.One, .9f, Tween.TransitionType.Linear, Tween.EaseType.In, (float)(x + y) * .032f);
                AddChild(tile);

                // if (RandomHelpers.DrawResult(2))
                // {
                // 	tile.ChangeGroup("Fertilized");
                // }
                if ((RandomHelpers.DrawResult(100)))
                {
                    tile.ChangeGroup("Corn");
                }
                // else if ((RandomHelpers.DrawResult(50)))
                // {
                // 	tile.ChangeGroup("Grass");
                // }
                else
                {
                    tile.ChangeGroup("Dirt");
                }
                // else if (RandomHelpers.DrawResult(3))
                // {
                //     tile.Modulate = Color.ColorN("Blue", 1);
                // }

                tile.PositionIndex = Width * y + x;
                tile.X = x;
                tile.Y = y;

                Field.Add(tile.PositionIndex, tile);
            }
        }

        AddChild(tween);
        tween.Start();

        GrowCooldown = new Cooldown(GrowTime, this);
        GrowCooldown.Use();

        UpdateUi();
    }

    private void UpdateUi()
    {
        DaysLabel.Text = $"Day: {NumberOfDays} Crops: {Player.HarvestedCropsCount} Weeds: {Player.HavestedWeedCount} $ {Player.GoldCoins} Thorned: {Player.ThornTimer > 0f}";
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        if (TriggerEndOfGame)
        {
            GetTree().Paused = true;
            GameOverPanel.Visible = true;
        }

        if (GrowCooldown.Use())
        {
            NumberOfDays++;
            GrassTiles = GetTree().GetNodesInGroup("Grass").Cast<Tile>().ToList();
            FertilzedTiles = GetTree().GetNodesInGroup("Fertilized").Cast<Tile>().ToList();
            DirtTiles = GetTree().GetNodesInGroup("Dirt").Cast<Tile>().ToList();
            CornTiles = GetTree().GetNodesInGroup("Corn").Cast<Tile>().ToList();
            WeedTiles = GetTree().GetNodesInGroup("Weed").Cast<Tile>().ToList();

            GrowCrops();
            GrowWeeds();
            SpawnWeeds();
            SpawnAngries();


            if (NumberOfDays > 0 && NumberOfDays % 4 == 0)
            {
                Shop.OpenShop();
            }
            else
            {
                Shop.Visible = false;
            }
        }


        UpdateUi();
    }

    private void SpawnAngries()
    {
        var weedRatio = (float)WeedTiles.Count / (float)Field.Count;
        if (NumberOfDays == 5)
        {
            SpawnAngryPlant();
        }

        if (NumberOfDays == 10)
        {
            SpawnAngryPlant();
            SpawnAngryPlant();
        }

        if (!Day15Done && (NumberOfDays == 15 || weedRatio > .7f))
        {
            Day15Done = true;
            // for (var i = 0; i < 1; i++)
            // {
            //     SpawnAngryPlant();
            // }

            SpawnAngryCorn();
        }

        else if (NumberOfDays >= 20 && NumberOfDays % 5 == 0)
        {
            MadnessLevel++;
            for (var i = 0; i < Math.Max(3, MadnessLevel * 2); i++)
            {
                SpawnAngryPlant();
            }

            for (var i = 0; i < Math.Max(2, MadnessLevel); i++)
            {
                SpawnAngryCorn();
            }

            if (MadnessLevel >= 3)
            {
                SpawnAngryTomato();
            }

            if (MadnessLevel >= 5)
            {
                SpawnAngryEggplant();
            }
        }
    }

    private void SpawnAngryPlant()
    {
        var angryPlant = AngryPlantScene.Instance<AngryPlant>();
        angryPlant.GlobalPosition = new Vector2(RandomHelpers.RangeInt(100, 300), RandomHelpers.RangeInt(50, 150));
        GetTree().Root.AddChild(angryPlant);
    }

    private void SpawnAngryTomato()
    {
        var angryTomato = AngryTomatoScene.Instance<AngryTomato>();
        angryTomato.GlobalPosition = new Vector2(RandomHelpers.RangeInt(100, 300), RandomHelpers.RangeInt(50, 150));
        GetTree().Root.AddChild(angryTomato);
    }

    private void SpawnAngryEggplant()
    {
        var angryEggplant = AngryEggplantScene.Instance<AngryEggplant>();
        angryEggplant.GlobalPosition = new Vector2(RandomHelpers.RangeInt(100, 300), RandomHelpers.RangeInt(50, 150));
        GetTree().Root.AddChild(angryEggplant);
    }

    private void SpawnAngryCorn()
    {
        var angryCorn = AngryCornScene.Instance<AngryCorn>();
        angryCorn.GlobalPosition = new Vector2(RandomHelpers.RangeInt(100, 300), RandomHelpers.RangeInt(50, 150));
        GetTree().Root.AddChild(angryCorn);
    }

    private void SpawnWeeds()
    {
        var numberOfSpawns = SpawnFactor * ((float)NumberOfDays * (float)NumberOfDays) + 1f;

        for (var i = 0; i < numberOfSpawns; i++)
        {
            if (DirtTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, DirtTiles.Count);
                DirtTiles[randomIndex].ChangeGroup("Weed");
            }
            else if (FertilzedTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, FertilzedTiles.Count);
                FertilzedTiles[randomIndex].ChangeGroup("Weed");
            }
            else if (GrassTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, GrassTiles.Count);
                GrassTiles[randomIndex].ChangeGroup("Weed");
            }
            else if (CornTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, CornTiles.Count);
                CornTiles[randomIndex].ChangeGroup("Weed");
            }
            else
            {
                TriggerEndOfGame = true;
            }
        }
    }

    private void GrowCrops()
    {
        foreach (var tile in GrassTiles)
        {
            if (RandomHelpers.DrawResult(3))
            {
                tile.ChangeGroup("Corn", true);
            }
        }

        foreach (var tile in FertilzedTiles)
        {
            if (RandomHelpers.DrawResult(3))
            {
                tile.ChangeGroup("Grass", true);
            }
        }
    }

    private void GrowWeeds()
    {
        var neighbors = new int[4];
        var spawnedAngriesCount = 0;
        foreach (var tile in WeedTiles)
        {
            tile.Stage++;
            neighbors[0] = Width * (tile.Y - 1) + tile.X;
            neighbors[1] = Width * (tile.Y + 1) + tile.X;
            neighbors[2] = Width * (tile.Y) + tile.X + 1;
            neighbors[3] = Width * (tile.Y) + tile.X - 1;

            for (var i = 0; i < 4; i++)
            {
                if (neighbors[i] >= 0 && neighbors[i] < Field.Count)
                {
                    Field[neighbors[i]].ChangeGroup("Weed", true);
                }
            }

            tile.ModulateFromGroup();

            if (spawnedAngriesCount < 3 && tile.Stage == 4)
            {
                var angryPlant = AngryPlantScene.Instance<AngryPlant>();
                angryPlant.GlobalPosition = tile.GlobalPosition;

                GetTree().Root.AddChild(angryPlant);
                spawnedAngriesCount++;
            }
        }
    }
}

