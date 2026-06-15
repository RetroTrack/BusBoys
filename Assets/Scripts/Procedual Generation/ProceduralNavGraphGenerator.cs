using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public class ProceduralRoadGraphGenerator : MonoBehaviour
    {
        private enum RoadType
        {
            None,
            Straight,
            Corner,
            TJunction
        }

        [System.Serializable]
        private class CellData
        {
            public bool up;
            public bool right;
            public bool down;
            public bool left;

            public RoadType type = RoadType.None;
            public Quaternion rotation = Quaternion.identity;

            public int ConnectionCount =>
                (up ? 1 : 0) +
                (right ? 1 : 0) +
                (down ? 1 : 0) +
                (left ? 1 : 0);
        }

        [Header("References")]
        [SerializeField] private NavGraph navGraph;

        [Header("Grid")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 8);
        [SerializeField] private float cellSize = 20f;
        [SerializeField] private int seed = 12345;

        [Header("Prefabs")]
        [SerializeField] private GameObject roadStraight;
        [SerializeField] private GameObject roadCorner;
        [SerializeField] private GameObject roadTJunction;

        [Header("Inner roads")]
        [Range(0f, 1f)]
        [SerializeField] private float horizontalRoadChance = 0.45f;

        [Range(0f, 1f)]
        [SerializeField] private float verticalRoadChance = 0.45f;

        [SerializeField] private float nodeLinkDistance = 12f;
        [SerializeField] private bool generateOnStart = false;

        [Header("Hierarchy")]
        [SerializeField] private Transform roadsParent;

        private CellData[,] cells;
        private readonly List<GameObject> spawnedRoads = new();
        private readonly List<NavNode> generatedNodes = new();
        private readonly List<NavEdge> generatedEdges = new();

        private void Start()
        {
            if (generateOnStart)
                Generate();
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (navGraph == null)
            {
                Debug.LogWarning("NavGraph reference ontbreekt.");
                return;
            }

            if (roadStraight == null || roadCorner == null || roadTJunction == null)
            {
                Debug.LogWarning("Vul roadStraight, roadCorner en roadTJunction in.");
                return;
            }

            if (gridSize.x < 3 || gridSize.y < 3)
            {
                Debug.LogWarning("Grid moet minimaal 3x3 zijn voor een buitenring.");
                return;
            }

            ClearGenerated();
            Random.InitState(seed);

            if (roadsParent == null)
            {
                GameObject holder = new GameObject("Generated Roads");
                holder.transform.SetParent(transform);
                holder.transform.localPosition = Vector3.zero;
                holder.transform.localRotation = Quaternion.identity;
                roadsParent = holder.transform;
            }

            cells = new CellData[gridSize.x, gridSize.y];
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    cells[x, y] = new CellData();
                }
            }

            BuildOuterLoop();
            BuildInnerRoads();
            CleanupInvalidNetwork();
            ResolveCellTypes();
            SpawnPrefabs();
            BuildGraphFromSpawnedPrefabs();
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

            if (roadsParent != null)
            {
                for (int i = roadsParent.childCount - 1; i >= 0; i--)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(roadsParent.GetChild(i).gameObject);
                    else
                        Destroy(roadsParent.GetChild(i).gameObject);
#else
                    Destroy(roadsParent.GetChild(i).gameObject);
#endif
                }
            }

            spawnedRoads.Clear();
        }

        private void BuildOuterLoop()
        {
            for (int x = 0; x < gridSize.x - 1; x++)
            {
                ConnectCells(new Vector2Int(x, 0), new Vector2Int(x + 1, 0));
                ConnectCells(new Vector2Int(x, gridSize.y - 1), new Vector2Int(x + 1, gridSize.y - 1));
            }

            for (int y = 0; y < gridSize.y - 1; y++)
            {
                ConnectCells(new Vector2Int(0, y), new Vector2Int(0, y + 1));
                ConnectCells(new Vector2Int(gridSize.x - 1, y), new Vector2Int(gridSize.x - 1, y + 1));
            }
        }

        private void BuildInnerRoads()
        {
            for (int y = 1; y < gridSize.y - 1; y++)
            {
                if (Random.value > horizontalRoadChance)
                    continue;

                int startX = 1;
                int endX = gridSize.x - 2;

                for (int x = startX; x < endX; x++)
                {
                    if (WouldCreateFourWay(new Vector2Int(x, y), new Vector2Int(x + 1, y)))
                        continue;

                    ConnectCells(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                }

                TryConnectHorizontalRowToBorder(y);
            }

            for (int x = 1; x < gridSize.x - 1; x++)
            {
                if (Random.value > verticalRoadChance)
                    continue;

                int startY = 1;
                int endY = gridSize.y - 2;

                for (int y = startY; y < endY; y++)
                {
                    if (WouldCreateFourWay(new Vector2Int(x, y), new Vector2Int(x, y + 1)))
                        continue;

                    ConnectCells(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }

                TryConnectVerticalColumnToBorder(x);
            }
        }

        private void TryConnectHorizontalRowToBorder(int y)
        {
            Vector2Int leftInner = new Vector2Int(1, y);
            Vector2Int rightInner = new Vector2Int(gridSize.x - 2, y);
            Vector2Int leftBorder = new Vector2Int(0, y);
            Vector2Int rightBorder = new Vector2Int(gridSize.x - 1, y);

            if (!WouldCreateFourWay(leftInner, leftBorder))
                ConnectCells(leftInner, leftBorder);

            if (!WouldCreateFourWay(rightInner, rightBorder))
                ConnectCells(rightInner, rightBorder);
        }

        private void TryConnectVerticalColumnToBorder(int x)
        {
            Vector2Int bottomInner = new Vector2Int(x, 1);
            Vector2Int topInner = new Vector2Int(x, gridSize.y - 2);
            Vector2Int bottomBorder = new Vector2Int(x, 0);
            Vector2Int topBorder = new Vector2Int(x, gridSize.y - 1);

            if (!WouldCreateFourWay(bottomInner, bottomBorder))
                ConnectCells(bottomInner, bottomBorder);

            if (!WouldCreateFourWay(topInner, topBorder))
                ConnectCells(topInner, topBorder);
        }

        private bool WouldCreateFourWay(Vector2Int a, Vector2Int b)
        {
            int aCount = GetProjectedConnectionCount(a, b);
            int bCount = GetProjectedConnectionCount(b, a);
            return aCount > 3 || bCount > 3;
        }

        private int GetProjectedConnectionCount(Vector2Int cell, Vector2Int extraNeighbor)
        {
            CellData c = cells[cell.x, cell.y];
            int count = c.ConnectionCount;

            Vector2Int delta = extraNeighbor - cell;

            if (delta == Vector2Int.up && !c.up) count++;
            else if (delta == Vector2Int.right && !c.right) count++;
            else if (delta == Vector2Int.down && !c.down) count++;
            else if (delta == Vector2Int.left && !c.left) count++;

            return count;
        }

        private void CleanupInvalidNetwork()
        {
            bool changed = true;
            int guard = 0;

            while (changed && guard < 20)
            {
                guard++;
                changed = false;

                List<Vector2Int> toRemove = new();

                for (int x = 1; x < gridSize.x - 1; x++)
                {
                    for (int y = 1; y < gridSize.y - 1; y++)
                    {
                        CellData c = cells[x, y];
                        int count = c.ConnectionCount;

                        if (count == 1)
                            toRemove.Add(new Vector2Int(x, y));
                    }
                }

                if (toRemove.Count == 0)
                    break;

                changed = true;

                foreach (Vector2Int pos in toRemove)
                {
                    RemoveAllConnections(pos);
                }
            }
        }

        private void RemoveAllConnections(Vector2Int p)
        {
            if (cells[p.x, p.y].up)
                DisconnectCells(p, p + Vector2Int.up);

            if (cells[p.x, p.y].right)
                DisconnectCells(p, p + Vector2Int.right);

            if (cells[p.x, p.y].down)
                DisconnectCells(p, p + Vector2Int.down);

            if (cells[p.x, p.y].left)
                DisconnectCells(p, p + Vector2Int.left);
        }

        private void ConnectCells(Vector2Int a, Vector2Int b)
        {
            Vector2Int delta = b - a;

            if (delta == Vector2Int.up)
            {
                cells[a.x, a.y].up = true;
                cells[b.x, b.y].down = true;
            }
            else if (delta == Vector2Int.right)
            {
                cells[a.x, a.y].right = true;
                cells[b.x, b.y].left = true;
            }
            else if (delta == Vector2Int.down)
            {
                cells[a.x, a.y].down = true;
                cells[b.x, b.y].up = true;
            }
            else if (delta == Vector2Int.left)
            {
                cells[a.x, a.y].left = true;
                cells[b.x, b.y].right = true;
            }
        }

        private void DisconnectCells(Vector2Int a, Vector2Int b)
        {
            Vector2Int delta = b - a;

            if (delta == Vector2Int.up)
            {
                cells[a.x, a.y].up = false;
                cells[b.x, b.y].down = false;
            }
            else if (delta == Vector2Int.right)
            {
                cells[a.x, a.y].right = false;
                cells[b.x, b.y].left = false;
            }
            else if (delta == Vector2Int.down)
            {
                cells[a.x, a.y].down = false;
                cells[b.x, b.y].up = false;
            }
            else if (delta == Vector2Int.left)
            {
                cells[a.x, a.y].left = false;
                cells[b.x, b.y].right = false;
            }
        }

        private void ResolveCellTypes()
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    CellData cell = cells[x, y];
                    int count = cell.ConnectionCount;

                    if (count < 2 || count > 3)
                    {
                        cell.type = RoadType.None;
                        continue;
                    }

                    if (count == 2)
                    {
                        if ((cell.left && cell.right) || (cell.up && cell.down))
                        {
                            cell.type = RoadType.Straight;

                            if (cell.left && cell.right)
                                cell.rotation = Quaternion.Euler(0f, 90f, 0f);
                            else
                                cell.rotation = Quaternion.Euler(0f, 0f, 0f);
                        }
                        else
                        {
                            cell.type = RoadType.Corner;

                            if (cell.left && cell.down)
                                cell.rotation = Quaternion.Euler(0f, 90f, 0f);
                            else if (cell.down && cell.right)
                                cell.rotation = Quaternion.Euler(0f, 0f, 0f);
                            else if (cell.right && cell.up)
                                cell.rotation = Quaternion.Euler(0f, 270f, 0f);
                            else if (cell.up && cell.left)
                                cell.rotation = Quaternion.Euler(0f, 180f, 0f);
                        }
                    }
                    else if (count == 3)
                    {
                        cell.type = RoadType.TJunction;

                        if (cell.left && cell.right && cell.down)
                            cell.rotation = Quaternion.Euler(0f, 90f, 0f);
                        else if (cell.down && cell.left && cell.up)
                            cell.rotation = Quaternion.Euler(0f, 180f, 0f);
                        else if (cell.left && cell.right && cell.up)
                            cell.rotation = Quaternion.Euler(0f, 270f, 0f);
                        else if (cell.down && cell.right && cell.up)
                            cell.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                }
            }
        }

        private void SpawnPrefabs()
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    CellData cell = cells[x, y];
                    if (cell.type == RoadType.None)
                        continue;

                    GameObject prefab = GetPrefabForType(cell.type);
                    if (prefab == null)
                        continue;

                    Vector3 worldPos = transform.position + new Vector3(x * cellSize, 0f, y * cellSize);
                    GameObject instance = Instantiate(prefab, worldPos, cell.rotation, roadsParent);
                    instance.name = $"{cell.type}_{x}_{y}";
                    spawnedRoads.Add(instance);
                }
            }
        }

        private void BuildGraphFromSpawnedPrefabs()
        {
            navGraph.Nodes.Clear();
            navGraph.Edges.Clear();
            generatedNodes.Clear();
            generatedEdges.Clear();

            foreach (GameObject road in spawnedRoads)
            {
                NavNode[] nodes = road.GetComponentsInChildren<NavNode>(true);
                NavEdge[] edges = road.GetComponentsInChildren<NavEdge>(true);

                foreach (NavNode node in nodes)
                {
                    generatedNodes.Add(node);
                    navGraph.Nodes.Add(node);
                }

                foreach (NavEdge edge in edges)
                {
                    generatedEdges.Add(edge);
                    navGraph.Edges.Add(edge);
                }
            }

            LinkNearbyNodes();
            AssignClosestNodeToEdges();
        }

        private void LinkNearbyNodes()
        {
            for (int i = 0; i < generatedNodes.Count; i++)
            {
                for (int j = i + 1; j < generatedNodes.Count; j++)
                {
                    NavNode a = generatedNodes[i];
                    NavNode b = generatedNodes[j];

                    float dist = Vector3.Distance(a.transform.position, b.transform.position);
                    if (dist <= nodeLinkDistance)
                    {
                        a.AddNeighbor(b);
                        b.AddNeighbor(a);
                    }
                }
            }
        }

        private void AssignClosestNodeToEdges()
        {
            foreach (NavEdge edge in generatedEdges)
            {
                NavNode closest = null;
                float closestDist = float.MaxValue;

                foreach (NavNode node in generatedNodes)
                {
                    float dist = Vector3.Distance(edge.transform.position, node.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = node;
                    }
                }

                edge.node = closest;
            }
        }

        private GameObject GetPrefabForType(RoadType type)
        {
            switch (type)
            {
                case RoadType.Straight: return roadStraight;
                case RoadType.Corner: return roadCorner;
                case RoadType.TJunction: return roadTJunction;
                default: return null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 pos = transform.position + new Vector3(x * cellSize, 0f, y * cellSize);
                    Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }
    }
}