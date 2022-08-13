
using UnityEngine;

namespace Noble.TileEngine
{
    public class AttackBehaviour : TickableBehaviour
    {
		public Tile targetTile;
		Creature identityCreature;
		DungeonObject targetObject;
		bool attackWillHit;

        override public void Awake()
        {
			base.Awake();
			identityCreature = owner.GetComponent<Creature>();
		}

		override public void StartAction()
		{
			owner.tickable.nextActionTime = identityCreature.ticksPerAttack;
			foreach (var dOb in targetTile.objectList)
			{
				if (dOb.isCollidable)
				{
					targetObject = dOb;
				}
			}
		}

		override public void StartSubAction(ulong time) 
		{
			Creature targetCreature = targetObject.GetComponent<Creature>();

			identityCreature.lastDirectionAttackedOrMoved = identityCreature.GetDirection(targetObject.x, targetObject.y, identityCreature.x, identityCreature.y);

			attackWillHit = false;

			Weapon weapon = null;
			if (identityCreature.rightHandObject)
			{
				weapon = identityCreature.rightHandObject.GetComponent<Weapon>();
			}

			if (weapon)
			{
				weapon.targetCreature = targetCreature;
				weapon.StartSubAction(time);
			}

			if (targetCreature != null)
			{
				float roll = UnityEngine.Random.Range(0, 20);
				roll += identityCreature.dexterity;
				if (roll > targetCreature.dexterity)
				{
					// Hit, but do we do damange?
					if (roll > targetCreature.dexterity + targetCreature.defense)
					{
						// Got past armor / defense
						attackWillHit = true;
					}
				}
			}

			identityCreature.attackAnimationTime = 0;
		}
		override public bool ContinueSubAction(ulong time) 
		{
			bool isCreatureDone = true;
			if (identityCreature.attackAnimationTime < identityCreature.attackAnimationDuration)
			{
				Creature targetCreature = targetObject.GetComponent<Creature>();
				Vector3 originalPosition = identityCreature.baseObject.originalGlyphPosition;

				float offset = identityCreature.attackMovementAnimation.Evaluate(identityCreature.attackAnimationTime / identityCreature.attackAnimationDuration) * identityCreature.attackAnimationScale;

				var glyph = identityCreature.baseObject.glyphs;
				if (glyph)
				{
					switch (identityCreature.lastDirectionAttackedOrMoved)
					{
						case Direction.UP: glyph.transform.localPosition = originalPosition + Vector3.up * offset; break;
						case Direction.DOWN: glyph.transform.localPosition = originalPosition + Vector3.down * offset; break;
						case Direction.RIGHT: glyph.transform.localPosition = originalPosition + Vector3.right * offset; break;
						case Direction.LEFT: glyph.transform.localPosition = originalPosition + Vector3.left * offset; break;
						case Direction.UP_LEFT: glyph.transform.localPosition = originalPosition + Util.UpLeft * offset; break;
						case Direction.DOWN_LEFT: glyph.transform.localPosition = originalPosition + Util.DownLeft * offset; break;
						case Direction.UP_RIGHT: glyph.transform.localPosition = originalPosition + Util.UpRight * offset; break;
						case Direction.DOWN_RIGHT: glyph.transform.localPosition = originalPosition + Util.DownRight * offset; break;
					}
				}
				if (attackWillHit && targetCreature)
				{
					targetCreature.baseObject.DamageFlash(identityCreature.attackAnimationTime / identityCreature.attackAnimationDuration);
				}

				identityCreature.attackAnimationTime += Time.deltaTime;

				isCreatureDone = false;
			}
			else
            {
				isCreatureDone = true;
            }				

			Weapon weapon = null;
			bool isWeaponDone = true;
			if (identityCreature.rightHandObject)
			{
				weapon = identityCreature.rightHandObject.GetComponent<Weapon>();
			}
			if (weapon)
			{
				isWeaponDone = weapon.ContinueSubAction(time);
			}

			return isCreatureDone && isWeaponDone;
		}
		override public void FinishSubAction(ulong time) 
		{
			Map.instance.TryMoveObject(owner, targetTile.x, targetTile.y);
			Creature targetCreature = targetObject.GetComponent<Creature>();

			Weapon weapon = null;
			if (identityCreature.rightHandObject != null)
			{
				weapon = identityCreature.rightHandObject.GetComponent<Weapon>();
			}

			if (attackWillHit && targetCreature)
			{
				targetCreature.baseObject.DamageFlash(1);
				// Got past armor / defense
				if (!weapon)
				{
					targetCreature.baseObject.TakeDamage(1);
				}
			}

			Vector3 originalPosition = identityCreature.baseObject.originalGlyphPosition;
			identityCreature.baseObject.glyphs.transform.localPosition = originalPosition;

			if (weapon)
			{
				weapon.FinishSubAction(time);
			}
		}
	}
}
