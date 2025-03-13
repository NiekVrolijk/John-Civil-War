using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public float hexSize = 1f;
    public LayerMask unitLayer;
    public int maxMoveRange = 3;

    private UnitScript selectedUnit;
    private HashSet<Vector2Int> occupiedTiles = new();
    private List<Vector2Int> validMoveTiles = new();

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

    private void TrySelectUnit(Vector2Int hexPosition)
    {
        Collider2D hit = Physics2D.OverlapPoint(HexToWorld(hexPosition), unitLayer);
        if (hit != null)
        {
            if (selectedUnit != null)
                selectedUnit.SetSelected(false); // Deselect previous unit

            selectedUnit = hit.GetComponent<UnitScript>();
            if (selectedUnit != null)
            {
                selectedUnit.SetSelected(true);
                validMoveTiles = GetValidMoves(selectedUnit.hexPosition, maxMoveRange);
                HighlightTiles(validMoveTiles, true);
            }
        }
    }

    private void TryMoveUnit(Vector2Int targetHex)
    {
        if (selectedUnit != null && validMoveTiles.Contains(targetHex) && !occupiedTiles.Contains(targetHex))
        {
            occupiedTiles.Remove(selectedUnit.hexPosition);
            occupiedTiles.Add(targetHex);

            selectedUnit.hexPosition = targetHex;
            selectedUnit.transform.position = HexToWorld(targetHex);

            HighlightTiles(validMoveTiles, false);
            selectedUnit.SetSelected(false);
            selectedUnit = null;
        }
    }

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

    private List<Vector2Int> GetNeighbors(Vector2Int hex)
    {
        List<Vector2Int> neighbors = new();
        foreach (var dir in hexDirections)
        {
            neighbors.Add(hex + dir);
        }
        return neighbors;
    }

    private Vector2 HexToWorld(Vector2Int hex)
    {
        float x = hexSize * Mathf.Sqrt(3) * (hex.x + 0.5f * hex.y);
        float y = hexSize * 1.5f * hex.y;
        return new Vector2(x, y);
    }

    private Vector2Int WorldToHex(Vector2 worldPos)
    {
        float q = (Mathf.Sqrt(3) / 3 * worldPos.x - 1.0f / 3 * worldPos.y) / hexSize;
        float r = (2.0f / 3 * worldPos.y) / hexSize;
        return HexRound(q, r);
    }

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

    private void HighlightTiles(List<Vector2Int> tiles, bool show)
    {
        foreach (var tile in tiles)
        {
            Vector3 pos = HexToWorld(tile);
            Debug.DrawLine(pos, pos + Vector3.up * 0.2f, show ? Color.green : Color.clear, 1f);
        }
    }
}
