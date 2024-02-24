using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloApp
{
    public class OthelloAiService
    {
        public static ReverseStoneInfo? GetReverseStoneInfo(
            IDictionary<Stone, ReverseStoneInfo> stoneDictionary,
            int boardSize,
            bool isPuttedLeftTop,
            bool isPuttedLeftBottom,
            bool isPuttedRightTop,
            bool isPuttedRightBottom)
        {
            if (stoneDictionary == null) return null;
            if (!stoneDictionary.Any()) return null;

            var stoneInfo = default(ReverseStoneInfo);

            // 四隅の判定
            stoneInfo = stoneDictionary.Values.Where(x => x.IsCorner).FirstOrDefault();
            if (stoneInfo != null) return stoneInfo;

            // 四隅取れてる場合のオススメ
            if (isPuttedLeftTop) 
            {
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Column == 1).OrderBy(x => x.PutStone.Row).FirstOrDefault();
                if (stoneInfo != null) return stoneInfo;
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Row == 1).OrderBy(x => x.PutStone.Column).FirstOrDefault();
                if (stoneInfo != null) return stoneInfo;
            }
            if (isPuttedLeftBottom)
            {
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Column == 1).OrderBy(x => x.PutStone.Row).LastOrDefault();
                if (stoneInfo != null) return stoneInfo;
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Row == boardSize).OrderBy(x => x.PutStone.Column).FirstOrDefault();
                if (stoneInfo != null) return stoneInfo;
            }
            if (isPuttedRightTop)
            {
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Row == 1).OrderBy(x => x.PutStone.Column).LastOrDefault();
                if (stoneInfo != null) return stoneInfo;
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Column == boardSize).OrderBy(x => x.PutStone.Row).FirstOrDefault();
                if (stoneInfo != null) return stoneInfo;
            }
            if (isPuttedRightBottom)
            {
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Row == boardSize).OrderBy(x => x.PutStone.Column).LastOrDefault();
                if (stoneInfo != null) return stoneInfo;
                stoneInfo = stoneDictionary.Values.Where(x => x.PutStone.Column == boardSize).OrderBy(x => x.PutStone.Row).LastOrDefault();
                if (stoneInfo != null) return stoneInfo;
            }

            // 一時クエリ（危険部分をさけた状態）
            var tempQuery = stoneDictionary.Values.Where(x => !x.IsCornerSuperDanger).Where(x => !x.IsCornerDanger);

            // 中央判定（この場合、勝利に影響が小さいのでランダムにする。毎回同じ手になると、つまらないので）
            var stoneInfoCenterList = tempQuery.Where(x => x.IsCenter).OrderByDescending(x => x.ReverseCount).ToList();
            var randomIndex = new Random().Next(minValue: 0, maxValue: stoneInfoCenterList.Count);
            stoneInfo = stoneInfoCenterList.ElementAtOrDefault(randomIndex);
            if (stoneInfo != null)
            {
                Debug.WriteLine("中央");
                return stoneInfo;
            }

            // 四隅付近のオススメ
            stoneInfo = tempQuery.Where(x => x.IsCornerNear).OrderByDescending(x => x.ReverseCount).FirstOrDefault();
            if (stoneInfo != null)
            {
                Debug.WriteLine("四隅おすすめ");
                return stoneInfo;
            }

            // ハシの判定
            stoneInfo = tempQuery.Where(x => x.IsEdge).OrderByDescending(x => x.ReverseCount).FirstOrDefault();
            if (stoneInfo != null)
            {
                Debug.WriteLine("はし");
                return stoneInfo;
            }

            // 危険部分以外で一番数の多い石を取得
            stoneInfo = tempQuery.OrderByDescending(x => x.ReverseCount).FirstOrDefault();
            if (stoneInfo != null)
            {
                Debug.WriteLine("一番おおい");
                return stoneInfo;
            }

            // ------------------------------------------
            // 一時クエリはここまで
            // ------------------------------------------

            // ちょっと危険な部分をせめる
            stoneInfo = stoneDictionary.Values.Where(x => x.IsCornerDanger).OrderByDescending(x => x.ReverseCount).FirstOrDefault();
            if (stoneInfo != null)
            {
                Debug.WriteLine("ちょい危険");
                return stoneInfo;
            }

            // 現時点で一番数の多い石を取得
            stoneInfo = stoneDictionary.Values.OrderByDescending(x => x.ReverseCount).FirstOrDefault();
            if (stoneInfo != null)
            {
                Debug.WriteLine("その他");
                return stoneInfo;
            }

            return null;
        }


        public class ReverseStoneInfo
        {
            public ReverseStoneInfo(Stone putStone, IEnumerable<Stone> reverseStoneList, int boardSize)
            {
                PutStone = putStone;
                ReverseStoneList = reverseStoneList;
                ReverseCount = reverseStoneList?.Count() ?? 0;

                IsEdge = putStone.IsEdge(boardSize);
                IsCorner = putStone.IsCorner(boardSize);

                IsCornerSuperDanger = CheckCornerSuperDanger(putStone.Row, putStone.Column);
                bool CheckCornerSuperDanger(int row, int column)
                {
                    var targetList = new List<(int, int)>()
                    {
                        (2, 2),
                        (boardSize - 1, 2),
                        (2, boardSize -1),
                        (boardSize - 1, boardSize -1),
                    };
                    return targetList.Contains((row, column));
                }

                IsCornerDanger = CheckCornerDanger(putStone.Row, putStone.Column);
                bool CheckCornerDanger(int row, int column)
                {
                    var targetList = new List<(int, int)>()
                    {
                        // 左上
                        (1, 2),
                        (2, 1),
                        // 左下
                        (boardSize - 1, 1),
                        (boardSize, 2),
                        // 右上
                        (1, boardSize -1),
                        (2, boardSize),
                        // 右下
                        (boardSize, boardSize -1),
                        (boardSize - 1, boardSize),
                    };
                    return targetList.Contains((row, column));
                }



                IsCornerNear = CheckCornerNear(putStone.Row, putStone.Column);
                bool CheckCornerNear(int row, int column)
                {
                    var targetList = new List<(int, int)>()
                    {
                        // 左上
                        (3, 1),
                        (3, 2),
                        (3, 3),
                        (2, 3),
                        (1, 3),
                        // 左下
                        (boardSize - 2, 1),
                        (boardSize - 2, 2),
                        (boardSize - 2, 3),
                        (boardSize - 1, 3),
                        (boardSize, 3),
                        // 右上
                        (1, boardSize - 2),
                        (2, boardSize - 2),
                        (3, boardSize - 2),
                        (3, boardSize - 1),
                        (3, boardSize),
                        // 右下
                        (boardSize - 2, boardSize),
                        (boardSize - 2, boardSize - 1),
                        (boardSize - 2, boardSize - 2),
                        (boardSize - 1, boardSize - 2),
                        (boardSize, boardSize - 2),
                    };
                    return targetList.Contains((row, column));
                }

                IsCenter = CheckCenter(putStone.Row, putStone.Column, boardSize);
                bool CheckCenter(int row, int column, int boardSize)
                {
                    var centerStart = (boardSize / 2) - 1;
                    var centerEnd = (boardSize / 2) + 2;
                    var isValidRow = centerStart <= row && row <= centerEnd;
                    var isValidColumn = centerStart <= column && column <= centerEnd;
                    return isValidRow && isValidColumn;
                }

            }

            public bool IsEdge { get; set; } = false;
            public bool IsCorner { get; set; } = false;
            public bool IsCornerEdge { get; set; } = false;     // 四隅のとなり端
            public bool IsCornerNear { get; set; } = false; // 四隅 2マス周辺（四隅をとるため置いた方がよい）
            public bool IsCornerDanger { get; set; } = false;   // 四隅 1マス周辺（危険部分）
            public bool IsCornerSuperDanger { get; set; } = false;   // 四隅 1マス周辺（危険部分）

            public bool IsCenter { get; set; } = false; // 中央付近


            public Stone PutStone { get; set; }
            public IEnumerable<Stone> ReverseStoneList { get; set; }
            public int ReverseCount { get; set; }
        }

    }
}
