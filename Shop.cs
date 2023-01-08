using Godot;
using System;
using System.Linq;

public class Shop : PanelContainer
{
    private const int NumberOfBuyableItems = 3;
    [Export] public NodePath PlayerNodePath;
    public Player Player;

    [Export] public NodePath PlayAreaNodePath;
    public PlayArea PlayArea;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Player = GetNode<Player>(PlayerNodePath);
        PlayArea = GetNode<PlayArea>(PlayAreaNodePath);
    }


    public void OpenShop()
    {
        GetTree().Paused = true;
        Visible = true;

        RollPerks(false);
    }

    public void Buy(int index)
    {
        var boughtPerk = BuyablePerks[index];
        boughtPerk.Effect(this);
        Player.GoldCoins = Player.GoldCoins - boughtPerk.Cost;
        Player.AddPerk(boughtPerk);

        if (boughtPerk.Unique)
        {
            Perks = Perks.Where(p => p != boughtPerk).ToArray();
        }

        CloseAndContinue();
    }

    private void CloseAndContinue()
    {
        GetTree().Paused = false;
        Visible = false;
    }

    private void RollPerks(bool spend)
    {
        if (spend)
        {
            Player.GoldCoins -= 50;
        }
        // pick 3 random perks
        BuyablePerks = Perks
            .PickN(NumberOfBuyableItems);

        for (var i = 0; i < NumberOfBuyableItems; i++)
        {
            var path = "ShopContainer/Items/Item" + i;

            GetNode<Label>(path + "/Name").Text = BuyablePerks[i].Name;
            GetNode<Button>(path + "/Buy").Text = $"$ {BuyablePerks[i].Cost}";
            GetNode<Button>(path + "/Buy").Disabled = Player.GoldCoins < BuyablePerks[i].Cost;
        }

        GetNode<Button>("ShopContainer/HBoxContainer/RerollItems").Disabled = Player.GoldCoins < 50;

    }

    public void _on_Continue_pressed()
    {
        CloseAndContinue();
    }

    public void _on_Buy_Item1_pressed()
    {
        Buy(0);
    }
    public void _on_Buy_Item2_pressed()
    {
        Buy(1);
    }
    public void _on_Buy_Item3_pressed()
    {
        Buy(2);
    }

    public void _on_RerollItems_pressed()
    {
        RollPerks(true);
    }

    public Perk[] Perks = new Perk[] {
        new Perk
        {
            Name = "More Shurikens (+1)",
            Cost = 250,
            Effect = (shop) => {
                shop.Player.ScythCount++;
            }
        },
        new Perk
        {
            Name = "Longer Shurikens (+1s)",
            Cost = 50,
            Effect = (shop) => {
                shop.Player.ShurikenLifeTime = shop.Player.ShurikenLifeTime + 1f;
            }

        },
        new Perk
        {
            Name = "Wider Shurikens (+25%)",
            Cost = 100,
            Effect = (shop) => {
                shop.Player.ShurikenSize = (int)((float)shop.Player.ShurikenSize * 1.25f);
            }

        },
        new Perk
        {
            Name = "Wider Planting (+2m)",
            Cost = 100,
            Effect = (shop) => {
                shop.Player.GrowSize();
            }

        },
        new Perk
        {
            Name = "Weed Killer",
            Cost = 500,
            Unique = true,
            Effect = (shop) => {
                shop.Player.WeedKiller();
            }

        },
        new Perk
        {
            Name = "Speed Increase (+10%)",
            Cost = 100,
            Unique = true,
            Effect = (shop) => {
                shop.Player.speed = shop.Player.speed * 1.1f;
            }

        },
        new Perk
        {
            Name = "Dash Longer (+10%)",
            Cost = 100,
            Unique = true,
            Effect = (shop) => {
                shop.Player.dashDuration = shop.Player.dashDuration * 1.10f;
            }

        },
        // new Perk
        // {
        //     Id = "farmer",
        //     Name = "Farmer (+1 crop stage)",
        //     Cost = 500,
        //     Effect = (shop) => {
        //         // todo
        //     }
        // },
        new Perk
        {
            Name = "Crop Gold (+5 gold per)",
            Unique = true,
            Cost = 100,
            Effect = (shop) => {
                // todo
            }
        },
        new Perk
        {
            Name = "Cut Weed is Fertilized",
            Cost = 5000,
            Unique = true,
            Effect = (shop) => {
                // noop
            }
        },
        // new Perk
        // {
        //     Name = "Cut Weed is Fertilized x2",
        //     Cost = 5000,
        //     Unique = true,
        //     Effect = (shop) => {
        //         // noop
        //     }

        // },
        // new Perk
        // {
        //     Name = "Cut Weed becomes corn",
        //     Cost = 10000,
        //     Unique = true,
        //     Effect = (shop) => {
        //         // noop
        //     }

        // },
        // new Perk
        // {
        //     Name = "Cut Weed becomes gold",
        //     Cost = 20000,
        //     Unique = true,
        //     Effect = (shop) => {
        //         // noop
        //     }

        // },
    };
    private Perk[] BuyablePerks;
}


public class Perk
{
    public string Id;
    public string Name;
    public int Cost;
    public Action<Shop> Effect;
    public bool Unique;
}
