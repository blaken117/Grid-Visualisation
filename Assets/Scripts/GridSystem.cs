using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Editor Visuals:")]
    [SerializeField] bool DisplayGridBounds = true;
    [SerializeField] bool DisplayCorners = true;
    [SerializeField] bool DisplayCells = true;
    [SerializeField] bool DisplayCellInfo = true;

    [Header("Properties:")]
    [SerializeField] Vector2 GridSize;
    enum E_GridCellStart { WorldBottomLeft , WorldBottomRight , WorldTopLeft , WorldTopRight }
    [SerializeField] E_GridCellStart GridCellStart;
    [SerializeField] List<FixedCellSizes> FixedCellSizes = new List<FixedCellSizes>();
    [Range(0.0f,0.99f)]
    [SerializeField] float CellSpacing = 0.5f;
    [SerializeField] GameObject CellPrefab;

    // Internal members
    Cell[,] Cells;
    float CellSize = 1;
    int MaxGridSize = 999;
    int CellSizeX, CellSizeZ;
    Vector3 WorldBottomLeft;
    Vector3 WorldBottomRight;
    Vector3 WorldTopLeft;
    Vector3 WorldTopRight;
    Vector2 TempGridSize;

    #region Unity Methods

    private void Start()
    {
        GenerateGridOnPlay();
    }

    void OnValidate()
    {
        GenerateEditorGrid();
    }

    void OnDrawGizmos()
    {
        if (DisplayCorners)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(WorldBottomLeft, 0.5f);
            Gizmos.DrawWireSphere(WorldBottomRight, 0.5f);

            Gizmos.DrawWireSphere(WorldTopLeft, 0.5f);
            Gizmos.DrawWireSphere(WorldTopRight, 0.5f);
        }

        if (DisplayGridBounds)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x, 0, GridSize.y));
        }

        if (DisplayCells)
        {
            Gizmos.color = Color.yellow;

            if (Cells != null)
            {
                foreach (var cell in Cells)
                {
                    Gizmos.DrawWireCube(cell.WorldPosition, new Vector3(CellSize - CellSpacing, 0, CellSize - CellSpacing));
                }
            }
        }

        for (int index = transform.childCount - 1; index >= 0; index--)
        {
            transform.GetChild(index).gameObject.SetActive(DisplayCellInfo);
        }
    }

    #endregion

    #region Custom Methods

    /// <summary>
    /// Used during on validate to create the grid
    /// </summary>
    void GenerateEditorGrid()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying)
                return;

            for (int index = transform.childCount - 1; index >= 0; index--)
            {
                DestroyImmediate(transform.GetChild(index).gameObject);
            }

            GetBestCellSizes();

            SetGridSizeAndBounds();

            Cells = new Cell[CellSizeX, CellSizeZ];

            for (int x = 0; x < CellSizeX; x++)
            {
                for (int z = 0; z < CellSizeZ; z++)
                {
                    Vector3 worldPos = SetStartVector(x, z);

                    var cellObject = Instantiate(CellPrefab, worldPos, Quaternion.identity, transform);

                    var textMesh = cellObject.GetComponentInChildren<TextMesh>();

                    textMesh.text = x.ToString() + ", " + z.ToString() + "\n" + worldPos.ToString();

                    textMesh.characterSize = CellSize;

                    Cells[x, z] = new Cell(worldPos);
                }
            }

            PlayerPrefs.DeleteAll();
        };
    }

    /// <summary>
    /// Used during playmode to setup and store values when entering and exiting playmode
    /// </summary>
    void GenerateGridOnPlay()
    {
        for (int index = transform.childCount - 1; index >= 0; index--)
        {
            Destroy(transform.GetChild(index).gameObject);
        }

        for (int i = 0; i < FixedCellSizes.Count; i++)
        {
            if (FixedCellSizes[i].EnableSize)
            {
                CellSize = FixedCellSizes[i].CellSize;

                PlayerPrefs.SetString(FixedCellSizes[i].SizeName, FixedCellSizes[i].SizeName);
            }
        }

        SetGridSizeAndBounds();

        Cells = new Cell[CellSizeX, CellSizeZ];

        for (int x = 0; x < CellSizeX; x++)
        {
            for (int z = 0; z < CellSizeZ; z++)
            {
                Vector3 worldPos = SetStartVector(x, z);

                var cellObject = Instantiate(CellPrefab, worldPos, Quaternion.identity, transform);

                var textMesh = cellObject.GetComponentInChildren<TextMesh>();

                textMesh.text = x.ToString() + ", " + z.ToString() + "\n" + worldPos.ToString();

                textMesh.characterSize = CellSize;

                Cells[x, z] = new Cell(worldPos);
            }
        }
    }

    /// <summary>
    /// Sets grid cell size and bounds for gizmos to be displayed
    /// </summary>
    void SetGridSizeAndBounds()
    {
        CellSizeX = Mathf.Min(Mathf.RoundToInt(GridSize.x / CellSize), MaxGridSize);
        CellSizeZ = Mathf.Min(Mathf.RoundToInt(GridSize.y / CellSize), MaxGridSize);

        WorldBottomLeft = transform.position - Vector3.right * Mathf.Min(GridSize.x, MaxGridSize) / 2 - Vector3.forward * Mathf.Min(GridSize.y, MaxGridSize) / 2;
        WorldBottomRight = transform.position - Vector3.left * Mathf.Min(GridSize.x, MaxGridSize) / 2 - Vector3.forward * Mathf.Min(GridSize.y, MaxGridSize) / 2;
        WorldTopLeft = transform.position - Vector3.right * Mathf.Min(GridSize.x, MaxGridSize) / 2 + Vector3.forward * Mathf.Min(GridSize.y, MaxGridSize) / 2;
        WorldTopRight = transform.position - Vector3.left * Mathf.Min(GridSize.x, MaxGridSize) / 2 + Vector3.forward * Mathf.Min(GridSize.y, MaxGridSize) / 2;
    }

    /// <summary>
    /// Creates vector to begin world position calculations
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    Vector3 SetStartVector(int x, int z)
    {
        Vector3 worldPos = new Vector3();

        if (GridCellStart == E_GridCellStart.WorldBottomLeft)
        {
            worldPos = WorldBottomLeft + Vector3.right * (x * CellSize + (CellSize / 2)) + Vector3.forward * (z * CellSize + (CellSize / 2));
        }

        if (GridCellStart == E_GridCellStart.WorldBottomRight)
        {
            worldPos = WorldBottomRight - Vector3.right * (x * CellSize + (CellSize / 2)) + Vector3.forward * (z * CellSize + (CellSize / 2));
        }

        if (GridCellStart == E_GridCellStart.WorldTopLeft)
        {
            worldPos = WorldTopLeft + Vector3.right * (x * CellSize + (CellSize / 2)) - Vector3.forward * (z * CellSize + (CellSize / 2));
        }

        if (GridCellStart == E_GridCellStart.WorldTopRight)
        {
            worldPos = WorldTopRight - Vector3.right * (x * CellSize + (CellSize / 2)) - Vector3.forward * (z * CellSize + (CellSize / 2));
        }

        return worldPos;
    }

    /// <summary>
    /// Sets list in inspector to show fixed cell sizes to work within current grid size
    /// </summary>
    void GetBestCellSizes()
    {
        if (GridSize != TempGridSize)
        {
            FixedCellSizes.Clear();

            for (int i = 0; i <= GridSize.x; i++)
            {
                if (GridSize.x % i == 0 && GridSize.y % i == 0)
                {
                    var fixedCellSize = new FixedCellSizes(i.ToString(), i);

                    FixedCellSizes.Add(fixedCellSize);
                }
            }

            TempGridSize = GridSize;
        }

        for (int i = 0; i < FixedCellSizes.Count; i++)
        {
            if (FixedCellSizes[i].SizeName == PlayerPrefs.GetString(FixedCellSizes[i].SizeName))
            {
                FixedCellSizes[i].EnableSize = true;
            }

            if (FixedCellSizes[i].EnableSize)
            {
                CellSize = FixedCellSizes[i].CellSize;
            }
        }
    }

    #endregion
}
