namespace Noble.DungeonCrawler
{
	using Noble.TileEngine;
	using System.Collections;
    using UnityEngine;
	using System.Linq;
    using System.Collections.Generic;

    public class FireAOEBehaviour : TargetableBehaviour
    {
		public GameObject fireballObjectPrefab;
		public GameObject firePrefab;
		GameObject fireballObject;
		float attackStartTime = 0;

        override public void Awake()
        {
			base.Awake();
		}

		override protected List<Tile> GetThreatenedTiles()
		{
			List<Tile> threatenedTile = new List<Tile>();

			float dirX = Map.instance.GetXDifference(owner.x, targetTile.x);
			float dirY = targetTile.y - owner.y;
			Vector2 direction = new Vector2(dirX, dirY);
			float distance = direction.magnitude;
			direction.Normalize();

			float stepSize = Mathf.Min(Map.instance.tileWidth, Map.instance.tileHeight) * .9f;
			Vector2 center = new Vector2(owner.x + .5f, owner.y + .5f);
			for (int d = 1; d < distance / stepSize; d++)
			{
				Vector2 relative = center + direction * d * stepSize;

				int y = (int)relative.y;
				if (y < 0 || y >= Map.instance.height) break;

				int wrappedX = (int)Map.instance.GetXPositionOnMap(relative.x);

				if (Map.instance.tileObjects[y][wrappedX].IsCollidable() && (y != owner.y || wrappedX != owner.x))
				{
					threatenedTile.Add(Map.instance.tileObjects[y][wrappedX]);
					break;
				}
			}

			if (threatenedTile.Count == 0)
            {
				threatenedTile.Add(targetTile);
            }				

			return threatenedTile;
		}


		override public IEnumerator StartActionCoroutine()
		{
			yield return base.StartActionCoroutine();

			fireballObject = Instantiate(fireballObjectPrefab);
			fireballObject.transform.position = identityCreature.leftHand.transform.position - Vector3.forward;
		}

		override public void StartSubAction(ulong time) 
		{
			attackStartTime = Time.time;

			float dirX = Map.instance.GetXDifference(owner.x, targetTile.x);
			float dirY = targetTile.y - owner.y;
			Vector2 direction = new Vector2(dirX, dirY);
			float distance = direction.magnitude;
			direction.Normalize();

			float stepSize = Mathf.Min(Map.instance.tileWidth, Map.instance.tileHeight) * .9f;
			Vector2 center = new Vector2(owner.x + .5f, owner.y + .5f);
			for (int d = 1; d < distance / stepSize; d++)
			{
				Vector2 relative = center + direction * d * stepSize;

				int y = (int)relative.y;
				if (y < 0 || y >= Map.instance.height) break;

				int wrappedX = (int)Map.instance.GetXPositionOnMap(relative.x);

				if (Map.instance.tileObjects[y][wrappedX].IsCollidable() && (y != owner.y || wrappedX != owner.x))
                {
					targetTile = Map.instance.tileObjects[y][wrappedX];
					break;
                }
			}
		}
		override public bool ContinueSubAction(ulong time) 
		{
			Vector2 startPosition = identityCreature.leftHand.transform.position;
			Vector2 endPosition = new Vector2(targetTile.transform.position.x + Map.instance.tileWidth/2, targetTile.transform.position.y + Map.instance.tileHeight/2);
			float unitsPerSecond = 10;
			float timeSinceAttackStart = Time.time - attackStartTime;
			if ((endPosition - startPosition).magnitude > Map.instance.TotalWidth / 2)
            {
				if ((startPosition.x - Map.instance.transform.position.x) > Map.instance.TotalWidth / 2)
                {
					endPosition.x += Map.instance.TotalWidth;
				}
				else
                {
					endPosition.x -= Map.instance.TotalWidth;
				}
			}

			float duration = (startPosition - endPosition).magnitude / unitsPerSecond;
			fireballObject.transform.position = (Vector3)Vector2.Lerp(startPosition, endPosition, timeSinceAttackStart / duration) - Vector3.forward;

			if (timeSinceAttackStart > duration)
            {
				return true;
            }
			else
            {
				return false;
            }
		}
		override public void FinishSubAction(ulong time)
		{
			if (targetTile && targetTile.objectList != null)
			{
				DungeonObject targetObject = targetTile.objectList.FirstOrDefault(ob => ob.isCollidable);
				if (targetObject)
				{
					targetObject.TakeDamage(10);
				}
			}
			var fire = Instantiate(firePrefab);
			targetTile.AddObject(fire.GetComponent<DungeonObject>());
			
			Destroy(fireballObject);
		}
	}
}
