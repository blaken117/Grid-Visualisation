using UnityEngine;

[System.Serializable]
public class FixedCellSizes
{
    public string SizeName;
    [HideInInspector] public float CellSize;
    public bool EnableSize;

    public FixedCellSizes(string _sizeName, float _cellSize)
    {
        SizeName = _sizeName;
        CellSize = _cellSize;
    }
}
