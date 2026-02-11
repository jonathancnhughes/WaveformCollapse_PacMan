using JFlex.PacmanWFC.Data;
using JFlex.PacmanWFC.View;
using System.Collections;
using UnityEngine;

namespace JFlex.PacmanWFC
{
    public struct GenerationConfig
    {
        public int Height;
        public int Width;
        public SpritePartMapping SpritePartMapping;
        public DelayTimings DelayTimings;
    }

     public class GridGenerator : MonoBehaviour
    {
        private enum WAVEFUNCTIONSTATUS
        {
            NOTSTARTED,
            RUNNING,
            FINISHED,
            ERROR
        }

        [Header("UI")]
        [SerializeField]
        private PacmanUI pacmanUI;

        [Header("Prefabs")]
        [SerializeField]
        private PaletteButton paletteButtonPrefab;


        [Header("Grid View")]
        [SerializeField]
        private PacManGridView gridView;

        [Header("Data")]
        [SerializeField]
        private TilesConfig tilesConfig;
        [SerializeField]
        private EdgeSpritesMapping edgeSpritesMapping;
        [SerializeField]
        private PalettesData paletteData;

        [Header("Timings Data")]
        [SerializeField]
        private DelayTimings timings_Normal;
        [SerializeField]
        private DelayTimings timings_Fast;
        [SerializeField]
        private DelayTimings timings_Ultra;
        [SerializeField]
        private DelayTimings timings_NoDelay;

        private SpritePartMapping spritePartMapping;

        private CellGrid<CellObj> cellGrid;

        private UIDelegates.OnStatusUpdateCallback statusUpdateCallback;
        private UIDelegates.OnGenerationFinishedCallback generationFinishedCallback;

        public bool IsBuilding { get; private set; }

        private GenerationConfig genConfig;


        private void Awake()
        {
            pacmanUI.CreatePaletteButtons(paletteData, paletteButtonPrefab);

            pacmanUI.OnStartGenerate += PacmanUI_OnStartGenerate;
        }

        private void OnDestroy()
        {
            pacmanUI.OnStartGenerate -= PacmanUI_OnStartGenerate;
        }

        private void PacmanUI_OnStartGenerate(
            GenerationConfig generationConfig, 
            UIDelegates.OnStatusUpdateCallback onStatusUpdateCallback, 
            UIDelegates.OnGenerationFinishedCallback onGenerationFinishedCallback)
        {
            genConfig = generationConfig;

            // decide what delay timings to use
            var cellSize = generationConfig.Height * (generationConfig.Width / 2);

            genConfig.DelayTimings = cellSize switch
            {
                <= 120 => timings_Normal,
                <= 500 => timings_Fast,
                <= 1000 => timings_Ultra,
                _ => timings_NoDelay
            };
            
            statusUpdateCallback = onStatusUpdateCallback;
            generationFinishedCallback = onGenerationFinishedCallback;

            StartCoroutine(GenerateGridCoroutine(genConfig));
        }

        private IEnumerator GenerateGridCoroutine(GenerationConfig genConfig)
        {
            var height = genConfig.Height;
            var width = genConfig.Width;
            var spritePartMapping = genConfig.SpritePartMapping;

            for (var attempts = 0; attempts < 20; attempts++)
            {
                cellGrid = new CellGrid<CellObj>(height, width, tilesConfig, edgeSpritesMapping, statusUpdateCallback);

                gridView.CreateCells(height, width, spritePartMapping);
                gridView.RefreshAll(cellGrid);

                var generateRoutine = cellGrid.GenerateGrid(genConfig.DelayTimings);

                while (generateRoutine.MoveNext())
                {
                    var update = generateRoutine.Current;

                    if (update.Type == UPDATE_TYPE.Reset)
                    {
                        statusUpdateCallback("Resetting");
                        yield return new WaitForSeconds(update.Delay);

                        break; // exit out of generate grid routine.
                    }
                    else if (update.Type == UPDATE_TYPE.Cell)
                    {
                        gridView.RefreshCell(update.UpdatedCell);
                    }
                    else if (update.Type == UPDATE_TYPE.Cells)
                    {
                        gridView.RefreshCells(update.UpdatedCells);
                    }

                    if (update.Delay > 0)
                    {
                        yield return new WaitForSeconds(update.Delay);
                    }
                    else
                    {
                        yield return null;
                    }
                }

                if (cellGrid.IsComplete)
                {
                    generationFinishedCallback();
                    break; // exit out of attempts loop
                }
            }
        }
    }
}