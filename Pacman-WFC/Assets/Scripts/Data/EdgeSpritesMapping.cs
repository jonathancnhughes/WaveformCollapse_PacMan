using UnityEngine;

namespace JFlex.PacmanWFC.Data
{
    [System.Serializable]
    public struct SpriteDirectionPair
    {
        public Direction Direction;
        public SPRITEPART_TYPE Original;
    }

    [System.Serializable]
    public struct DoubleEdgeUpgradeMapping
    {
        public SpriteDirectionPair SpriteDirectionPair;
        public SPRITEPART_TYPE Upgraded;
    }

    [CreateAssetMenu(fileName = "EdgeSpritesConfig", menuName = "Pacman-WFC/Edge Sprites Config")]
    public class EdgeSpritesMapping : ScriptableObject
    {
        private readonly SPRITEPART_TYPE EmptySprite = SPRITEPART_TYPE.EMPTY;

        [SerializeField]
        private DoubleEdgeUpgradeMapping[] doubleEdgeUpgradeMappings;

        public bool TryGetDoubleEdgeUpdate(SPRITEPART_TYPE spritePart, Direction direction, out SPRITEPART_TYPE upgradedPart)
        {
            // If the intended sprite is empty we know there is no double-edge upgrade so exit early.
            if (spritePart == EmptySprite)
            {
                upgradedPart = spritePart;
                return false;
            }

            foreach (var mapping in doubleEdgeUpgradeMappings)
            {
                var sdp = mapping.SpriteDirectionPair;
                if (sdp.Original == spritePart && (sdp.Direction & direction) != 0)
                {
                    upgradedPart = mapping.Upgraded;
                    return true;
                }
            }

            upgradedPart = spritePart;
            return false;
        }
    }
}