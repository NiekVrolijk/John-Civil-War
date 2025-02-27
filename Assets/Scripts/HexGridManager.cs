using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public float hexSize = 1f;  // Adjust based on your Tilemap settings
    public LayerMask unitLayer; // Assign in Unity: Only detect Units
    public int maxMoveRange = 3;

    private UnitScript selectedUnit;
    private HashSet<Vector2Int> occupiedTiles = new(); // Tracks occupied hexes
    private List<Vector2Int> validMoveTiles = new();   // Stores valid move positions

    // Pointy-Top Hex Directions (Axial)
    private readonly Vector2Int[] hexDirections = {
        new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1)
    };

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int hexClicked = WorldToHex(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (selectedUnit == null)
                TrySelectUnit(hexClicked);
            else 
                TryMoveUnit(hexClicked);
        }
    }

    // 📌 Select a unit if one is clicked
    private void TrySelectUnit(Vector2Int hexPosition)
    {
        Collider2D hit = Physics2D.OverlapPoint(HexToWorld(hexPosition), unitLayer);
        if (hit != null)
        {
            selectedUnit = hit.GetComponent<UnitScript>();
            if (selectedUnit != null)
            {
                validMoveTiles = GetValidMoves(selectedUnit.hexPosition, maxMoveRange);
                HighlightTiles(validMoveTiles, true);
            }
        }
    }

    // 📌 Try to move the selected unit
    private void TryMoveUnit(Vector2Int targetHex)
    {
        if (selectedUnit != null && validMoveTiles.Contains(targetHex) && !occupiedTiles.Contains(targetHex))
        {
            occupiedTiles.Remove(selectedUnit.hexPosition); // Free old tile
            occupiedTiles.Add(targetHex); // Mark new tile as occupied

            selectedUnit.hexPosition = targetHex;
            selectedUnit.transform.position = HexToWorld(targetHex);

            HighlightTiles(validMoveTiles, false);
            selectedUnit = null; // Deselect unit after moving
        }
    }

    // 📌 Get valid move positions using BFS (Flood-Fill)
    private List<Vector2Int> GetValidMoves(Vector2Int startHex, int maxMove)
    {
        List<Vector2Int> validMoves = new();
        Queue<(Vector2Int, int)> frontier = new();
        HashSet<Vector2Int> visited = new();

        frontier.Enqueue((startHex, 0));
        visited.Add(startHex);

        while (frontier.Count > 0)
        {
            var (currentHex, distance) = frontier.Dequeue();
            if (distance > 0) validMoves.Add(currentHex);

            if (distance < maxMove)
            {
                foreach (var neighbor in GetNeighbors(currentHex))
                {
                    if (!visited.Contains(neighbor) && !occupiedTiles.Contains(neighbor))
                    {
                        frontier.Enqueue((neighbor, distance + 1));
                        visited.Add(neighbor);
                    }
                }
            }
        }
        return validMoves;
    }

    // 📌 Get Hex Neighbors
    private List<Vector2Int> GetNeighbors(Vector2Int hex)
    {
        List<Vector2Int> neighbors = new();
        foreach (var dir in hexDirections)
        {
            neighbors.Add(hex + dir);
        }
        return neighbors;
    }

    // 📌 Convert Hex to World Position (Pointy-Top)
    private Vector2 HexToWorld(Vector2Int hex)
    {
        float x = hexSize * (Mathf.Sqrt(3) * hex.x + Mathf.Sqrt(3) / 2 * hex.y);
        float y = hexSize * (3.0f / 2.0f * hex.y);
        return new Vector2(x, y);
    }

    // 📌 Convert World Position to Hex Coordinates
    private Vector2Int WorldToHex(Vector2 worldPos)
    {
        float q = (Mathf.Sqrt(3) / 3 * worldPos.x - 1.0f / 3 * worldPos.y) / hexSize;
        float r = (2.0f / 3 * worldPos.y) / hexSize;
        return HexRound(q, r);
    }
    
    // 📌 Round floating hex coords to nearest axial coordinates
    private Vector2Int HexRound(float q, float r)
    {
        float s = -q - r;
        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float qDiff = Mathf.Abs(rq - q);
        float rDiff = Mathf.Abs(rr - r);
        float sDiff = Mathf.Abs(rs - s);

        if (qDiff > rDiff && qDiff > sDiff) rq = -rr - rs;
        else if (rDiff > sDiff) rr = -rq - rs;

        return new Vector2Int(rq, rr);
    }

    // 📌 (Optional) Highlight Movement Range
    private void HighlightTiles(List<Vector2Int> tiles, bool show)
    {
        foreach (var tile in tiles)
        {
            Vector3 pos = HexToWorld(tile);
            Debug.DrawLine(pos, pos + Vector3.up * 0.2f, show ? Color.green : Color.clear, 1f);
        }
    }
}
