using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BaseBuilding.Assets;
using Godot;

namespace BaseBuilding;

public partial class BuildMenu : Control
{
	private readonly BuildingCollection _buildingCollection = new();
	private readonly ObjectPool<Sprite2D> _ghosts;
	private Building? _building;
	private Vector2i? _dragStart;

	public BuildMenu()
	{
		_ghosts = new ObjectPool<Sprite2D>(ResetSprite);
	}

	[MemberNotNullWhen(true, nameof(_dragStart))]
	private bool IsDragging => _dragStart != null;

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

		if (!IsDragging && @event.IsActionPressed(InputActions.MouseclickLeft))
		{
			var tileMap = GetNode<TileMap>("%Map");
			var mousePosition = tileMap.GetGlobalMousePosition();
			_dragStart = tileMap.LocalToMap(mousePosition);
		}

		if (@event.IsActionReleased(InputActions.MouseclickLeft))
		{
			HandleMouseDrag(_building);
		}
		else if (@event.IsActionPressed(InputActions.MouseclickRight))
		{
			SetEnabled(false);
		}
	}

	private void HandleMouseDrag(Building building)
	{
		Debug.Assert(IsDragging);

		var tileMap = GetNode<TileMap>("%Map");
		var mousePosition = tileMap.GetGlobalMousePosition();
		var clampedMousePosition = tileMap.LocalToMap(mousePosition);

		if (_building == _buildingCollection.Remove)
		{
			foreach (var tile in GetTilesInRectangle(_dragStart.Value, clampedMousePosition))
			{
				var nodeName = $"{tile}";
				var node = tileMap.GetNodeOrNull(nodeName);
				node?.Free();
			}
		}
		else
		{
			foreach (var tile in GetTilesInRectangle(_dragStart.Value, clampedMousePosition))
			{
				PlaceGhost(tileMap, building, tile);
			}
		}

		_dragStart = null;
		GetTree().Root.SetInputAsHandled();
	}

	private static void PlaceGhost(TileMap tileMap, Building building, Vector2i tile)
	{
		var nodeName = $"{tile}";

		if (tileMap.HasNode(nodeName))
		{
			// TODO: Consider cancelling placement if invalid tile is selected
			return;
		}

		var position = tileMap.MapToLocal(tile);

		tileMap.AddChild(new Sprite2D
		{
			Texture = building.Texture, GlobalPosition = position, ZIndex = 1, Name = nodeName,
		});
	}

	public override void _Process(double delta)
	{
		var tileMap = GetNode<TileMap>("%Map");
		var buildGhost = tileMap.GetNodeOrNull<Node2D>("BuildGhost");

		if (buildGhost == null)
		{
			buildGhost = new Node2D { Name = "BuildGhost", ZIndex = 2 };
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

		if (!IsDragging)
		{
			return;
		}

		ArgumentNullException.ThrowIfNull(_building);

		var mousePosition = tileMap.GetGlobalMousePosition();
		var clampedMousePosition = tileMap.LocalToMap(mousePosition);

		foreach (var tile in GetTilesInRectangle(_dragStart.Value, clampedMousePosition))
		{
			var position = tileMap.MapToLocal(tile);
			var nodeName = $"{tile}";

			var sprite = _ghosts.Acquire();
			sprite.Texture = _building.GhostTexture;
			sprite.GlobalPosition = position;
			sprite.Name = nodeName;

			if (!CanBuild(tile))
			{
				sprite.SelfModulate = Colors.Red;
			}

			buildGhost.AddChild(sprite);
		}
	}

	private bool CanBuild(Vector2i tile)
	{
		var tileMap = GetNode<TileMap>("%Map");
		var nodeName = $"{tile}";

		if (tileMap.HasNode(nodeName))
		{
			return false;
		}

		return true;
	}

	private static IEnumerable<Vector2i> GetTilesInRectangle(Vector2i cornerOne, Vector2i cornerTwo)
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

	private static void ResetSprite(Sprite2D sprite)
	{
		sprite.ZIndex = default;
		sprite.Position = default;
		sprite.GlobalPosition = default;
		sprite.Modulate = Colors.White;
		sprite.SelfModulate = Colors.White;
	}
}
