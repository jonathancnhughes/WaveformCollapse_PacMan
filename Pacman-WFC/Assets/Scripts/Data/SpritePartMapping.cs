using UnityEngine;

namespace JFlex.PacmanWFC.Data
{
    [CreateAssetMenu(fileName = "SpritePartMapping", menuName = "Pacman-WFC/SpritePartMapping")]
    public class SpritePartMapping : ScriptableObject
    {
        [SerializeField]
        private Color primaryColour;
        public Color PrimaryColour => primaryColour;

        [SerializeField]
        private Color secondaryColour;
        public Color SecondaryColour => secondaryColour;

        [SerializeField]
        private SpritePartTypeDictionary mappings = new();

        public Sprite GetSpriteForPartType(SPRITEPART_TYPE partType)
        {
            if (mappings.TryGetValue(partType, out var sprite))
            {
                return sprite;
            }

            Debug.LogWarning($"SPRITEPART_TYPE {partType} not found!");

            return null;
        }

        public Sprite[] GetSpritesForTile(SPRITEPART_TYPE[] partTypes)
        {
            Sprite[] sprites = new Sprite[partTypes.Length];

            for (int i = 0; i < partTypes.Length; i++)
            {
                if (mappings.TryGetValue(partTypes[i], out var sprite))
                {
                    sprites[i] = sprite;
                }
            }

            return sprites;
        }
    }
}