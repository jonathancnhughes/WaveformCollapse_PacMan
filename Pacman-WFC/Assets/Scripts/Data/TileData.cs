using System;
using UnityEngine;

namespace JFlex.PacmanWFC.Data
{
    [Flags]
    public enum SPRITEPART_SECTION
    {
        EMPTY = 0,
        LEFT_SIDE,
        TOP_SIDE,
        RIGHT_SIDE,
        BOTTOM_SIDE,
        BOTTOM_LEFT,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_RIGHT,
        TOP_LEFT_CORNER,
        TOP_RIGHT_CORNER,
        BOTTOM_LEFT_CORNER,
        BOTTOM_RIGHT_CORNER,
        GHOST_LEFT_ENTRANCE,
        GHOST_RIGHT_ENTRANCE,
        GHOST_TOP_LEFT,
        GHOST_TOP_RIGHT,
        GHOST_BOTTOM_LEFT,
        GHOST_BOTTOM_RIGHT,
        ANY_CORNER = TOP_LEFT_CORNER | TOP_RIGHT_CORNER | BOTTOM_LEFT_CORNER | BOTTOM_RIGHT_CORNER
    }


    public enum SPRITEPART_TYPE
    {
        EMPTY,
        LEFT_EDGE,
        RIGHT_EDGE,
        TOP_EDGE,
        BOTTOM_EDGE,
        TOP_LEFT_CORNER,
        TOP_RIGHT_CORNER,
        BOTTOM_LEFT_CORNER,
        BOTTOM_RIGHT_CORNER,
        INNER_BOTTOM_LEFT,
        INNER_BOTTOM_RIGHT,
        INNER_TOP_LEFT,
        INNER_TOP_RIGHT,
        DOUBLE_LEFT_EDGE,
        DOUBLE_RIGHT_EDGE,
        DOUBLE_TOP_EDGE,
        DOUBLE_BOTTOM_EDGE,
        DOUBLE_TOP_LEFT_CORNER,
        DOUBLE_BOTTOM_LEFT_CORNER,
        DOUBLE_TOP_RIGHT_CORNER,
        DOUBLE_BOTTOM_RIGHT_CORNER,
        DOUBLE_BOTTOM_LEFT_LEFT_EDGE,
        DOUBLE_BOTTOM_LEFT_BOTTOM_EDGE,
        DOUBLE_TOP_RIGHT_RIGHT_EDGE,
        DOUBLE_TOP_RIGHT_TOP_EDGE,
        DOUBLE_TOP_LEFT_LEFT_EDGE,
        DOUBLE_TOP_LEFT_TOP_EDGE,
        DOUBLE_BOTTOM_RIGHT_RIGHT_EDGE,
        DOUBLE_BOTTOM_RIGHT_BOTTOM_EDGE,
        GHOST_TOP_LEFT,
        GHOST_TOP_RIGHT,
        GHOST_BOTTOM_LEFT,
        GHOST_BOTTOM_RIGHT,
        GHOST_LEFT_ENTRANCE,
        GHOST_RIGHT_ENTRANCE,
        GHOST_DOOR
    }

    [CreateAssetMenu(fileName = "TileData", menuName = "Pacman-WFC/TileData")]
    public class TileData : ScriptableObject
    {
        [SerializeField]
        private SPRITEPART_TYPE[] spriteParts;
        public SPRITEPART_TYPE[] SpriteParts => spriteParts;

        [SerializeField]
        private Direction connections = Direction.None;
        public Direction Connections => connections;

        // A better test?
        public bool IsEmptyTile => name.ToLower().StartsWith("tile_empty");

        [SerializeField]
        private TileData mirroredTile;
        public TileData MirroredTile => mirroredTile;

        private readonly int[] LEFT_EDGE_SPRITES =
        {
            BOTTOMROW_LEFT,
            MIDDLEROW_LEFT,
            TOPROW_LEFT
        };

        private readonly int[] TOP_EDGE_SPRITES =
        {
            TOPROW_LEFT,
            TOPROW_MIDDLE,
            TOPROW_RIGHT
        };

        private readonly int[] RIGHT_EDGE_SPRITES =
        {
            BOTTOMROW_RIGHT,
            MIDDLEROW_RIGHT,
            TOPROW_RIGHT
        };

        private readonly int[] BOTTOM_EDGE_SPRITES =
        {
            BOTTOMROW_LEFT,
            BOTTOMROW_MIDDLE,
            BOTTOMROW_RIGHT
        };

        private const int BOTTOMROW_LEFT = 0;
        private const int BOTTOMROW_MIDDLE = 1;
        private const int BOTTOMROW_RIGHT = 2;
        private const int MIDDLEROW_LEFT = 3;
        private const int MIDDLEROW_MIDDLE = 4;
        private const int MIDDLEROW_RIGHT = 5;
        private const int TOPROW_LEFT = 6;
        private const int TOPROW_MIDDLE = 7;
        private const int TOPROW_RIGHT = 8;

        public TileData Clone()
        {
            return Instantiate(this);
        }

        public override bool Equals(object other)
        {
            var thisName = name.ToLower().Replace("(clone)", "").Trim();
            var otherName = (other as TileData).name.ToLower().Replace("(clone)", "").Trim();

            return (thisName == otherName);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool HasConnection(Direction direction)
        {
            return (Connections & direction) != 0;
        }

        public bool CanConnectToNeighbour(TileData neighbour, Direction dir)
        {
            return HasConnection(dir.Opposite()) && neighbour.HasConnection(dir) ||
                !HasConnection(dir.Opposite()) && !neighbour.HasConnection(dir);
        }

        public bool SharedEdgeWithNeighbour(TileData neighbour, Direction dir)
        {
            return HasConnection(dir) && neighbour.HasConnection(dir.Opposite());
        }

        private int[] GetSpritesListForDirection(Direction dir) => dir switch
        {
            Direction.Right => RIGHT_EDGE_SPRITES,
            Direction.Down => BOTTOM_EDGE_SPRITES,
            Direction.Left => LEFT_EDGE_SPRITES,
            Direction.Up => TOP_EDGE_SPRITES,
            _ => throw new NotImplementedException(),
        };

        public void AddDoubleEdges(Direction directions, EdgeSpritesMapping spritePartMapping)
        {
            foreach (var dir in DirectionExtensions.AllDirections)
            {
                if ((dir & directions) == 0)
                {
                    continue;
                }

                var spritesList = GetSpritesListForDirection(dir);

                for (int i = 0; i < spritesList.Length; i++)
                {
                    var spritePartIdx = spritesList[i];
                    
                    if (spritePartMapping.TryGetDoubleEdgeUpdate(spriteParts[spritePartIdx], dir, 
                        out var newSpritePart))
                    {
                        spriteParts[spritePartIdx] = newSpritePart;
                    }
                }
            }           
        }

        public void AddDoubleEdge(SPRITEPART_SECTION spriteSection)
        {
            switch (spriteSection)
            {
                case SPRITEPART_SECTION.LEFT_SIDE:

                    spriteParts[BOTTOMROW_LEFT] = SPRITEPART_TYPE.DOUBLE_LEFT_EDGE;
                    spriteParts[MIDDLEROW_LEFT] = SPRITEPART_TYPE.DOUBLE_LEFT_EDGE;
                    spriteParts[TOPROW_LEFT] = SPRITEPART_TYPE.DOUBLE_LEFT_EDGE;

                    break;
                case SPRITEPART_SECTION.TOP_SIDE:

                    spriteParts[TOPROW_LEFT] = SPRITEPART_TYPE.DOUBLE_TOP_EDGE;
                    spriteParts[TOPROW_MIDDLE] = SPRITEPART_TYPE.DOUBLE_TOP_EDGE;
                    spriteParts[TOPROW_RIGHT] = SPRITEPART_TYPE.DOUBLE_TOP_EDGE;

                    break;
                case SPRITEPART_SECTION.RIGHT_SIDE:

                    spriteParts[BOTTOMROW_RIGHT] = SPRITEPART_TYPE.DOUBLE_RIGHT_EDGE;
                    spriteParts[MIDDLEROW_RIGHT] = SPRITEPART_TYPE.DOUBLE_RIGHT_EDGE;
                    spriteParts[TOPROW_RIGHT] = SPRITEPART_TYPE.DOUBLE_RIGHT_EDGE;

                    break;

                case SPRITEPART_SECTION.GHOST_LEFT_ENTRANCE:

                    spriteParts[BOTTOMROW_LEFT] = SPRITEPART_TYPE.DOUBLE_BOTTOM_EDGE;
                    spriteParts[BOTTOMROW_MIDDLE] = SPRITEPART_TYPE.GHOST_LEFT_ENTRANCE;
                    spriteParts[BOTTOMROW_RIGHT] = SPRITEPART_TYPE.GHOST_DOOR;

                    break;

                case SPRITEPART_SECTION.GHOST_RIGHT_ENTRANCE:

                    spriteParts[BOTTOMROW_LEFT] = SPRITEPART_TYPE.GHOST_DOOR;
                    spriteParts[BOTTOMROW_MIDDLE] = SPRITEPART_TYPE.GHOST_RIGHT_ENTRANCE;
                    spriteParts[BOTTOMROW_RIGHT] = SPRITEPART_TYPE.DOUBLE_BOTTOM_EDGE;

                    break;

                case SPRITEPART_SECTION.GHOST_TOP_LEFT:

                    spriteParts[BOTTOMROW_RIGHT] = SPRITEPART_TYPE.GHOST_TOP_LEFT;
                    break;

                case SPRITEPART_SECTION.GHOST_TOP_RIGHT:

                    spriteParts[BOTTOMROW_LEFT] = SPRITEPART_TYPE.GHOST_TOP_RIGHT;
                    break;

                case SPRITEPART_SECTION.GHOST_BOTTOM_LEFT:

                    spriteParts[TOPROW_RIGHT] = SPRITEPART_TYPE.GHOST_BOTTOM_LEFT;
                    break;

                case SPRITEPART_SECTION.GHOST_BOTTOM_RIGHT:

                    spriteParts[TOPROW_LEFT] = SPRITEPART_TYPE.GHOST_BOTTOM_RIGHT;
                    break;

                default:

                    Debug.LogWarning($"An attempt was made to update sprite parts for section {spriteSection}");
                    break;
            }
        }
    }
}