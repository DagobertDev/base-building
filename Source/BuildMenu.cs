using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BaseBuilding.Assets;
using Godot;

namespace BaseBuilding;

public partial class BuildMenu : Control
{
	private readonly ObjectPool<Sprite2D> _ghosts = new();
	private Building? _building;

	private readonly BuildingCollection _buildingCollection = new();
	private Vector2i? _dragStart;

	public override void _Ready()
	{
		SetEnabled(false);
	}

	public void SetMode(string building)
	{
		_building = _buildingCollection.GetBuilding(building);
		SetEnabled(_building != null);
	}

	private void SetEnabled(bool enabled)
	{
		SetProcess(enabled);
		SetProcessUnhandledInput(enabled);

		_dragStart = null;

		var tileMap = GetNode<TileMap>("%Map");
		var buildGhost = tileMap.GetNodeOrNull<Node2D>("BuildGhost");

		if (buildGhost != null)
		{
			foreach (var sprite in buildGhost.GetChildren().Cast<Sprite2D>())
			{
				buildGhost.RemoveChild(sprite);
				_ghosts.Release(sprite);
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		Debug.Assert(_building != null);

		if (_dragStart == null && @event.IsActionPressed(InputActions.MouseclickLeft))
		{
			var tileMap = GetNode<TileMap>("%Map");
			var mousePosition = tileMap.GetGlobalMousePosition();
			_dragStart = tileMap.LocalToMap(mousePosition);
		}

		if (@event.IsActionReleased(InputActions.MouseclickLeft))
		{
			ArgumentNullException.ThrowIfNull(_dragStart);

			var tileMap = GetNode<TileMap>("%Map");
			var mousePosition = tileMap.GetGlobalMousePosition();
			var clampedMousePosition = tileMap.LocalToMap(mousePosition);

			if (_building == _buildingCollection.Remove)
			{
				foreach (var cell in GetPositionsInRectangle(_dragStart.Value, clampedMousePosition))
				{
					var position = tileMap.MapToLocal(cell);
					var nodeName = $"{position}";
					var node = tileMap.GetNodeOrNull(nodeName);
					node?.Free();
				}
			}
			else
			{
				ArgumentNullException.ThrowIfNull(_building);

				foreach (var cell in GetPositionsInRectangle(_dragStart.Value, clampedMousePosition))
				{
					var position = tileMap.MapToLocal(cell);
					var nodeName = $"{position}";

					if (tileMap.HasNode(nodeName))
					{
						// TODO: Consider cancelling placement if invalid cell is selected
						continue;
					}

					tileMap.AddChild(new Sprite2D
					{
						Texture = _building.Texture,
						GlobalPosition = position,
						ZIndex = 1,
						Name = nodeName,
					});
				}
			}

			_dragStart = null;
			GetTree().Root.SetInputAsHandled();
		}
		else if (@event.IsActionPressed(InputActions.MouseclickRight))
		{
			SetEnabled(false);
		}
	}

	public override void _Process(double delta)
	{
		var tileMap = GetNode<TileMap>("%Map");
		var buildGhost = tileMap.GetNodeOrNull<Node2D>("BuildGhost");

		if (buildGhost == null)
		{
			buildGhost = new Node2D
			{
				Name = "BuildGhost",
				ZIndex = 2,
			};
			tileMap.AddChild(buildGhost);
		}
		else
		{
			foreach (var sprite in buildGhost.GetChildren().Cast<Sprite2D>())
			{
				buildGhost.RemoveChild(sprite);
				_ghosts.Release(sprite);
			}
		}

		if (_dragStart == null)
		{
			return;
		}
		
		ArgumentNullException.ThrowIfNull(_building);

		var mousePosition = tileMap.GetGlobalMousePosition();
		var clampedMousePosition = tileMap.LocalToMap(mousePosition);

		foreach (var cell in GetPositionsInRectangle(_dragStart.Value, clampedMousePosition))
		{
			var position = tileMap.MapToLocal(cell);
			var nodeName = $"{position}";

			var texture = _building.Texture;

			var sprite = _ghosts.Acquire();
			sprite.Texture = texture;
			sprite.GlobalPosition = position;
			sprite.Name = nodeName;
			sprite.SelfModulate = Colors.White.Lerp(Colors.Transparent, 0.3f);
			buildGhost.AddChild(sprite);
		}
	}

	private IEnumerable<Vector2i> GetPositionsInRectangle(Vector2i cornerOne, Vector2i cornerTwo)
	{
		var start = cornerOne;
		var end = cornerTwo;

		if (start.x > end.x)
		{
			(end.x, start.x) = (start.x, end.x);
		}

		if (start.y > end.y)
		{
			(end.y, start.y) = (start.y, end.y);
		}

		var width = end.x - start.x;
		var height = end.y - start.y;

		for (var offsetX = 0; offsetX <= width; offsetX++)
		{
			for (var offsetY = 0; offsetY <= height; offsetY++)
			{
				yield return start + new Vector2i(offsetX, offsetY);
			}
		}
	}
}
