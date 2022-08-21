namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using System;
    using System.Collections.Generic;

    public class TargetableBehaviour : TickableBehaviour
    {
		public Tile targetTile;
		protected Creature identityCreature;

		public DungeonObject outlinePrefab;
		protected List<DungeonObject> outlineObjects = new List<DungeonObject>();

		public DungeonObject threatenedPrefab;
		protected List<DungeonObject> threatenedObjects = new List<DungeonObject>();

		public float aimingRadius = 3;

        override public void Awake()
        {
			base.Awake();
			identityCreature = owner.GetComponent<Creature>();
		}

        public override bool IsActionACoroutine()
        {
			return true;
        }
		virtual public void AddOutline(List<Tile> tiles)
		{
			foreach (Tile tile in tiles) AddOutline(tile);
		}

		virtual public void AddOutline(Tile tile)
		{
			outlineObjects.Add(tile.SpawnAndAddObject(outlinePrefab, 1, true));
		}

		virtual public void RemoveOutline()
		{
			foreach (DungeonObject outlineObject in outlineObjects)
			{
				outlineObject.tile.RemoveObject(outlineObject, true);
			}
			outlineObjects.Clear();
		}

		virtual public void AddThreatened(List<Tile> tiles)
		{
			if (tiles == null) return;
			foreach (Tile tile in tiles) AddThreatened(tile);
		}

		virtual public void AddThreatened(Tile tile)
		{
			threatenedObjects.Add(tile.SpawnAndAddObject(threatenedPrefab, 1));
		}

		virtual public void RemoveThreatened()
		{
			foreach (DungeonObject threatenedObject in threatenedObjects)
			{
				threatenedObject.tile.RemoveObject(threatenedObject, true);
			}
			threatenedObjects.Clear();
		}

		override public IEnumerator StartActionCoroutine()
		{
			owner.tickable.nextActionTime = identityCreature.ticksPerAttack;

			CameraTarget.instance.owner = HighlightTile.instance;
			CameraTarget.instance.thresholdX = 6;
			CameraTarget.instance.thresholdY = 4;
			AimOverlay.instance.gameObject.SetActive(true);

			var allowedTiles = Map.instance.GetTilesInRadius(
				Player.instance.identity.x, Player.instance.identity.y,
				aimingRadius,
				null,
				false
			);

			AddOutline(allowedTiles);

			HighlightTile.instance.allowedTiles = allowedTiles;

			HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].gameObject.SetActive(true);
			HighlightTile.instance.isKeyboardControlled = true;
			//HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.red;
			bool isDone = false;
			while (!isDone)
			{
				while (!PlayerInputHandler.instance.HasInput)
				{
					if (targetTile != HighlightTile.instance.tile)
					{
						targetTile = HighlightTile.instance.tile;
						RemoveThreatened();
						AddThreatened(GetThreatenedTiles());
					}
					yield return new WaitForEndOfFrame();
				}
				Command nextCommand = PlayerInputHandler.instance.commandQueue.Peek();
				HighlightTile.instance.Move(nextCommand);
				if (nextCommand.key == Key.Space || nextCommand.mouseButton == Mouse.current.leftButton)
				{
					isDone = true;
				}
				PlayerInputHandler.instance.commandQueue.Dequeue();
				if (targetTile != HighlightTile.instance.tile)
				{
					targetTile = HighlightTile.instance.tile;
					RemoveThreatened();
					AddThreatened(GetThreatenedTiles());
				}
				yield return new WaitForEndOfFrame();
			}
			//HighlightTile.instance.GetComponent<DungeonObject>().glyphs.glyphs[0].tint = Color.white;
			HighlightTile.instance.isKeyboardControlled = false;

			HighlightTile.instance.allowedTiles = null;

			CameraTarget.instance.owner = Player.instance.identity;
			CameraTarget.instance.thresholdX = 0;
			CameraTarget.instance.thresholdY = 0;
			AimOverlay.instance.gameObject.SetActive(false);

			RemoveOutline();
			RemoveThreatened();
			targetTile = HighlightTile.instance.tile;
		}

        virtual protected List<Tile> GetThreatenedTiles()
        {
			return null;
        }
    }
}
