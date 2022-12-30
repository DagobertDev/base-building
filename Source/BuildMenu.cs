using System;
using System.Diagnostics;
using Godot;

namespace BaseBuilding;

public partial class BuildMenu : Control
{
	public enum BuildMode
	{
		Disabled = 0, Wall = 1, Remove = 2,
	}

	private BuildMode _buildMode = BuildMode.Disabled;

	[Export(PropertyHint.ResourceType, nameof(Texture2D))]
	private Texture2D _texture = null!;

	public override void _Ready()
	{
		SetEnabled(false);
	}

	public void SetMode(BuildMode buildMode)
	{
		if (!Enum.IsDefined(buildMode))
		{
			throw new ArgumentException(nameof(buildMode));
		}

		_buildMode = buildMode;
		SetEnabled(_buildMode != BuildMode.Disabled);
	}

	private void SetEnabled(bool enabled)
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

			var nodeName = $"{mapPosition}";

			if (_buildMode == BuildMode.Wall)
			{
				if (tileMap.HasNode(nodeName))
				{
					return;
				}

				tileMap.AddChild(new Sprite2D
				{
					Texture = _texture,
					GlobalPosition = mapPosition,
					ZIndex = 1,
					Name = nodeName,
				});
			}
			else if (_buildMode == BuildMode.Remove)
			{
				var node = tileMap.GetNodeOrNull(nodeName);
				node?.Free();
			}

			GetTree().Root.SetInputAsHandled();
		}
		else if (@event.IsActionPressed(InputActions.MouseclickRight))
		{
			SetEnabled(false);
		}
	}
}
