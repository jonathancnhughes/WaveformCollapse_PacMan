using JFlex.PacmanWFC.Data;
using System.Collections.Generic;
using UnityEngine;

namespace JFlex.PacmanWFC.View
{
    public class PacManGridView : MonoBehaviour
    {
        [SerializeField]
        private CellObjView cellPrefab;

        private CellObjView[,] cells;

        private const float CELL_SIZE = 0.24f;
        public float Half_Cell_Size = CELL_SIZE / 2f;

        private int height;
        private int width;

        private GameObject gridContainer;

        private SpritePartMapping spritePartMapping;

        private Sprite emptySprite;

        public void CreateCells(int height, int width, SpritePartMapping spritePartMapping)
        {
            DestroyGrid();

            cells = new CellObjView[height, width];
            this.height = height;
            this.width = width;
            this.spritePartMapping = spritePartMapping;

            gridContainer = new GameObject("Grid");
            gridContainer.transform.SetParent(transform);

            emptySprite = spritePartMapping.GetSpriteForPartType(SPRITEPART_TYPE.EMPTY);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cellPosition = new Vector3(CELL_SIZE * x, CELL_SIZE * y, 0);
                    var cellObj = Instantiate(cellPrefab, cellPosition, Quaternion.identity, gridContainer.transform);
                    
                    cellObj.ResetCell(emptySprite);
                    cellObj.gameObject.name = $"Cell ({x},{y})";

                    cells[y, x] = cellObj;
                }
            }
        }

        public CellObjView GetCell(int x, int y)
        {
            return cells[y, x];
        }

        private void DestroyGrid()
        {
            if (gridContainer != null)
            {
                DestroyImmediate(gridContainer);
            }
        }

        public void ResetCells()
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    cells[y, x].ResetCell(emptySprite);
                }
            }
        }

        public void RefreshAll(CellGrid<CellObj> cellGrid)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cellData = cellGrid.GetCell(x, y);

                    cells[y, x].UpdatePossibilityCount(cellData.TileCount);
                    if (cellData.IsCollapsed)
                    {
                        var sprites = spritePartMapping.GetSpritesForTile(cellData.SpriteParts);
                        cells[y, x].SetSpriteParts(sprites);
                    }
                }
            }
        }

        public void RefreshCells(List<CellObj> cellsToUpdate)
        {
            foreach (var cell in cellsToUpdate)
            {
                var x = cell.X;
                var y = cell.Y;

                cells[y, x].UpdatePossibilityCount(cell.TileCount);
                if (cell.IsCollapsed)
                {
                    var sprites = spritePartMapping.GetSpritesForTile(cell.SpriteParts);
                    cells[y, x].SetSpriteParts(sprites);
                }
            }
        }
    }
}