namespace Noble.TileEngine
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using System;

    public class Tile : MonoBehaviour
    {
        public Vector2Int position = Vector2Int.zero;
        public int x => position.x;
        public int y => position.y;
        public Map map;
        public bool isDirty;
        public float gapBetweenLayers = .1f;

        public bool isInView = false;
        public bool isLit = false;
        public bool isAlwaysLit = false;

        public LinkedList<DungeonObject> objectList = new LinkedList<DungeonObject>();

        public void Init(Map map, int x, int y)
        {
            //transform.parent = map.transform;
            this.map = map;
            position.x = x;
            position.y = y;
            map.tilesThatAllowSpawn.Add(this);
            transform.localPosition = new Vector3(x * map.tileWidth, y * map.tileHeight, 0);
            if (isAlwaysLit) isLit = true;
            
            //SetInView(true);
        }

        public bool ContainsObjectWithComponent<T>() where T : MonoBehaviour
        {
            foreach (var hay in objectList)
            {
                if (hay.GetComponent<T>() != null) return true;
            }

            return false;
        }

        public bool ContainsObjectOfType(DungeonObject needle)
        {
            foreach (var hay in objectList)
            {
                if (needle.objectName == hay.objectName) return true;
            }

            return false;
        }

        public bool ContainsObjectOfType(DungeonObject[] needles)
        {
            foreach (var hay in objectList)
            {
                foreach (var needle in needles) if (needle.objectName == hay.objectName) return true;
            }

            return false;
        }

        public bool ContainsObjectOfType(string needle)
        {
            foreach (var hay in objectList)
            {
                if (needle == hay.objectName) return true;
            }

            return false;
        }

        public bool ContainsObjectOfType(string[] needles)
        {
            foreach (var hay in objectList)
            {
                foreach (var needle in needles) if (needle == hay.objectName) return true;
            }

            return false;
        }

        public void SetInView(bool isVisible)
        {
            isInView = isVisible;
            if (isInView)
            {
                foreach (var ob in objectList)
                {
                    if (ob.glyphsOb)
                    {
                        ob.SetInView(true, isLit);
                    }
                    if (ob.coversObjectsBeneath) break;
                }
            }
            else
            {
                foreach (var ob in objectList)
                {
                    ob.SetInView(false, isLit);
                }
            }

            //if (!isVisible)
            {
                foreach (var ob in objectList)
                {
                    ob.SetLit(isVisible && isLit, isVisible);
                }
            }
        }

        public void SetLit(bool isLit)
        {
            if (isAlwaysLit) isLit = true;
            //if (isAlwaysLit && isLit == false) return;

            this.isLit = isLit;
            if (isLit)
            {
                foreach (var ob in objectList)
                {
                    if (ob.glyphsOb)
                    {
                        ob.SetLit(isInView, isInView);
                    }
                    if (ob.coversObjectsBeneath) break;
                }
            }
            else
            {
                foreach (var ob in objectList)
                {
                    ob.SetLit(false, isInView);
                }
            }
        }

        public void StepOn(DungeonObject creature)
        {
            foreach (var ob in objectList)
            {
                ob.SteppedOn(creature);
            }
        }

        public void PreStepOn(DungeonObject creature)
        {
            foreach (var ob in objectList)
            {
                ob.PreSteppedOn(creature);
            }
        }

        public void UpdateLighting()
        {
            foreach (var ob in objectList)
            {
                ob.UpdateLighting();
            }
        }

        public DungeonObject SpawnAndAddObject(DungeonObject dungeonObject, int quantity = 1, int layer = 0)
        {
            var ob = Instantiate(dungeonObject).GetComponent<DungeonObject>();
            ob.quantity = quantity;
            ob.transform.position = new Vector3(0, 0, -layer);
            AddObject(ob, false, layer);

            return ob;
        }

        public void DestroyAllObjects()
        {
            foreach (var ob in objectList)
            {
                ob.inventory.DestroyAll();
                Destroy(ob);
            }
            if (!map.tilesThatAllowSpawn.Contains(this))
            {
                map.tilesThatAllowSpawn.Add(this);
            }
            objectList.Clear();
        }

        public void AddObject(DungeonObject ob, bool isMove = false, int layer = 0)
        {
            bool isFirstPlacement = ob.tile == null;

            ob.transform.parent = map.layers[layer];
            ob.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
            //if (addToBottom)
            {
                objectList.AddLast(ob);
            }
            //else
            //{
            //    objectList.AddFirst(ob);
            //}
            if (isMove)
            {
                ob.Move(position);
            }
            else
            {
                ob.SetPosition(position);
            }
            if (ob.preventsObjectSpawning)
            {
                map.tilesThatAllowSpawn.Remove(this);
            }

            //if (ob.illuminationRange != 0)
            {
                //map.UpdateLighting(ob.tilePosition, ob.illuminationRange);
                //map.UpdateLighting();
            }

            SetInView(isInView);
            SetLit(isLit);

            if (isFirstPlacement)
            {
                ob.Spawn();
            }

            UpdateObjectLayers();
        }

        public void UpdateObjectLayers()
        {
            Sort(objectList.First, objectList.Last, o => o.transform.position.z);
            //int l = 0;
            //var el = objectList.Last;
            //while (el != null)
            //{
            //    var ob = el.Value;
            //    ob.transform.localPosition = new Vector3(ob.transform.localPosition.x, ob.transform.localPosition.y, -l * gapBetweenLayers);
            //    l++;
            //    el = el.Previous;
            //}
        }

        private static void Sort<T, R>(LinkedListNode<T> head, LinkedListNode<T> tail, Func<T, R> valueGetter) where R : IComparable<R>
        {
            if (head == tail) return; // there is nothing to sort

            void Swap(LinkedListNode<T> a, LinkedListNode<T> b)
            {
                var tmp = a.Value;
                a.Value = b.Value;
                b.Value = tmp;
            }

            var pivot = tail;
            var node = head;
            while (node.Next != pivot)
            {
                if (valueGetter(node.Value).CompareTo(valueGetter(pivot.Value)) > 0)
                {
                    Swap(pivot, pivot.Previous);
                    Swap(node, pivot);
                    pivot = pivot.Previous; // move pivot backward
                }
                else node = node.Next; // move node forward
            }
            if (valueGetter(node.Value).CompareTo(valueGetter(pivot.Value)) > 0)
            {
                Swap(node, pivot);
                pivot = node;
            }

            // pivot location is found, now sort nodes below and above pivot.
            // if head or tail is equal to pivot we reached boundaries and we should stop recursion.
            if (head != pivot) Sort(head, pivot.Previous, valueGetter);
            if (tail != pivot) Sort(pivot.Next, tail, valueGetter);
        }

        public void RemoveObject(DungeonObject ob, bool destroyObject = false)
        {
            ob.transform.parent = null;
            objectList.Remove(ob);
            if (destroyObject)
            {
                Destroy(ob.gameObject);
            }

            if (AllowsSpawn())
            {
                if (!map.tilesThatAllowSpawn.Contains(this))
                {
                    map.tilesThatAllowSpawn.Add(this);
                }
            }

            //if (ob.illuminationRange != 0)
            {
                //map.UpdateLighting(ob.tilePosition, ob.illuminationRange);
                //map.UpdateLighting();
            }

            SetInView(isInView);
            SetLit(isLit);

            UpdateObjectLayers();
        }

        public void RemoveAllObjects()
        {
            if (objectList == null || objectList.Count == 0) return;

            foreach (var dOb in objectList)
            {
                Destroy(dOb.gameObject);
            }

            objectList.Clear();
            if (!map.tilesThatAllowSpawn.Contains(this))
            {
                map.tilesThatAllowSpawn.Add(this);
            }
        }

        public bool IsCollidable()
        {
            foreach (var ob in objectList)
            {
                if (ob.isCollidable) return true;
            }
            return false;
        }

        public int GetPathingWeight()
        {
            return objectList.Sum(ob => ob.pathingWeight);
        }

        public bool DoesBlockLineOfSight()
        {
            foreach (var ob in objectList)
            {
                if (ob.blocksLineOfSight) return true;
            }
            return false;
        }

        public bool ContainsCollidableObject()
        {
            return objectList.Any(x => x.isCollidable);
        }

        public bool AllowsSpawn()
        {
            return !objectList.Any(x => x.preventsObjectSpawning);
        }

        public static Tile GetClosestTile(List<Tile> tiles, Vector2 pos)
        {
            Tile closestTile = null;
            float minDistance = float.MaxValue;

            foreach (var tile in tiles)
            {
                Vector2 difference = Map.instance.GetDifference((Vector2)tile.transform.position + Map.instance.tileDimensions / 2, pos);
                float distance = difference.magnitude;
                if (distance < minDistance)
                {
                    closestTile = tile;
                    minDistance = distance;
                }
            }

            return closestTile;
        }
    }
}