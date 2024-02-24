using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiOthelloApp.Stone;
using static MultiOthelloApp.StoneExtensions;

namespace MultiOthelloApp
{
    public class OthelloService
    {
        public IList<Stone> StoneList { get; private set; } = new List<Stone>();

        public int NoneCount => StoneList.Count(x => x.Color == ColorType.None);
        public int BlackCount => StoneList.Count(x => x.Color == ColorType.Black);
        public int WhiteCount => StoneList.Count(x => x.Color == ColorType.White);
        public int RedCount => StoneList.Count(x => x.Color == ColorType.Red);
        public int BlueCount => StoneList.Count(x => x.Color == ColorType.Blue);

        public List<ColorType> Winners { get; private set; } = new List<ColorType>();
        public ColorType MyColor { get; private set; } = ColorType.None;

        public int DefaultPlayerCount { get; private set; } = 4;
        public IEnumerable<int> SupportPlayerCount { get; private set; } = new List<int>() { 2, 3, 4 };
        public int PlayerCount { get; private set; }


        public int DefaultBoardSize { get; private set; } = 8;
        public IEnumerable<int> SupportBoardSize { get; private set; } = new List<int>() { 4, 6, 8, 10, 12 };   // 大きすぎると、つまらない
        public IEnumerable<int> BoardSizeRange { get; private set; } = new List<int>();

        public bool IsPuttedSelf(int row, int column) => StoneList
            .Where(x => x.Row == row)
            .Where(x => x.Column == column)
            .Any(x => x.Color == MyColor);

        public void Start(int boardSize, int playerCount)
        {
            BoardSizeRange = Enumerable.Range(1, boardSize).ToList();
            PlayerCount = playerCount;

            // 初期化
            MyColor = ColorType.Black;
            Winners.Clear();
            StoneList.Clear();

            // 盤づくり
            var centerA = boardSize / 2;
            var centerB = (boardSize / 2) + 1;
            foreach (var row in BoardSizeRange)
            {
                foreach (var column in BoardSizeRange)
                {
                    var color = ColorType.None;

                    // ※ 販売されてるゲームの初期配色 （これでは、狙われたら終了）
                    //if (row == centerA && column == centerA) color = ColorType.Red;
                    //if (row == centerB && column == (centerA - 1)) color = ColorType.Red;

                    //if (row == centerB && column == centerB) color = ColorType.Blue;
                    //if (row == centerA && column == (centerB + 1)) color = ColorType.Blue;

                    //if (row == centerA && column == centerB) color = ColorType.White;
                    //if (row == (centerA - 1) && column == centerA) color = ColorType.White;

                    //if (row == centerB && column == centerA) color = ColorType.Black;
                    //if (row == (centerB + 1) && column == centerB) color = ColorType.Black;


                    if (row == centerA && column == centerA) color = ColorType.Red;
                    if (row == centerB && column == (centerA - 1)) color = ColorType.Red;
                    if (row == (centerA - 2) && column == (centerB + 1)) color = ColorType.Red;

                    if (row == centerA && column == centerB) color = ColorType.White;
                    if (row == (centerA - 1) && column == centerA) color = ColorType.White;
                    if (row == (centerB + 1) && column == (centerB + 2)) color = ColorType.White;

                    if (row == centerB && column == centerB) color = ColorType.Blue;
                    if (row == centerA && column == (centerB + 1)) color = ColorType.Blue;
                    if (row == (centerB + 2) && column == (centerA - 1)) color = ColorType.Blue;

                    if (row == centerB && column == centerA) color = ColorType.Black;
                    if (row == (centerB + 1) && column == centerB) color = ColorType.Black;
                    if (row == (centerA - 1) && column == (centerA - 2)) color = ColorType.Black;

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
                var counts = new List<Tuple<ColorType, int>>
                {
                    new Tuple<ColorType, int>(ColorType.Black, BlackCount),
                    new Tuple<ColorType, int>(ColorType.White, WhiteCount),
                    new Tuple<ColorType, int>(ColorType.Red, RedCount),
                    new Tuple<ColorType, int>(ColorType.Blue, BlueCount)
                };
                // 石の数が多いプレイヤーを検索する。勝者として表示させる
                Winners.AddRange(counts.Where(x => x.Item2 == counts.Max(x => x.Item2)).Select(x => x.Item1));
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
                if (NoneCount == 0) return true;    // 打つところがない状態

                var playerCountList = new List<int> { BlackCount, WhiteCount };
                if (PlayerCount == 3) playerCountList.Add(RedCount);
                if (PlayerCount == 4) playerCountList.Add(BlueCount);

                // 残りのプレイヤーが0件の場合、終了。
                var zeroPlayer = playerCountList.Count(x => x == 0);
                return zeroPlayer == (PlayerCount - 1);
            }
        }

        /// <summary>
        /// 該当ターンの人が置ける石の場所を判定する
        /// </summary>
        public void SetCanReverse() 
        { 
            var noneStoneList = StoneList.Where(x => x.Color == ColorType.None).ToList();
            foreach (var noneStone in noneStoneList)
            {
                // noneStoneList の各要素で、GetReverseStoneList を実行。1以上なら置けると判定。
                if (!GetReverseStoneList(noneStone).Any()) continue;
                noneStone.SetCanReverse();
            }
        }

        private void TakeTurn()
        {
            // プレイヤー数
            // ２人：黒 → 白 → ...　
            // ３人：黒 → 白 → 赤 → ...
            // ４人：黒 → 白 → 赤 → 青 → ...
            switch (MyColor)
            {
                case ColorType.Black:
                    MyColor = ColorType.White;
                    break;
                case ColorType.White:
                    MyColor = (PlayerCount == 2) ? ColorType.Black : ColorType.Red;
                    break;
                case ColorType.Red:
                    MyColor = (PlayerCount == 3) ? ColorType.Black : ColorType.Blue;
                    break;
                case ColorType.Blue:
                    MyColor = ColorType.Black;
                    break;
                default:
                    break;
            }

            // ※ 石の数が０の場合、次のプレイヤーに進む
            // ※ 無限ループを防ぐため、すべての石が０の場合は終了させる
            if (!StoneList.Any()) {
                return;
            }
            switch (MyColor)
            {
                case ColorType.Black:
                    if (BlackCount == 0) TakeTurn();
                    break;
                case ColorType.White:
                    if (WhiteCount == 0) TakeTurn();
                    break;
                case ColorType.Red:
                    if (RedCount == 0) TakeTurn();
                    break;
                case ColorType.Blue:
                    if (BlueCount == 0) TakeTurn();
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
