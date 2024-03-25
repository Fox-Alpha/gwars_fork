using Godot;
using System;

public partial class Map : Node2D
{
    private NetworkManager networkManager = null;
    public NetworkManager NetworkManager
    {
        set
        {
            networkManager = value;

            if (networkManager != null)
            {
                if (Multiplayer.IsServer())
                {
                    networkManager.ServerClientLoggedIn += OnServerClientLoggedIn;
                }
            }
        }
    }

    private Node2D units = null;
    private Node2D players = null;
    private Node2D materials = null;
    private PackedScene playerScene = GD.Load<PackedScene>("res://scenes/Player/Player.tscn");
    private PackedScene workerScene = GD.Load<PackedScene>("res://scenes/units/Worker/Worker.tscn");
    private PackedScene treeScene = GD.Load<PackedScene>("res://scenes/materials/Tree/Tree.tscn");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        units = GetNode<Node2D>("%Units");
        players = GetNode<Node2D>("%Players");
        materials = GetNode<Node2D>("%Materials");

        players.ChildEnteredTree += OnPlayersChildEnteredTree;
    }

    public Unit GetUnit(string name)
    {
        return units.GetNodeOrNull<Unit>(name);
    }

    public void ServerCreateEntity(string entityName, Vector2 position)
    {
        switch (entityName)
        {
            case "Worker":
                Worker worker = (Worker)workerScene.Instantiate();
                worker.Position = position;
                units.AddChild(worker, true); break;
            case "Tree":
                Tree tree = (Tree)treeScene.Instantiate();
                tree.Position = position;
                materials.AddChild(tree, true); break;
            default:
                GD.Print("Unknown unit name: " + entityName);
                break;
        }

    }
    private void OnServerClientLoggedIn(Client client)
    {
        Player player = (Player)playerScene.Instantiate();
        player.Map = this;
        player.Username = client.Username;
        player.PeerID = client.PeerID;
        players.AddChild(player, true);
    }

    private void OnPlayersChildEnteredTree(Node child)
    {
        GD.Print("Child entered tree");
        if (child is Player player)
        {
            GD.Print("Setting player map");
            player.Map = this;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void CreateEntityRPC(string entityName, Vector2 position)
    {
        if (!Multiplayer.IsServer())
        {
            GD.Print("CreateEntityRPC called on non-server node. Ignoring.");
            return;
        }

        ServerCreateEntity(entityName, position);
    }
}
