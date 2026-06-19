using BusBoys.Assets.Scripts.Core.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Generation
{
    public class ProceduralRoadGraphGenerator : MonoBehaviour
    {
        private enum RoadType { None, Straight, Corner, TJunction, Cross }

        [System.Serializable]
        private class CellData
        {
            public bool up, right, down, left;
            public RoadType type = RoadType.None;
            public Quaternion rotation = Quaternion.identity;

            public int ConnectionCount =>
                (up ? 1 : 0) + (right ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0);
        }

        [Header("References")]
        [SerializeField] private NavGraph navGraph;
        [SerializeField] private GenerateStops generateStops;
        [SerializeField] private GraphBootstrap graphBootstrap;

        [Header("Generation")]
        [SerializeField] private RoadGenerationSettings settings;
        [SerializeField] private int seed = 12345;

        [Header("Graph")]
        [SerializeField] private float nodeLinkDistance = 12f;
        [SerializeField] private bool generateOnStart = false;

        [Header("Hierarchy")]
        [SerializeField] private Transform roadsParent;

        private CellData[,] cells;
        private int width, height, cellCount;

        private readonly List<GameObject> spawnedRoads = new();
        private readonly List<NavNode> generatedNodes = new();
        private readonly List<NavEdge> generatedEdges = new();

        public IReadOnlyList<NavNode> GetSpawnedNodes() => generatedNodes; // track these as you instantiate


        private static readonly Vector2Int[] CardinalDirs =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private void Start()
        {
            if (generateOnStart)
            {
                if (roadsParent.childCount == 0)
                {
                    Generate();
                }
                else
                {
                    generateStops.ReturnBusStops();
                    Debug.LogWarning("Road generation was not cleared before playing! Please clear in the inspector next time.");
                }
            }
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (navGraph == null)
            {
                Debug.LogWarning("NavGraph reference ontbreekt.");
                return;
            }

            if (settings.roadStraight == null || settings.roadCorner == null || settings.roadTJunction == null || settings.roadCross == null)
            {
                Debug.LogWarning("Vul roadStraight, roadCorner, roadTJunction en roadCross in.");
                return;
            }

            if (settings.gridSize.x < 3 || settings.gridSize.y < 3)
            {
                Debug.LogWarning("Grid moet minimaal 3x3 zijn.");
                return;
            }

            ClearGenerated();
            Random.InitState(seed);

            width = settings.gridSize.x;
            height = settings.gridSize.y;
            cellCount = width * height;

            EnsureRoadParentExists();
            InitializeCells();

            GenerateNetwork();
            SealOpenEnds();
            CleanupInvalidNetwork();
            ResolveCellTypes();

            SpawnPrefabs();
            //BuildGraphFromSpawnedPrefabs();
            generateStops.GenerateStop();
            graphBootstrap.AddNodes();
            graphBootstrap.LinkEdges();
            Debug.Log("Generation Complete");
        }

        [ContextMenu("New Seed + Generate")]
        public void NewSeedAndGenerate()
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Generate();
        }

        [ContextMenu("Clear Generated")]
        public void ClearGenerated()
        {
            generatedNodes.Clear();
            generatedEdges.Clear();

            if (navGraph != null)
            {
                navGraph.Nodes.Clear();
                navGraph.Edges.Clear();
            }

            if (generateStops != null)
                generateStops.ResetStops();

            if (roadsParent != null)
            {
                int childCountBefore = roadsParent.childCount;

                for (int i = roadsParent.childCount - 1; i >= 0; i--)
                {
                    var child = roadsParent.GetChild(i);
                    child.SetParent(null);

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject);
                    else
                        Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
                }

                Debug.Log($"ClearGenerated: removed {childCountBefore} children from roadsParent.");
            }

            spawnedRoads.Clear();
        }

        private void EnsureRoadParentExists()
        {
            if (roadsParent != null) return;

            var holder = new GameObject("Generated Roads");
            holder.transform.SetParent(transform);
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;
            roadsParent = holder.transform;
        }

        private void InitializeCells()
        {
            cells = new CellData[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    cells[x, y] = new CellData();
        }

        private void GenerateNetwork()
        {
            List<Vector2Int> anchors = GenerateAnchorPoints();
            if (anchors.Count < 2) return;

            List<Vector2Int> ordered = OrderAnchorsByNearestNeighbor(anchors);

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                List<Vector2Int> path = FindPathAStar(ordered[i], ordered[i + 1], -1);
                if (path != null && path.Count > 1)
                    CarvePath(path);
            }

            int extraCount = Mathf.Min(settings.extraConnections, ordered.Count);
            for (int i = 0; i < extraCount; i++)
            {
                Vector2Int start = ordered[Random.Range(0, ordered.Count)];
                Vector2Int goal = FindBestExtraConnectionTarget(start, ordered);
                if (start == goal) continue;

                List<Vector2Int> path = FindPathAStar(start, goal, -1);
                if (path != null && path.Count > 1)
                    CarvePath(path);
            }
        }

        private List<Vector2Int> GenerateAnchorPoints()
        {
            List<Vector2Int> candidates = new(cellCount);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    candidates.Add(new Vector2Int(x, y));

            Shuffle(candidates);

            int targetCount = Mathf.Clamp(width * height / 16, settings.minAnchorPoints, settings.maxAnchorPoints);
            int dynamicDistance = Mathf.Clamp(Mathf.Min(width, height) / 3, 2, 8);
            int requiredDistance = Mathf.Max(2, Mathf.Max(settings.minAnchorDistance, dynamicDistance));
            int maxAttempts = Mathf.Max(candidates.Count, targetCount * settings.anchorAttemptsMultiplier);

            List<Vector2Int> anchors = new(targetCount);
            int attempts = 0;

            for (int i = 0; i < candidates.Count && attempts < maxAttempts && anchors.Count < targetCount; i++, attempts++)
            {
                Vector2Int candidate = candidates[i];

                if (settings.preferEdgeUsage && IsEdgeCell(candidate))
                {
                    if (IsFarEnoughFromAll(candidate, anchors, Mathf.Max(2, requiredDistance - 1)))
                        anchors.Add(candidate);
                    continue;
                }

                if (IsFarEnoughFromAll(candidate, anchors, requiredDistance))
                    anchors.Add(candidate);
            }

            if (anchors.Count < 2 && candidates.Count >= 2)
            {
                anchors.Clear();
                anchors.Add(candidates[0]);
                anchors.Add(candidates[candidates.Count - 1]);
            }

            return anchors;
        }

        private bool IsFarEnoughFromAll(Vector2Int candidate, List<Vector2Int> existing, int minDistance)
        {
            foreach (var point in existing)
                if (ManhattanDistance(candidate, point) < minDistance)
                    return false;
            return true;
        }

        private List<Vector2Int> OrderAnchorsByNearestNeighbor(List<Vector2Int> anchors)
        {
            List<Vector2Int> remaining = new(anchors);
            List<Vector2Int> ordered = new(anchors.Count);

            Vector2Int current = remaining[0];
            ordered.Add(current);
            remaining.RemoveAt(0);

            while (remaining.Count > 0)
            {
                int bestIndex = 0;
                int bestDist = ManhattanDistance(current, remaining[0]);

                for (int i = 1; i < remaining.Count; i++)
                {
                    int d = ManhattanDistance(current, remaining[i]);
                    if (d < bestDist) { bestDist = d; bestIndex = i; }
                }

                current = remaining[bestIndex];
                ordered.Add(current);
                remaining.RemoveAt(bestIndex);
            }

            return ordered;
        }

        private Vector2Int FindBestExtraConnectionTarget(Vector2Int source, List<Vector2Int> anchors)
        {
            Vector2Int best = source;
            int bestScore = int.MaxValue;

            foreach (var candidate in anchors)
            {
                if (candidate == source) continue;
                int distance = ManhattanDistance(source, candidate);
                if (distance < 3 || distance >= bestScore) continue;
                bestScore = distance;
                best = candidate;
            }

            return best;
        }

        private List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int goal, int blockedStartNeighborIndex)
        {
            float[] gScore = new float[cellCount];
            float[] fScore = new float[cellCount];
            int[] cameFrom = new int[cellCount];
            int[] moveDir = new int[cellCount];
            byte[] state = new byte[cellCount];
            List<int> openSet = new(64);

            for (int i = 0; i < cellCount; i++)
            {
                gScore[i] = float.MaxValue;
                fScore[i] = float.MaxValue;
                cameFrom[i] = -1;
                moveDir[i] = -1;
            }

            int startIndex = ToIndex(start);
            int goalIndex = ToIndex(goal);

            gScore[startIndex] = 0f;
            fScore[startIndex] = Heuristic(start, goal);
            openSet.Add(startIndex);
            state[startIndex] = 1;

            while (openSet.Count > 0)
            {
                int currentIndex = GetLowestFScoreIndex(openSet, fScore);
                Vector2Int current = FromIndex(currentIndex);

                if (currentIndex == goalIndex)
                    return ReconstructPath(cameFrom, currentIndex);

                RemoveFromOpenSet(openSet, currentIndex);
                state[currentIndex] = 2;

                for (int dirIndex = 0; dirIndex < 4; dirIndex++)
                {
                    if (currentIndex == startIndex && dirIndex == blockedStartNeighborIndex)
                        continue;

                    Vector2Int neighbor = current + CardinalDirs[dirIndex];
                    if (!IsInsideGrid(neighbor)) continue;

                    int neighborIndex = ToIndex(neighbor);
                    if (state[neighborIndex] == 2) continue;
                    if (!CanTraverseBetween(current, neighbor)) continue;

                    float tentativeG = gScore[currentIndex] +
                        GetTraversalCost(current, neighbor, moveDir[currentIndex], dirIndex);

                    if (tentativeG >= gScore[neighborIndex]) continue;

                    cameFrom[neighborIndex] = currentIndex;
                    moveDir[neighborIndex] = dirIndex;
                    gScore[neighborIndex] = tentativeG;
                    fScore[neighborIndex] = tentativeG + Heuristic(neighbor, goal);

                    if (state[neighborIndex] != 1)
                    {
                        openSet.Add(neighborIndex);
                        state[neighborIndex] = 1;
                    }
                }
            }

            return null;
        }

        private int GetLowestFScoreIndex(List<int> openSet, float[] fScore)
        {
            int best = openSet[0];
            float bestScore = fScore[best];

            for (int i = 1; i < openSet.Count; i++)
            {
                float score = fScore[openSet[i]];
                if (score < bestScore) { bestScore = score; best = openSet[i]; }
            }

            return best;
        }

        // Swap-remove for O(1) unordered removal
        private void RemoveFromOpenSet(List<int> openSet, int value)
        {
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i] != value) continue;
                int last = openSet.Count - 1;
                openSet[i] = openSet[last];
                openSet.RemoveAt(last);
                return;
            }
        }

        private float GetTraversalCost(Vector2Int current, Vector2Int neighbor, int prevDirIndex, int newDirIndex)
        {
            float cost = 1f;

            if (AreConnected(current, neighbor))
                cost -= settings.reusedRoadCostBonus;

            if (prevDirIndex != -1 && prevDirIndex != newDirIndex)
                cost += settings.turnPenalty;

            if (WouldCreateParallelRoad(current, neighbor))
            {
                if (settings.blockPerfectParallelRoads) return float.MaxValue;
                cost += settings.nearParallelPenalty;
            }

            if (settings.preferEdgeUsage && (IsEdgeCell(current) || IsEdgeCell(neighbor)))
                cost -= 0.08f;

            return Mathf.Max(0.05f, cost);
        }

        private bool CanTraverseBetween(Vector2Int a, Vector2Int b)
        {
            if (!AreAdjacent(a, b)) return false;
            if (AreConnected(a, b)) return true;
            if (settings.disallowAdjacentMajorJunctions && WouldCreateInvalidAdjacentJunctions(a, b)) return false;
            return true;
        }

        private void CarvePath(List<Vector2Int> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (CanConnectCells(path[i], path[i + 1]))
                    ConnectCells(path[i], path[i + 1]);
            }
        }

        private bool CanConnectCells(Vector2Int a, Vector2Int b)
        {
            if (!IsInsideGrid(a) || !IsInsideGrid(b)) return false;
            if (!AreAdjacent(a, b)) return false;
            if (AreConnected(a, b)) return false;
            if (settings.disallowAdjacentMajorJunctions && WouldCreateInvalidAdjacentJunctions(a, b)) return false;
            if (WouldCreateParallelRoad(a, b)) return false;
            return true;
        }

        private bool WouldCreateParallelRoad(Vector2Int a, Vector2Int b)
        {
            Vector2Int delta = b - a;

            if (delta == Vector2Int.right || delta == Vector2Int.left)
            {
                int minX = Mathf.Min(a.x, b.x);
                return HasHorizontalSegment(minX, a.y + 1) || HasHorizontalSegment(minX, a.y - 1);
            }
            else if (delta == Vector2Int.up || delta == Vector2Int.down)
            {
                int minY = Mathf.Min(a.y, b.y);
                return HasVerticalSegment(a.x + 1, minY) || HasVerticalSegment(a.x - 1, minY);
            }

            return false;
        }

        private bool HasHorizontalSegment(int leftX, int y)
        {
            if (leftX < 0 || leftX >= width - 1 || y < 0 || y >= height) return false;
            return cells[leftX, y].right && cells[leftX + 1, y].left;
        }

        private bool HasVerticalSegment(int x, int bottomY)
        {
            if (x < 0 || x >= width || bottomY < 0 || bottomY >= height - 1) return false;
            return cells[x, bottomY].up && cells[x, bottomY + 1].down;
        }

        private bool WouldCreateInvalidAdjacentJunctions(Vector2Int a, Vector2Int b)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighborOfA = a + CardinalDirs[i];
                if (IsInsideGrid(neighborOfA) && IsProjectedMajorJunctionTouchingMajor(neighborOfA, a, b))
                    return true;

                Vector2Int neighborOfB = b + CardinalDirs[i];
                if (IsInsideGrid(neighborOfB) && IsProjectedMajorJunctionTouchingMajor(neighborOfB, a, b))
                    return true;
            }

            return IsProjectedMajorJunctionTouchingMajor(a, a, b) ||
                   IsProjectedMajorJunctionTouchingMajor(b, a, b);
        }

        private bool IsProjectedMajorJunctionTouchingMajor(Vector2Int cell, Vector2Int a, Vector2Int b)
        {
            if (!IsMajorJunction(GetProjectedRoadTypeAfterConnection(cell, a, b)))
                return false;

            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighbor = cell + CardinalDirs[i];
                if (IsInsideGrid(neighbor) && IsMajorJunction(GetProjectedRoadTypeAfterConnection(neighbor, a, b)))
                    return true;
            }

            return false;
        }

        private bool IsMajorJunction(RoadType type) =>
            type == RoadType.TJunction || type == RoadType.Cross;

        private RoadType GetProjectedRoadTypeAfterConnection(Vector2Int cell, Vector2Int a, Vector2Int b)
        {
            CellData c = cells[cell.x, cell.y];
            bool up = c.up, right = c.right, down = c.down, left = c.left;

            Vector2Int delta = (cell == a) ? b - a : (cell == b) ? a - b : Vector2Int.zero;

            if (delta == Vector2Int.up) up = true;
            else if (delta == Vector2Int.right) right = true;
            else if (delta == Vector2Int.down) down = true;
            else if (delta == Vector2Int.left) left = true;

            return DetermineRoadType(up, right, down, left);
        }

        private void SealOpenEnds()
        {
            bool changed = true;
            int guard = 0;

            while (changed && guard < 32)
            {
                guard++;
                changed = false;

                List<Vector2Int> deadEnds = GetDeadEnds();
                if (deadEnds.Count == 0) break;

                foreach (var deadEnd in deadEnds)
                {
                    if (cells[deadEnd.x, deadEnd.y].ConnectionCount != 1) continue;

                    if (TrySealDeadEnd(deadEnd))
                        changed = true;
                    else
                    {
                        RemoveDeadEndBranch(deadEnd);
                        changed = true;
                    }
                }
            }
        }

        private List<Vector2Int> GetDeadEnds()
        {
            List<Vector2Int> result = new();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (cells[x, y].ConnectionCount == 1)
                        result.Add(new Vector2Int(x, y));
            return result;
        }

        private bool TrySealDeadEnd(Vector2Int deadEnd)
        {
            Vector2Int blockedNeighbor = GetOnlyConnectedNeighbor(deadEnd);
            int blockedDirIndex = GetDirectionIndex(blockedNeighbor - deadEnd);
            List<Vector2Int> candidates = GetSealTargetCandidates(deadEnd, blockedNeighbor);

            foreach (var candidate in candidates)
            {
                List<Vector2Int> path = FindPathAStar(deadEnd, candidate, blockedDirIndex);
                if (path == null || path.Count < 2) continue;

                CarvePath(path);
                if (cells[deadEnd.x, deadEnd.y].ConnectionCount >= 2)
                    return true;
            }

            return false;
        }

        private Vector2Int GetOnlyConnectedNeighbor(Vector2Int cell)
        {
            CellData c = cells[cell.x, cell.y];
            if (c.up) return cell + Vector2Int.up;
            if (c.right) return cell + Vector2Int.right;
            if (c.down) return cell + Vector2Int.down;
            if (c.left) return cell + Vector2Int.left;
            return cell;
        }

        private List<Vector2Int> GetSealTargetCandidates(Vector2Int deadEnd, Vector2Int blockedNeighbor)
        {
            List<Vector2Int> candidates = new();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var candidate = new Vector2Int(x, y);
                    if (candidate == deadEnd || candidate == blockedNeighbor) continue;
                    if (cells[x, y].ConnectionCount < 1) continue;
                    if (ManhattanDistance(deadEnd, candidate) < 3) continue;
                    candidates.Add(candidate);
                }
            }

            candidates.Sort((a, b) => ManhattanDistance(deadEnd, a).CompareTo(ManhattanDistance(deadEnd, b)));
            return candidates;
        }

        private void RemoveDeadEndBranch(Vector2Int start)
        {
            Vector2Int current = start;
            while (IsInsideGrid(current) && cells[current.x, current.y].ConnectionCount == 1)
            {
                Vector2Int next = GetOnlyConnectedNeighbor(current);
                DisconnectCells(current, next);
                current = next;
            }
        }

        private void CleanupInvalidNetwork()
        {
            bool changed = true;
            int guard = 0;

            while (changed && guard < 24)
            {
                guard++;
                changed = false;

                List<Vector2Int> deadEnds = new();
                List<(Vector2Int a, Vector2Int b)> invalidJunctionLinks = new();

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Vector2Int cellPos = new Vector2Int(x, y);
                        CellData cell = cells[x, y];

                        if (cell.ConnectionCount == 1)
                            deadEnds.Add(cellPos);

                        if (!settings.disallowAdjacentMajorJunctions) continue;
                        if (!IsMajorJunction(GetCurrentRoadType(cellPos))) continue;

                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int neighbor = cellPos + CardinalDirs[i];
                            if (!IsInsideGrid(neighbor)) continue;
                            if (!AreConnected(cellPos, neighbor)) continue;
                            if (!IsMajorJunction(GetCurrentRoadType(neighbor))) continue;

                            // Record the pair with lower-coordinate cell first to avoid duplicates
                            if (cellPos.x < neighbor.x || (cellPos.x == neighbor.x && cellPos.y < neighbor.y))
                                invalidJunctionLinks.Add((cellPos, neighbor));
                        }
                    }
                }

                if (deadEnds.Count == 0 && invalidJunctionLinks.Count == 0) break;

                changed = true;

                foreach (var deadEnd in deadEnds)
                    RemoveAllConnections(deadEnd);

                foreach (var (a, b) in invalidJunctionLinks)
                    DisconnectCells(a, b);
            }
        }

        private RoadType GetCurrentRoadType(Vector2Int cell)
        {
            CellData c = cells[cell.x, cell.y];
            return DetermineRoadType(c.up, c.right, c.down, c.left);
        }

        private void RemoveAllConnections(Vector2Int p)
        {
            CellData c = cells[p.x, p.y];
            if (c.up) DisconnectCells(p, p + Vector2Int.up);
            if (c.right) DisconnectCells(p, p + Vector2Int.right);
            if (c.down) DisconnectCells(p, p + Vector2Int.down);
            if (c.left) DisconnectCells(p, p + Vector2Int.left);
        }

        private void ConnectCells(Vector2Int a, Vector2Int b)
        {
            Vector2Int delta = b - a;
            if (delta == Vector2Int.up) { cells[a.x, a.y].up = true; cells[b.x, b.y].down = true; }
            else if (delta == Vector2Int.right) { cells[a.x, a.y].right = true; cells[b.x, b.y].left = true; }
            else if (delta == Vector2Int.down) { cells[a.x, a.y].down = true; cells[b.x, b.y].up = true; }
            else if (delta == Vector2Int.left) { cells[a.x, a.y].left = true; cells[b.x, b.y].right = true; }
        }

        private void DisconnectCells(Vector2Int a, Vector2Int b)
        {
            Vector2Int delta = b - a;
            if (delta == Vector2Int.up) { cells[a.x, a.y].up = false; cells[b.x, b.y].down = false; }
            else if (delta == Vector2Int.right) { cells[a.x, a.y].right = false; cells[b.x, b.y].left = false; }
            else if (delta == Vector2Int.down) { cells[a.x, a.y].down = false; cells[b.x, b.y].up = false; }
            else if (delta == Vector2Int.left) { cells[a.x, a.y].left = false; cells[b.x, b.y].right = false; }
        }

        private void ResolveCellTypes()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CellData cell = cells[x, y];
                    cell.type = DetermineRoadType(cell.up, cell.right, cell.down, cell.left);
                    cell.rotation = Quaternion.identity;

                    switch (cell.type)
                    {
                        case RoadType.Straight:
                            if (cell.left && cell.right)
                                cell.rotation = Quaternion.Euler(0f, 90f, 0f);
                            break;

                        case RoadType.Corner:
                            if (cell.left && cell.down) cell.rotation = Quaternion.Euler(0f, 90f, 0f);
                            else if (cell.down && cell.right) cell.rotation = Quaternion.identity;
                            else if (cell.right && cell.up) cell.rotation = Quaternion.Euler(0f, 270f, 0f);
                            else if (cell.up && cell.left) cell.rotation = Quaternion.Euler(0f, 180f, 0f);
                            break;

                        case RoadType.TJunction:
                            if (cell.left && cell.right && cell.down) cell.rotation = Quaternion.Euler(0f, 90f, 0f);
                            else if (cell.down && cell.left && cell.up) cell.rotation = Quaternion.Euler(0f, 180f, 0f);
                            else if (cell.left && cell.right && cell.up) cell.rotation = Quaternion.Euler(0f, 270f, 0f);
                            else if (cell.down && cell.right && cell.up) cell.rotation = Quaternion.identity;
                            break;

                        case RoadType.Cross:
                            cell.rotation = Quaternion.identity;
                            break;
                    }
                }
            }
        }

        private RoadType DetermineRoadType(bool up, bool right, bool down, bool left)
        {
            int count = (up ? 1 : 0) + (right ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0);

            if (count < 2) return RoadType.None;
            if (count == 4) return RoadType.Cross;
            if (count == 3) return RoadType.TJunction;

            return (left && right) || (up && down) ? RoadType.Straight : RoadType.Corner;
        }

        private void SpawnPrefabs()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CellData cell = cells[x, y];
                    if (cell.type == RoadType.None) continue;

                    GameObject prefab = GetPrefabForType(cell.type);
                    if (prefab == null) continue;

                    Vector3 worldPos = transform.position + new Vector3(x * settings.cellSize, 0f, y * settings.cellSize);
                    GameObject instance = Instantiate(prefab, worldPos, cell.rotation, roadsParent);
                    var node = instance.GetComponentInChildren<NavNode>();
                    if (node != null) generatedNodes.Add(node);
                    instance.name = $"{cell.type}_{x}_{y}";
                    spawnedRoads.Add(instance);
                }
            }
        }

        private GameObject GetPrefabForType(RoadType type) => type switch
        {
            RoadType.Straight => settings.roadStraight,
            RoadType.Corner => settings.roadCorner,
            RoadType.TJunction => settings.roadTJunction,
            RoadType.Cross => settings.roadCross,
            _ => null
        };

        private int ToIndex(Vector2Int p) => p.y * width + p.x;
        private Vector2Int FromIndex(int index) => new(index % width, index / width);
        private float Heuristic(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        private int ManhattanDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        private bool IsInsideGrid(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
        private bool IsEdgeCell(Vector2Int p) => p.x == 0 || p.y == 0 || p.x == width - 1 || p.y == height - 1;

        private bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            Vector2Int delta = b - a;
            return delta == Vector2Int.up || delta == Vector2Int.right ||
                   delta == Vector2Int.down || delta == Vector2Int.left;
        }

        private bool AreConnected(Vector2Int a, Vector2Int b)
        {
            Vector2Int delta = b - a;
            CellData c = cells[a.x, a.y];
            if (delta == Vector2Int.up) return c.up;
            if (delta == Vector2Int.right) return c.right;
            if (delta == Vector2Int.down) return c.down;
            if (delta == Vector2Int.left) return c.left;
            return false;
        }

        private int GetDirectionIndex(Vector2Int delta)
        {
            if (delta == Vector2Int.up) return 0;
            if (delta == Vector2Int.right) return 1;
            if (delta == Vector2Int.down) return 2;
            if (delta == Vector2Int.left) return 3;
            return -1;
        }

        private List<Vector2Int> ReconstructPath(int[] cameFrom, int currentIndex)
        {
            List<Vector2Int> path = new();
            for (int cursor = currentIndex; cursor != -1; cursor = cameFrom[cursor])
                path.Add(FromIndex(cursor));
            path.Reverse();
            return path;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int swapIndex = Random.Range(i, list.Count);
                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            for (int x = 0; x < settings.gridSize.x; x++)
                for (int y = 0; y < settings.gridSize.y; y++)
                {
                    Vector3 pos = transform.position + new Vector3(x * settings.cellSize, 0f, y * settings.cellSize);
                    Gizmos.DrawWireCube(pos, new Vector3(settings.cellSize, 0.1f, settings.cellSize));
                }
        }
    }
}