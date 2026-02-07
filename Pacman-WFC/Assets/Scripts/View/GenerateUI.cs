using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using JFlex.PacmanWFC.Data;

namespace JFlex.PacmanWFC.View
{
    public class GenerateUI : MonoBehaviour
    {
        private int height = 8;
        private int width = 10;

        [Header("Generation Settings")]
        [SerializeField]
        private TextMeshProUGUI heightValue;
        [SerializeField]
        private TextMeshProUGUI widthValue;

        [SerializeField]
        private Vector2 heightRange = new(7, 16);
        [SerializeField]
        private Vector2 widthRange = new(8, 14);

        [SerializeField]
        private TextMeshProUGUI demoValue;

        [SerializeField]
        private List<TextMeshProUGUI> allText;
        [SerializeField]
        private GameObject exitButton;

        [Header("Palette Buttons Setup")]
        [SerializeField]
        private PalettesData palettes;
        private SpritePartMapping spritePartMapping;

        [SerializeField]
        private PaletteButton paletteButtonPrefab;
        [SerializeField]
        private Transform paletteButtonsContainer;


        [Header("Generating Caption Setup")]
        [SerializeField]
        private GameObject inputsContainer;
        [SerializeField]
        private GameObject generatingCaptionContainer;
        [SerializeField]
        private TextMeshProUGUI captionLabel;

        [Header("Grid Builder")]
        [SerializeField]
        private GridBuilder gridBuilder;

        private bool demoMode = true;

        private void Awake()
        {
            UpdateHeightWidthValues();

            CreatePaletteButtons();

            // Set the first palette as the selected palette.
            SetPalette((TILE_PALETTE)0);

            generatingCaptionContainer.SetActive(false);

#if UNITY_WEBGL
            exitButton.SetActive(false);
#endif
        }

        private void CreatePaletteButtons()
        {
            if (palettes == null)
            {
                return;
            }

            foreach (var (key, palette) in palettes.Data)
            {
                if (palette == null)
                {
                    continue;
                }

                var pb = Instantiate(paletteButtonPrefab, paletteButtonsContainer);
                pb.Setup(key, palette.GetSpriteForPartType(SPRITEPART_TYPE.TOP_LEFT_CORNER));

                pb.Button.onClick.AddListener(() => SetPalette(key));
            }
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
            spritePartMapping = palettes.GetSpritePartMapping(palette);

            // update colours!
            foreach (var t in allText)
            {
                t.faceColor = (Color32)spritePartMapping.PrimaryColour;
                t.outlineColor = (Color32)spritePartMapping.SecondaryColour;
                t.ForceMeshUpdate();
            }
        }

        public void ToggleDemo()
        {
            demoMode = !demoMode;
            demoValue.text = demoMode ? "YES" : "NO";
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
            generatingCaptionContainer.SetActive(true);
            inputsContainer.SetActive(false);


            gridBuilder.BuildGrid(height, width, spritePartMapping, OnStatusUpdate);

            StartCoroutine(WaitUntilBuildComplete());
        }

        private void OnStatusUpdate(string status)
        {
            captionLabel.text = status.ToUpper();
        }

        private IEnumerator WaitUntilBuildComplete()
        {
            yield return new WaitUntil(() => !gridBuilder.IsBuilding);

            captionLabel.text = "DONE!";

            yield return new WaitForSeconds(1f);

            generatingCaptionContainer.SetActive(false);
            inputsContainer.SetActive(true);
        }
    }
}