using System.Diagnostics;
using Godot;

namespace BaseBuilding;

public partial class WallPlacer : Control
{
	[Export(PropertyHint.ResourceType, nameof(Texture2D))]
	private Texture2D _texture = null!;

	public override void _Ready()
	{
		SetEnabled(false);
	}

	public void SetEnabled(bool enabled)
	{
		SetProcessUnhandledInput(enabled);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		Debug.Assert(_texture != null);

		if (@event.IsActionPressed(InputActions.MouseclickLeft))
		{
			var tileMap = GetNode<TileMap>("%Map");
			var mousePosition = tileMap.GetGlobalMousePosition();	
			var mapPosition = tileMap.MapToLocal(tileMap.LocalToMap(mousePosition));
			
			tileMap.AddChild(new Sprite2D
			{
				Texture = _texture,
				GlobalPosition = mapPosition,
				ZIndex = 1,
			});

			GetTree().Root.SetInputAsHandled();
		}
		else if (@event.IsActionPressed(InputActions.MouseclickRight))
		{
			SetEnabled(false);
		}
	}
}
