using Godot;
using System;

public partial class Construction : Unit
{
    [Export]
    public string BuildingName
    {
        get
        {
            return buildingName;
        }
        set
        {
            buildingName = value;
            Name = value + "-Construction";
            SetInfo();
        }
    }
    public override bool IsRepairable { get; set; } = true;
    private string buildingName = "";
    private Label label = null;
    private Panel panel = null;
    private CollisionShape2D collisionShape = null;
    private float constructionTime = 0.0f;
    private float constructionProgress = 0.0f;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");
        panel = GetNode<Panel>("Panel");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        SetInfo();
    }

    private void SetInfo()
    {
        if (label == null)
        {
            return;
        }

        label.Text = BuildingName + "-Construction";

        SetPanelSize();
    }

    private void SetPanelSize()
    {

        if (panel == null || collisionShape == null)
        {
            return;
        }
        float radius = buildingName switch
        {
            "Townhall" => Townhall.Radius,
            _ => Unit.Radius,
        };

        panel.Size = new Vector2(radius * 2, radius * 2);
        panel.Position = new Vector2(-radius, -radius);
        collisionShape.Shape = new CircleShape2D { Radius = radius };
    }

    public override bool GetRepaired(float amount)
    {
        GD.Print("Repairing " + Name + " by " + amount);
        return true;
    }
}
