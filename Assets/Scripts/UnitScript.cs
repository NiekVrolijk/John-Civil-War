using UnityEngine;

public class UnitScript : MonoBehaviour
{
    public Vector2Int hexPosition;

    public void SetSelected(bool isSelected)
    {
        GetComponent<SpriteRenderer>().color = isSelected ? Color.yellow : Color.white;
    }
}
