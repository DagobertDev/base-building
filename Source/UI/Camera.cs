using System.Diagnostics;
using Godot;

namespace BaseBuilding.UI;

public partial class Camera : Camera2D
{
	private int Speed => 400;
	private float MinimumZoom => 1 / 18f;
	private float MaximumZoom => 1;
	private float ZoomStep => 0.1f;
	private float InitialZoom => 0.5f;

	public override void _Ready()
	{
		Debug.Assert(Speed > 0);
		Debug.Assert(MinimumZoom > 0);
		Debug.Assert(MinimumZoom <= MaximumZoom);
		Debug.Assert(ZoomStep is > 0 and < 1);
		Debug.Assert(InitialZoom >= MinimumZoom && InitialZoom <= MaximumZoom);
		
		Zoom = Vector2.One * InitialZoom;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed(InputActions.ZoomIn))
		{
			Zoom /= 1 - ZoomStep;
			Zoom = Zoom.Clamp(Vector2.One * MinimumZoom, Vector2.One * MaximumZoom);
		}
		else if (@event.IsActionPressed(InputActions.ZoomOut))
		{
			Zoom *= 1 - ZoomStep;
			Zoom = Zoom.Clamp(Vector2.One * MinimumZoom, Vector2.One * MaximumZoom);
		}
		else if (@event is InputEventMouseMotion motion && Input.IsActionPressed(InputActions.MouseclickRight))
		{ 
			GlobalPosition -= motion.Relative / Zoom;
		}
	}

	public override void _Process(double delta)
	{
		var direction = Input.GetVector(
			InputActions.CameraLeft,
			InputActions.CameraRight,
			InputActions.CameraUp,
			InputActions.CameraDown);

		Position += Speed * (float)delta * direction / Zoom;
	}
}
