using JFlex.PacmanWFC.Data;
using UnityEngine;
using UnityEngine.UI;

namespace JFlex.PacmanWFC.View
{
    public class PaletteButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        public Button Button => button;

        [SerializeField]
        private Image buttonImage;

        public void Setup(TILE_PALETTE palette, Sprite sprite)
        {
            gameObject.name = $"{palette}_button";
            buttonImage.sprite = sprite;
        }
    }
}