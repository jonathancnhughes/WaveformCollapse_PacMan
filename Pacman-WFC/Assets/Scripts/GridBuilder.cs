using JFlex.PacmanWFC.Data;
using JFlex.PacmanWFC.View;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JFlex.PacmanWFC
{
     public class GridBuilder : MonoBehaviour
    {
        private enum WAVEFUNCTIONSTATUS
        {
            NOTSTARTED,
            RUNNING,
            FINISHED,
            ERROR
        }

        [Header("Camera")]
        [SerializeField]
        private Camera cam;

        [Header("Grid View")]
        [SerializeField]
        private PacManGridView gridView;

        private int height;
        private int width;

        [Header("Demo Step")]
        [SerializeField]
        private float generationStep = 0.2f;
        [SerializeField]
        private float completeStep = 0.1f;

        [Header("Data")]
        [SerializeField]
        private TilesConfig tilesConfig;
        [SerializeField]
        private EdgeSpritesMapping edgeSpritesMapping;

        private CellGrid<CellObj> cellGrid;

        private Action<string> onStatusUpdate;

        public bool IsBuilding { get; private set; }

        public void BuildGrid(int height, int width, SpritePartMapping spritePartMapping, Action<string> onStatusUpdate)
        {
            IsBuilding = true;

            this.height = height;
            this.width = width;
            this.onStatusUpdate = onStatusUpdate;

            // Width needs to be even so it can be halved to generate the left half of the grid.
            if (this.width % 2 != 0)
            {
                this.width++;
            }

            gridView.CreateCells(height, this.width, spritePartMapping);

            // Adjust Camera so grid fits nicely!
            CenterGridOnCamera();

            StartCoroutine(DoWaveFunctionCollapse());
        }

        private void CenterGridOnCamera()
        {
            // Get one of the leftmost cells, and point the camera at it so
            // the grid nicely positioned with the UI overall
            var targetCell = gridView.GetCell(width - 1, height / 2);

            var cameraZ = cam.transform.position.z;

            cam.transform.position = new Vector3(
                targetCell.transform.position.x - gridView.Half_Cell_Size,
                targetCell.transform.position.y - gridView.Half_Cell_Size,
                cameraZ);

            // update camera size based on size of grid?
        }

        private IEnumerator DoWaveFunctionCollapse()
        {
            for (int genAttempts = 0; genAttempts < 100; genAttempts++)
            {
                cellGrid = new CellGrid<CellObj>(height, width, tilesConfig, edgeSpritesMapping, onStatusUpdate);

                cellGrid.PlaceTunnels();

                gridView.RefreshAll(cellGrid);

                cellGrid.PlaceGhostSpawnCells();

                gridView.RefreshAll(cellGrid);

                WAVEFUNCTIONSTATUS status = WAVEFUNCTIONSTATUS.NOTSTARTED;
                var attempts = 0;

                onStatusUpdate?.Invoke($"RUNNING WAVEFORM COLLAPSE GENERATION\n" +
                    $"Attempt #{(genAttempts + 1)}");

                while (status != WAVEFUNCTIONSTATUS.FINISHED && attempts < 20)
                {
                    status = WaveFunctionCollapseStep();

                    if (status == WAVEFUNCTIONSTATUS.ERROR)
                    {
                        Debug.Log("ERROR - COULD NOT COLLAPSE ALL CELLS!");

                        cellGrid.ResetCells();
                        attempts++;
                    }

                    gridView.RefreshAll(cellGrid);

                    yield return new WaitForSeconds(generationStep);
                }

                onStatusUpdate?.Invoke("checking grid is valid");

                if (cellGrid.IsValidGrid())
                {
                    break;
                }

                onStatusUpdate?.Invoke("restarting");
                yield return new WaitForSeconds(1f);

                gridView.ResetCells();
            }

            onStatusUpdate?.Invoke("Completing Grid");

            // Complete other half of grid!
            StartCoroutine(cellGrid.CompleteGrid(completeStep));

            while (!cellGrid.IsComplete)
            {
                gridView.RefreshAll(cellGrid);
                yield return null;
            }

            gridView.RefreshAll(cellGrid);

            IsBuilding = false;
        }

        // WAVE FUNCTION METHODS
        private WAVEFUNCTIONSTATUS WaveFunctionCollapseStep()
        {
            if (!cellGrid.TryGetCellWithLowestEntropy(out var cellToCollapse))
            {
                Debug.Log("No tiles to collapse!");
                return WAVEFUNCTIONSTATUS.FINISHED;
            }

            if (!cellGrid.TryCollapseCell(cellToCollapse))
            {
                return WAVEFUNCTIONSTATUS.ERROR;
            }

            var cellStack = new Stack<CellObj>();
            cellStack.Push(cellToCollapse);

            while (cellStack.Count > 0)
            {
                var cell = cellStack.Pop();
                foreach (Direction dir in DirectionExtensions.AllDirections)
                {
                    if (cellGrid.TryConstrainNeighbourOfCell(cell, dir, out var neighbour))
                    {
                        cellStack.Push(neighbour);
                    }
                }
            }

            return WAVEFUNCTIONSTATUS.RUNNING;
        }
    }
}