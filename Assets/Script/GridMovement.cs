using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class GridMovement : MonoBehaviour
{
    public Tilemap tilemap;  // Reference to the Tilemap component
    public float movementSpeed = 5f;  // Speed of player movement
    private Vector3 targetPosition;  // Where the player will move to
    private bool isMoving = false;

    public GameObject cursorHighlightPrefab;  // Prefab for cursor highlight
    private GameObject cursorHighlightInstance;  // Instance of the cursor highlight

    // For pathfinding
    private List<Vector3> path = new List<Vector3>();
    private int currentPathIndex = 0;
    public LayerMask obstacleLayer;  // Layer used for obstacles

    private Camera mainCamera;
    private const int maxSteps = 1000;  // Limit the maximum steps for pathfinding

    void Start()
    {
        // Cache the main camera
        mainCamera = Camera.main;

        // Initialize player position to the tilemap's cell position
        Vector3Int gridPosition = tilemap.WorldToCell(transform.position);
        targetPosition = tilemap.GetCellCenterWorld(gridPosition);
        transform.position = targetPosition;

        // Instantiate the cursor highlight object
        cursorHighlightInstance = Instantiate(cursorHighlightPrefab);
        cursorHighlightInstance.SetActive(false);  // Initially hide the highlight
    }

    void Update()
    {
        HandleMouseInput();
        MovePlayer();
        UpdateCursorHighlight();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Convert mouse world position to grid position
            Vector3Int gridPosition = tilemap.WorldToCell(mouseWorldPos);

            // Ensure the target position is always at the center of the clicked grid cell
            targetPosition = tilemap.GetCellCenterWorld(gridPosition);  // Get the center world position of the clicked cell

            // Use A* to find the most efficient path
            path = FindPath(transform.position, targetPosition);

            // If a valid path is found, start moving along it
            if (path != null && path.Count > 0)
            {
                isMoving = true;
                currentPathIndex = 0;
            }
        }
    }

    void MovePlayer()
    {
        if (isMoving && path != null && currentPathIndex < path.Count)
        {
            Vector3 nextPosition = path[currentPathIndex];

            transform.position = Vector3.MoveTowards(transform.position, nextPosition, movementSpeed * Time.deltaTime);

            // If the player is close enough to the target position, move to the next point in the path
            if (Vector3.Distance(transform.position, nextPosition) < 0.1f)
            {
                currentPathIndex++;
                if (currentPathIndex >= path.Count)
                {
                    transform.position = tilemap.GetCellCenterWorld(tilemap.WorldToCell(targetPosition));  // Snap to exact target position
                    isMoving = false;  // Reached the destination
                }
            }
        }
    }

    void UpdateCursorHighlight()
    {
        // Get the mouse position in screen coordinates
        Vector3 mouseScreenPos = Input.mousePosition;

        // Check if the mouse is within the screen bounds
        if (mouseScreenPos.x >= 0 && mouseScreenPos.x <= Screen.width &&
            mouseScreenPos.y >= 0 && mouseScreenPos.y <= Screen.height)
        {
            // Convert the screen position to world position
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0;  // Ensure the z position is zero for 2D

            // Convert mouse world position to grid position
            Vector3Int gridPosition = tilemap.WorldToCell(mouseWorldPos);

            // Update the cursor highlight position to the exact center of the grid cell
            cursorHighlightInstance.transform.position = tilemap.GetCellCenterWorld(gridPosition);

            // Show the highlight if it's not already active
            if (!cursorHighlightInstance.activeSelf)
            {
                cursorHighlightInstance.SetActive(true);
            }
        }
        else
        {
            // Hide the highlight if the mouse is outside the screen
            if (cursorHighlightInstance.activeSelf)
            {
                cursorHighlightInstance.SetActive(false);
            }
        }
    }

    // A* Pathfinding Algorithm
    List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        Vector3Int startGridPos = tilemap.WorldToCell(startWorldPos);
        Vector3Int targetGridPos = tilemap.WorldToCell(targetWorldPos);

        List<Node> openSet = new List<Node>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        Node startNode = new Node(startGridPos, 0, GetManhattanDistance(startGridPos, targetGridPos));
        openSet.Add(startNode);

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, float> gCost = new Dictionary<Vector3Int, float>();
        gCost[startGridPos] = 0;

        int steps = 0;  // To limit the pathfinding steps
        while (openSet.Count > 0 && steps < maxSteps)
        {
            steps++;

            // Find the node with the lowest fCost
            Node currentNode = GetLowestFCostNode(openSet);
            openSet.Remove(currentNode);

            if (currentNode.gridPosition == targetGridPos)
            {
                return ReconstructPath(cameFrom, currentNode.gridPosition);
            }

            closedSet.Add(currentNode.gridPosition);

            foreach (Vector3Int neighbor in GetNeighbors(currentNode.gridPosition))
            {
                if (closedSet.Contains(neighbor)) continue;
                if (IsObstacle(neighbor)) continue;  // Skip if it's an obstacle

                float tentativeGCost = gCost[currentNode.gridPosition] + GetManhattanDistance(currentNode.gridPosition, neighbor);

                if (!gCost.ContainsKey(neighbor) || tentativeGCost < gCost[neighbor])
                {
                    gCost[neighbor] = tentativeGCost;
                    float fCost = tentativeGCost + GetManhattanDistance(neighbor, targetGridPos);

                    openSet.Add(new Node(neighbor, tentativeGCost, fCost));
                    cameFrom[neighbor] = currentNode.gridPosition;
                }
            }
        }

        Debug.LogWarning("Pathfinding exceeded maximum allowed steps.");
        return null; // Return null if no path is found or max steps exceeded
    }

    Node GetLowestFCostNode(List<Node> nodes)
    {
        Node lowest = nodes[0];
        foreach (Node node in nodes)
        {
            if (node.fCost < lowest.fCost || (node.fCost == lowest.fCost && node.hCost < lowest.hCost))
            {
                lowest = node;
            }
        }
        return lowest;
    }

    List<Vector3Int> GetNeighbors(Vector3Int gridPos)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            gridPos + new Vector3Int(1, 0, 0),
            gridPos + new Vector3Int(-1, 0, 0),
            gridPos + new Vector3Int(0, 1, 0),
            gridPos + new Vector3Int(0, -1, 0)
        };
        return neighbors;
    }

    bool IsObstacle(Vector3Int gridPos)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(gridPos);
        Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f, obstacleLayer);
        return hit != null;
    }

    float GetManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector3> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int currentGridPos)
    {
        List<Vector3> path = new List<Vector3>();
        while (cameFrom.ContainsKey(currentGridPos))
        {
            path.Add(tilemap.GetCellCenterWorld(currentGridPos));
            currentGridPos = cameFrom[currentGridPos];
        }
        path.Reverse();
        return path;
    }

    // Node class for A* algorithm
    class Node
    {
        public Vector3Int gridPosition;
        public float gCost;
        public float hCost;
        public float fCost { get { return gCost + hCost; } }

        public Node(Vector3Int gridPosition, float gCost, float hCost)
        {
            this.gridPosition = gridPosition;
            this.gCost = gCost;
            this.hCost = hCost;
        }
    }
}
