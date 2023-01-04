namespace Noble.TileEngine
{
    using UnityEngine;
    using System.Collections.Generic;
    using Noble.TileEngine;
    using System.Linq;

    // Uses standard A* to find optimal paths between tiles
    public static class PathUtil
    {
        static readonly (int, int)[] neighbors_indexes = new (int, int)[] {
            ( -1, -1 ), ( +0, -1 ), ( +1, -1 ),
            ( -1, +0 ),             ( +1, +0 ),
            ( -1, +1 ), ( +0, +1 ), ( +1, +1 )
        };

        static readonly float[] neighbor_weights = new float[] {
            1.44f,  1.00f,  1.44f,
            1.00f,          1.00f,
            1.44f,  1.00f,  1.44f
        };

        class BinaryHeapSet<T1, T2>
        {
            HashSet<T1> openSet = new();
            PriorityQueue<T1, T2> openQueue = new();

            public int Count => openSet.Count;
            public T1 Peek() => openQueue.Peek();
            public bool Contains(T1 node) => openSet.Contains(node);

            public void Enqueue(T1 node, T2 priority)
            {
                openSet.Add(node);
                openQueue.Enqueue(node, priority);
            }

            public T1 Dequeue()
            {
                openSet.Remove(openQueue.Peek());
                return openQueue.Dequeue();
            }

        }

        private static List<Tile> constructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
        {
            List<Tile> path = new();
            path.Add(current);

            while (cameFrom.Keys.Contains(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }

            return path;
        }

        public static List<Tile> FindPath(Tile startTile, Tile endTile)
        {
            BinaryHeapSet<Tile, float> openSet = new();
            openSet.Enqueue(startTile, 0);

            Dictionary<Tile, Tile> cameFrom = new();

            // For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
            Dictionary<Tile, float> gScore = new();
            Map.instance.ForEachTile(t => gScore.Add(t, float.PositiveInfinity));
            gScore[startTile] = 0;

            // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
            // how cheap a path could be from start to finish if it goes through n.
            Dictionary<Tile, float> fScore = new();
            Map.instance.ForEachTile(t => fScore.Add(t, float.PositiveInfinity));
            fScore[startTile] = distance(startTile, endTile);

            while (openSet.Count != 0)
            {
                // This operation can occur in O(Log(N)) time if openSet is a min-heap or a priority queue
                Tile current = openSet.Peek();
                if (current == endTile)
                {
                    return constructPath(cameFrom, current);
                }

                openSet.Dequeue();
                for (int i = 0; i < neighbors_indexes.Length; i++)
                { 
                    (int, int) neighborIndex = neighbors_indexes[i];
                    float neighborWeight = neighbor_weights[i];
                    Tile neighbor = Map.instance.GetTile(current.x + neighborIndex.Item1, current.y + neighborIndex.Item2);
                    // d(current,neighbor) is the weight of the edge from current to neighbor
                    // tentative_gScore is the distance from start to the neighbor through current
                    float tentative_gScore = gScore[current] + neighborWeight * neighbor.GetPathingWeight();
                    if (tentative_gScore < gScore[neighbor])
                    {
                        // This path to neighbor is better than any previous one. Record it!
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;
                        fScore[neighbor] = tentative_gScore + distance(neighbor, endTile);
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }

            return null;
        }

        private static float distance(Tile currentTile, Tile endTile)
        {
            return Mathf.Sqrt(Mathf.Pow(endTile.x - currentTile.x, 2) + Mathf.Pow(endTile.y - currentTile.y, 2));
        }
    }
}
