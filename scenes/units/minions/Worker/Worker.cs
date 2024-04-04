using Godot;
using Godot.Collections;
using System;

public partial class Worker : Unit
{
	public float Speed = 300.0f;
	public uint MaxMaterialsInBag { get; set; } = 5;
	public float GatherSpeed = 1.0f;
	public uint GatherEfficiency = 1;
	private Area2D interactArea = null;
	private Timer gatherTimer = null;
	private ProgressBar progressBar = null;

	private Material materialTarget = null;
	private Unit storageTarget = null;

	private Array<Node> bodiesInInteractRange = new Array<Node>();

	[Export]
	private string materialTypeInBag = "";
	[Export]
	private uint amountOfMaterialInBag = 0;
	public uint AmountOfMaterialInBag
	{
		get { return amountOfMaterialInBag; }
		set
		{
			amountOfMaterialInBag = value;
			progressBar.Value = value;
		}
	}


	public override void _Ready()
	{
		base._Ready();

		interactArea = GetNode<Area2D>("%InteractArea");
		interactArea.BodyEntered += OnInteractAreaBodyEntered;
		interactArea.BodyExited += OnInteractAreaBodyExited;

		progressBar = GetNode<ProgressBar>("%ProgressBar");
		progressBar.MaxValue = MaxMaterialsInBag;

		AmountOfMaterialInBag = 0;

		gatherTimer = new Timer();
		gatherTimer.Name = "GatherTimer";
		gatherTimer.OneShot = true;
		gatherTimer.WaitTime = 1.0f;
		gatherTimer.Autostart = false;
		gatherTimer.Timeout += OnGatherTimerTimeout;
		AddChild(gatherTimer);
	}


	public override void _PhysicsProcess(double delta)
	{
		if (materialTarget != null && AmountOfMaterialInBag < MaxMaterialsInBag)
		{
			if (!bodiesInInteractRange.Contains(materialTarget))
			{
				Velocity = Position.DirectionTo(materialTarget.Position) * Speed;
			}
			else
			{
				Velocity = Vector2.Zero;

				if (gatherTimer.IsStopped())
				{
					gatherTimer.Start(GatherSpeed);
				}
			}

		}
		else if (storageTarget != null)
		{
			if (!bodiesInInteractRange.Contains(storageTarget))
			{
				Velocity = Position.DirectionTo(storageTarget.Position) * Speed;
			}
			else
			{
				Velocity = Vector2.Zero;

				if (storageTarget.StoreMaterial(materialTypeInBag, AmountOfMaterialInBag))
				{
					AmountOfMaterialInBag = 0;
					//progressBar.Value = AmountOfMaterialInBag;
					materialTypeInBag = "";
				}
			}
		}
		else if (Position.DistanceTo(targetPosition) > 8.0f)
		{
			Velocity = Position.DirectionTo(targetPosition) * Speed;

		}
		else
		{
			Velocity = Vector2.Zero;
		}

		MoveAndSlide();
	}

	public override void MoveTo(Vector2 position)
	{
		targetPosition = position;
		materialTarget = null;
		storageTarget = null;
	}

	public override void GatherMaterial(Material material, Unit Storage)
	{
		GD.Print("Gathering material from " + material.Name + " to " + Storage.Name);
		targetPosition = material.Position;
		materialTarget = material;
		storageTarget = Storage;
	}

	private void OnInteractAreaBodyEntered(Node body)
	{
		if (!bodiesInInteractRange.Contains(body))
		{
			bodiesInInteractRange.Add(body);
		}
	}

	private void OnInteractAreaBodyExited(Node body)
	{
		if (bodiesInInteractRange.Contains(body))
		{
			bodiesInInteractRange.Remove(body);
		}
	}

	private void OnGatherTimerTimeout()
	{
		if (materialTarget == null)
		{
			return;
		}

		if (!bodiesInInteractRange.Contains(materialTarget))
		{
			return;
		}

		if (AmountOfMaterialInBag >= MaxMaterialsInBag)
		{
			return;
		}

		if (materialTypeInBag == materialTarget.MaterialType)
		{
			AmountOfMaterialInBag = Math.Min(MaxMaterialsInBag, AmountOfMaterialInBag + GatherEfficiency);
			//progressBar.Value = AmountOfMaterialInBag;
		}
		else
		{
			materialTypeInBag = materialTarget.MaterialType;
			AmountOfMaterialInBag = GatherEfficiency;
			//progressBar.Value = AmountOfMaterialInBag;
		}
	}
}
