using System;

namespace JFlex.PacmanWFC
{
    [Flags]
    public enum Direction
    {
        None = 0,
        Right = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Up = 1 << 3,
    }

    public static class DirectionExtensions
    {
        public static readonly Direction[] AllDirections =
        {
            Direction.Up,
            Direction.Down,
            Direction.Left,
            Direction.Right,
        };

        public static (int dr, int dc) LookInDirection(this Direction direction) => direction switch 
        {
            Direction.Right => (1, 0),
            Direction.Down => (0, -1),
            Direction.Left => (-1, 0),
            Direction.Up => (0, 1),
            _ => throw new ArgumentException()
        };

        public static Direction Opposite(this Direction direction) => direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.None
        };

        public static Direction TurnRight(this Direction direction) => direction switch
        {
            Direction.Right => Direction.Down,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
            Direction.Up => Direction.Right,
            _ => throw new ArgumentException()
        };

        public static Direction TurnLeft(this Direction direction) => direction switch
        {
            Direction.Right => Direction.Up,
            Direction.Up => Direction.Left,
            Direction.Left => Direction.Down,
            Direction.Down => Direction.Right,
            _ => throw new ArgumentException()
        };
    }
}