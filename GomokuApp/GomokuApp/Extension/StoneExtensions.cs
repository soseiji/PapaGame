using GomokuApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GomokuApp.Dto.Stone;

namespace GomokuApp.Extension
{
    public static class StoneExtensions
    {
        // "列挙値=優先度" にする。優先して、斜めのマスに配置させる。
        public enum DirectionType
        {
            LeftTop = 0,
            LeftBottom,
            RightTop,
            RightBottom,
            Top,
            Bottom,
            Left,
            Right,
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

        
        public static Stone GetStone(this Stone stone, IEnumerable<Stone> stoneList, DirectionType direction, int level)
        {
            if (!stone.IsValid()) return new Stone();
            var (row, column) = stone.GetPotision(direction, level);
            return stoneList
                .Where(x => x.Row == row)
                .Where(x => x.Column == column)
                .SingleOrDefault()
                ?? new Stone();
        }

        public static Stone GetNextStone(this Stone stone, IEnumerable<Stone> stoneList, DirectionType direction)
        {
            return stone.GetStone(stoneList, direction, level: 1);
        }

        public static void SetStone(this Stone stone, IEnumerable<Stone> stoneList, ColorType color, bool isPut = false, bool isFinish = false)
        {
            var targetStone = stoneList
                .Where(x => x.Row == stone.Row)
                .Where(x => x.Column == stone.Column)
                .Single();
            targetStone.SetColor(color);
            if (isPut) targetStone.Put();
            if (isFinish) targetStone.Finish();
        }

        public static IEnumerable<Stone> CloneStoneList(this IEnumerable<Stone> stoneList)
        {
            var cloneStoneList = new List<Stone>();
            foreach (var stone in stoneList)
            {
                cloneStoneList.Add(new Stone(stone));
            }
            return cloneStoneList;
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

        public static bool IsValid(this Stone stone)
        {
            if (stone == null) return false;
            if (stone.Row <= 0) return false;
            if (stone.Column <= 0) return false;
            return true;
        }

        public static bool CanSetStone(this Stone stone)
        {
            if (!stone.IsValid()) return false;
            if (stone.Color != ColorType.None) return false;
            return true;
        }

    }
}
