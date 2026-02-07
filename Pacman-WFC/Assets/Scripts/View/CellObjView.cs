using UnityEngine;

namespace JFlex.PacmanWFC.View
{
    public class CellObjView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer[] spriteRenderers;
        [SerializeField]
        private TMPro.TextMeshPro numberText;

        public void SetSpriteParts(Sprite[] sprites)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sprite = sprites[i];
            }
        }

        public void UpdatePossibilityCount(int count = 0)
        {
            numberText.text = (count == 0) ? "" : count.ToString();
        }

        public void ResetCell(Sprite emptySprite)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sprite = emptySprite;
            }

            numberText.text = "";
        }
    }
}