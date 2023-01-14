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

		protected List<DungeonObject> outlineObjects = new List<DungeonObject>();
		protected List<DungeonObject> threatenedObjects = new List<DungeonObject>();
		public List<Tile> threatenedTiles = new List<Tile>();

		public float aimingRadius = 3;

		public Sprite shapeSprite;
		public List<Vector2Int> shape;
		public int level;

		protected Creature identityCreature => owner.gameObject.GetComponentInParent<Creature>(true);

		override public void Awake()
		{
			base.Awake();
		}

		virtual public void Start()
        {
			var pixels = shapeSprite.texture.GetPixels((int)shapeSprite.rect.xMin, (int)shapeSprite.rect.yMin, (int)shapeSprite.rect.width, (int)shapeSprite.rect.height);
			Vector2Int targetPixel = FindTargetPixel(pixels);
            FindThreatenedPixels(pixels, targetPixel);

		}

		void FindThreatenedPixels(Color[] pixels, Vector2Int targetPixel)
        {
			int i = 0;
			for (int x = 0; x < shapeSprite.rect.width; x++)
			{
				for (int y = 0; y < shapeSprite.rect.height; y++)
				{
					if (pixels[i].a == 1)
					{
						if (pixels[i].r >= .25f)
						{
							shape.Add(new Vector2Int(x - targetPixel.x, y - targetPixel.y));
						}
					}
					i++;
				}
			}
		}

		Vector2Int FindTargetPixel(Color[] pixels)
        {
			int i = 0;
			for (int x = 0; x < shapeSprite.rect.width; x++)
			{
				for (int y = 0; y < shapeSprite.rect.height; y++)
				{
					if (pixels[i].a == 1)
					{
						if (pixels[i].r >= .5f)
						{
							return new Vector2Int(x, y);
						}
					}
					i++;
				}
			}

			throw new Exception("No target pixel in shape texture");
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
			var outlinePrefab = Resources.Load<DungeonObject>("OutlineTile");
			outlineObjects.Add(tile.SpawnAndAddObject(outlinePrefab, 1, 0));
		}

		virtual public void RemoveOutline()
		{
			foreach (DungeonObject outlineObject in outlineObjects)
			{
				outlineObject.tile.RemoveObject(outlineObject, true);
			}
			outlineObjects.Clear();
		}

		virtual public void AddThreatened(List<Tile> hits)
		{
			if (hits == null) return;
			foreach (var hit in hits) AddThreatened(hit);
		}

		virtual public void AddThreatened(Tile tile)
		{
			var threatenedPrefab = Resources.Load<DungeonObject>("ThreatenedTile");
			threatenedObjects.Add(tile.SpawnAndAddObject(threatenedPrefab, 2));
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
            Debug.Log("F Given");
            identityCreature.tickable.nextActionTime = identityCreature.ticksPerAttack;

			CameraTarget.instance.owner = HighlightTile.instance;
			CameraTarget.instance.thresholdX = 6;
			CameraTarget.instance.thresholdY = 4;

			var allowedTiles = Map.instance.GetTilesInRadius(
				Player.Identity.tilePosition + owner.map.tileDimensions / 2,
				aimingRadius,
				null,
				false,
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

			CameraTarget.instance.owner = Player.Identity;
			CameraTarget.instance.thresholdX = 0;
			CameraTarget.instance.thresholdY = 0;

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
