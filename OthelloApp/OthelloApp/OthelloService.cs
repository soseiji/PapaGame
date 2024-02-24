using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OthelloApp.Stone;
using static OthelloApp.StoneExtensions;

namespace OthelloApp
{
    public class OthelloService
    {
        public IList<Stone> StoneList { get; private set; } = new List<Stone>();

        public int BlackCount => StoneList.Count(x => x.Color == ColorType.Black);
        public int WhiteCount => StoneList.Count(x => x.Color == ColorType.White);
        public int NoneCount => StoneList.Count(x => x.Color == ColorType.None);

        public ColorType? Winner { get; private set; } = null;
        public ColorType MyColor { get; private set; } = ColorType.Black;

        public int DefaultBoardSize { get; private set; } = 8;
        public IEnumerable<int> SupportBoardSize { get; private set; } = new List<int>() { 4, 6, 8, 10, 12 };   // 大きすぎると、つまらない
        public IEnumerable<int> BoardSizeRange { get; private set; } = new List<int>();

        public bool IsPuttedSelf(int row, int column) => 
            StoneList.Where(x => x.Row == row).Where(x => x.Column == column).Any(x => x.Color == MyColor);

        public void Start(int boardSize) 
        {
            BoardSizeRange = Enumerable.Range(1, boardSize).ToList();

            // 初期化
            MyColor = ColorType.Black;
            Winner = null;
            StoneList.Clear();

            // 盤づくり
            var centerLeft = boardSize / 2;
            var centerRight = (boardSize / 2) + 1;
            foreach (var row in BoardSizeRange)
            {
                foreach (var column in BoardSizeRange)
                {
                    var color = ColorType.None;
                    if (row == centerLeft && column == centerLeft) color = ColorType.Black;
                    if (row == centerRight && column == centerRight) color = ColorType.Black;
                    if (row == centerLeft && column == centerRight) color = ColorType.White;
                    if (row == centerRight && column == centerLeft) color = ColorType.White;
                    StoneList.Add(new Stone(color, row, column));
                }
            }
        }

        /// <summary>
        /// パスする
        /// </summary>
        public void Pass() => TakeTurn();

        /// <summary>
        /// 石を置いて反転する（終了判定も実施する）
        /// </summary>
        /// <param name="putStone">配置する石</param>
        /// <param name="reverseStoneList">反転する石</param>
        public void Reverse(Stone putStone, IEnumerable<Stone> reverseStoneList) 
        {
            // 配置情報のクリア
            foreach (var stone in StoneList) stone.ClearPutInfo();

            // 反転実施
            SetStone(putStone, MyColor, isPut: true);
            foreach (var reverseStone in reverseStoneList) SetStone(reverseStone, MyColor, isReverse: true);

            // 終了判定
            if (IsFinish()) 
            {
                Winner = BlackCount < WhiteCount ? ColorType.White : ColorType.Black;
            }
            else 
            {
                TakeTurn();
            }


            // 石をセット
            void SetStone(Stone stone, ColorType color, bool isPut = false, bool isReverse = false)
            {
                var targetStone = StoneList
                    .Where(x => x.Row == stone.Row)
                    .Where(x => x.Column == stone.Column)
                    .Single();
                if (isPut) targetStone.Put();
                if (isReverse) targetStone.Reverse();
                targetStone.SetColor(color);
            }

            // 終了判定
            bool IsFinish() 
            {
                if (BlackCount == 0) return true;
                if (WhiteCount == 0) return true;
                if (NoneCount == 0) return true;
                return false;
            }
        }

        private void TakeTurn()
        {
            switch (MyColor)
            {
                case ColorType.Black:
                    MyColor = ColorType.White;
                    break;
                case ColorType.White:
                    MyColor = ColorType.Black;
                    break;
                default:
                    break;
            }
        }

        public IEnumerable<Stone> GetReverseStoneList(Stone putStone) 
        {
            // 空きますチェック
            var isEmptyMass = StoneList
                .Where(x => x.Row == putStone.Row)
                .Where(x => x.Column == putStone.Column)
                .Single()
                .Color == ColorType.None;
            if (!isEmptyMass) return new List<Stone>();

            // 反転できる石を取得
            var reverseStoneList = new List<Stone>();
            foreach (DirectionType direction in Enum.GetValues(typeof(DirectionType)))
            {
                reverseStoneList.AddRange(GetReverseStoneList(putStone, direction));
            }
            return reverseStoneList;
        }

        private IEnumerable<Stone> GetReverseStoneList(Stone stone, DirectionType direction) 
        {
            // 指定方向の石をすべて取得
            // ※※※ ポイント ※※※
            // ・盤サイズ分を全チェックする。
            // ・端のマスの場合は余分なチェック処理が生じるが、抽象化のため全部チェック。
            var directionStoneList = BoardSizeRange
                .Select(x => GetStone(stone, direction, x))
                .ToList()
                ?? new List<Stone>();

            // 空きマスになる部分でカット
            var targetStoneList = new List<Stone>();
            foreach (var directionStone in directionStoneList)
            {
                if (directionStone.Color == ColorType.None) break;
                targetStoneList.Add(directionStone);
            }

            // 基本チェック（要素なし・全部自分・全部相手の場合、反転できない）
            if (targetStoneList.Count == 0) yield break;
            if (targetStoneList.All(x => x.Color == MyColor)) yield break;
            if (targetStoneList.All(x => x.Color != MyColor)) yield break;

            // 次要素チェック（隣が同じ色の場合、反転できない）
            var nextColor = GetStone(stone, direction, level: 1).Color;
            if (nextColor == ColorType.None) yield break;
            if (nextColor == MyColor) yield break;

            // 反転できる要素までチェック
            // 自分の色が見つかるまで要素を反転できる。
            foreach (var level in Enumerable.Range(1, targetStoneList.Count))
            {
                var targetStone = GetStone(stone, direction, level);
                if (targetStone.Color == MyColor) yield break;  // 自分の色になったら、そこで終了。
                yield return targetStone;
            }

            // 石を取得する（level：どれだけの深さを探索するか）
            Stone GetStone(Stone stone, DirectionType direction, int level)
            {
                var (row, column) = stone.GetPotision(direction, level);
                return StoneList
                    .Where(x => x.Row == row)
                    .Where(x => x.Column == column)
                    .SingleOrDefault()
                    ?? new Stone();
            }
        }
    }
}
