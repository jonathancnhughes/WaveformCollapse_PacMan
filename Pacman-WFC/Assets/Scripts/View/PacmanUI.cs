using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using JFlex.PacmanWFC.Data;

namespace JFlex.PacmanWFC.View
{
    public class UIDelegates
    {
        public delegate void OnStatusUpdateCallback(string status);
        public delegate void OnGenerationFinishedCallback();
    }

    public class PacmanUI : MonoBehaviour
    {
        private int height = 9;
        private int width = 12;

        [Header("Generation Settings")]
        [SerializeField]
        private TextMeshProUGUI heightValue;
        [SerializeField]
        private TextMeshProUGUI widthValue;

        [SerializeField]
        private Vector2 heightRange = new(7, 50);
        [SerializeField]
        private Vector2 widthRange = new(8, 50);

        [SerializeField]
        private List<TextMeshProUGUI> allText;
        [SerializeField]
        private GameObject exitButton;

        [Header("Palette Buttons Setup")]
        [SerializeField]
        private Transform paletteButtonsContainer;
        private PalettesData palettesData;

        private SpritePartMapping spritePartMapping;

        [Header("Generating Caption Setup")]
        [SerializeField]
        private GameObject[] inputContainers;
        [SerializeField]
        private GameObject generatingCaptionContainer;
        [SerializeField]
        private TextMeshProUGUI captionLabel;

        [Header("Grid Builder")]
        [SerializeField]
        private GridGenerator generator;

        public delegate void OnStartGenerateHandler(
            GenerationConfig generationConfig,
            UIDelegates.OnStatusUpdateCallback onStatusUpdateCallback, 
            UIDelegates.OnGenerationFinishedCallback onGenerationFinishedCallback);
       
        public event OnStartGenerateHandler OnStartGenerate = delegate { };

        private void Awake()
        {
            UpdateHeightWidthValues();

            generatingCaptionContainer.SetActive(false);

#if UNITY_WEBGL
            exitButton.SetActive(false);
#endif
        }

        public void CreatePaletteButtons(PalettesData palettes, PaletteButton buttonPrefab)
        {
            if (palettes == null)
            {
                return;
            }

            palettesData = palettes;

            foreach (var (key, palette) in palettes.Data)
            {
                if (palette == null)
                {
                    continue;
                }

                var pb = Instantiate(buttonPrefab, paletteButtonsContainer);
                pb.Setup(key, palette.GetSpriteForPartType(SPRITEPART_TYPE.TOP_LEFT_CORNER));

                pb.Button.onClick.AddListener(() => SetPalette(key));
            }

            // Set the first palette as the selected palette.
            SetPalette((TILE_PALETTE)0);
        }

        private void UpdateHeightWidthValues()
        {
            heightValue.text = height.ToString();
            widthValue.text = width.ToString();
        }

        public void IncreaseHeight()
        {
            height = (int)Mathf.Min(height + 1, heightRange.y);
            UpdateHeightWidthValues();
        }

        public void DecreaseHeight()
        {
            height = (int)Mathf.Max(height - 1, heightRange.x);
            UpdateHeightWidthValues();
        }

        public void IncreaseWidth()
        {
            width = (int)Mathf.Min(width + 2, widthRange.y);
            UpdateHeightWidthValues();
        }

        public void DecreaseWidth()
        {
            width = (int)Mathf.Max(width - 2, widthRange.x);
            UpdateHeightWidthValues();
        }

        public void SetPalette(TILE_PALETTE palette)
        {
            spritePartMapping = palettesData.GetSpritePartMapping(palette);

            // update colours!
            foreach (var t in allText)
            {
                t.faceColor = (Color32)spritePartMapping.PrimaryColour;
                t.outlineColor = (Color32)spritePartMapping.SecondaryColour;
                t.ForceMeshUpdate();
            }
        }

        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
        }

        public void GenerateGrid()
        {
            captionLabel.text = "GENERATING";

            ToggleInputs(false);

            var genConfig = new GenerationConfig
            {
                Height = height,
                Width = width,
                SpritePartMapping = spritePartMapping
            };           

            OnStartGenerate(genConfig, OnStatusUpdate, OnGenerationFinished);
        }

        private void OnStatusUpdate(string status)
        {
            captionLabel.text = status.ToUpper();
        }

        public void OnGenerationFinished()
        {
            StartCoroutine(PauseBeforeEnablingInputs());
        }

        private IEnumerator PauseBeforeEnablingInputs()
        {
            captionLabel.text = "DONE!";

            yield return new WaitForSeconds(1f);

            ToggleInputs(true);
        }

        private void ToggleInputs(bool toggle)
        {
            generatingCaptionContainer.SetActive(!toggle);
            foreach (var go in inputContainers)
            {
                go.SetActive(toggle);
            }
        }
    }
}