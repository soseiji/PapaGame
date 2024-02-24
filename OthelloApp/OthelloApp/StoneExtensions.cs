using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloApp
{
    public static class StoneExtensions
    {
        public enum DirectionType
        {
            Top = 0,
            Bottom,
            Left,
            Right,
            LeftTop,
            LeftBottom,
            RightTop,
            RightBottom,
        }

        // ※ ここ大事。座標を取得する。
        public static (int, int) GetPotision(this Stone stone, DirectionType direction, int level)
        {
            var row = stone?.Row ?? 0;
            var column = stone?.Column?? 0;
            return direction switch
            {
                DirectionType.Top => (row - level, column),
                DirectionType.Bottom => (row + level, column),
                DirectionType.Left => (row, column - level),
                DirectionType.Right => (row, column + level),
                DirectionType.LeftTop => (row - level, column - level),
                DirectionType.LeftBottom => (row + level, column - level),
                DirectionType.RightTop => (row - level, column + level),
                DirectionType.RightBottom => (row + level, column + level),
                _ => throw new ArgumentException($"direction:{direction}"),
            };

        }

        public static bool IsEdge(this Stone stone, int boardSize)
        {
            var row = stone?.Row ?? 0;
            var column = stone?.Column ?? 0;
            if (row == 1) return true;
            if (row == boardSize) return true;
            if (column == 1) return true;
            if (column == boardSize) return true;
            return false;
        }

        public static bool IsCorner(this Stone stone, int boardSize) 
        {
            var row = stone?.Row ?? 0;
            var column = stone?.Column ?? 0;
            if (row == 1 && column == 1) return true;
            if (row == boardSize && column == 1) return true;
            if (row == 1 && column == boardSize) return true;
            if (row == boardSize && column == boardSize) return true;
            return false;
        }
    }
}
