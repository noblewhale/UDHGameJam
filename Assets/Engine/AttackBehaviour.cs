
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
            owner.tickable.nextActionTime = TimeManager.instance.Time + identityCreature.ticksPerAttack;
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

			identityCreature.lastDirectionAttackedOrMoved = identityCreature.GetDirection(identityCreature.tilePosition, targetObject.tilePosition);

			attackWillHit = false;

			Weapon weapon = null;
			Equipable equippedRightHandItem = identityCreature.GetEquipment(Equipment.Slot.RIGHT_HAND_WEAPON);
            if (equippedRightHandItem)
			{
				weapon = equippedRightHandItem.GetComponent<Weapon>();
			}

			if (weapon)
			{
				weapon.targetCreature = targetCreature;
				weapon.StartSubAction(time);
			}

			if (targetCreature != null)
			{
                float roll = Random.Range(0, 20);
				int attackerDex = identityCreature.baseObject.GetPropertyValue<int>("Dexterity");
				int defenderDex = targetCreature.baseObject.GetPropertyValue<int>("Dexterity");
				int defenderArmor = targetCreature.baseObject.GetPropertyValue<int>("Armor");
                roll += attackerDex;
				if (roll > defenderDex)
				{
					// Hit, but do we do damange?
					if (roll > defenderDex + defenderArmor)
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

				var glyphs = identityCreature.baseObject.glyphs;
				if (glyphs)
				{
					switch (identityCreature.lastDirectionAttackedOrMoved)
					{
						case Direction.UP: glyphs.transform.localPosition = originalPosition + Vector3.up * offset; break;
						case Direction.DOWN: glyphs.transform.localPosition = originalPosition + Vector3.down * offset; break;
						case Direction.RIGHT: glyphs.transform.localPosition = originalPosition + Vector3.right * offset; break;
						case Direction.LEFT: glyphs.transform.localPosition = originalPosition + Vector3.left * offset; break;
						case Direction.UP_LEFT: glyphs.transform.localPosition = originalPosition + VectorUtil.UpLeft * offset; break;
						case Direction.DOWN_LEFT: glyphs.transform.localPosition = originalPosition + VectorUtil.DownLeft * offset; break;
						case Direction.UP_RIGHT: glyphs.transform.localPosition = originalPosition + VectorUtil.UpRight * offset; break;
						case Direction.DOWN_RIGHT: glyphs.transform.localPosition = originalPosition + VectorUtil.DownRight * offset; break;
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
            Equipable equippedRightHandItem = identityCreature.GetEquipment(Equipment.Slot.RIGHT_HAND_WEAPON);
            if (equippedRightHandItem)
            {
                weapon = equippedRightHandItem.GetComponent<Weapon>();
            }
            if (equippedRightHandItem)
			{
				weapon = equippedRightHandItem.GetComponent<Weapon>();
			}
			if (weapon)
			{
				isWeaponDone = weapon.ContinueSubAction(time);
			}

			return isCreatureDone && isWeaponDone;
		}
		override public void FinishSubAction(ulong time) 
		{
			Map.instance.TryMoveObject(owner, targetTile.tilePosition);
			Creature targetCreature = targetObject.GetComponent<Creature>();

			Weapon weapon = null;
            Equipable equippedRightHandItem = identityCreature.GetEquipment(Equipment.Slot.RIGHT_HAND_WEAPON);
            if (equippedRightHandItem)
            {
                weapon = equippedRightHandItem.GetComponent<Weapon>();
            }
            if (equippedRightHandItem != null)
			{
				weapon = equippedRightHandItem.GetComponent<Weapon>();
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

		public override float GetActionConfidence()
		{
			return .5f;
		}

	}
}
