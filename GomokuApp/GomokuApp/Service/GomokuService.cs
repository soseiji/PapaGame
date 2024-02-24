using GomokuApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GomokuApp.Dto.Stone;
using static GomokuApp.Extension.StoneExtensions;

namespace GomokuApp.Service
{
    public class GomokuService
    {
        public IList<Stone> StoneList { get; private set; } = new List<Stone>();

        public ColorType? Winner { get; private set; } = null;
        public ColorType MyColor { get; private set; } = ColorType.Black;

        public int Threshold { get; private set; } = 0;
        public int DefaultThreshold { get; private set; } = 4;
        public IEnumerable<int> SupportThreshold { get; private set; } = new List<int>() { 3, 4, 5 };

        public int DefaultBoardSize { get; private set; } = 10;
        public IEnumerable<int> SupportBoardSize { get; private set; } = new List<int>() { 8, 10, 12, 14 };
        public IEnumerable<int> BoardSizeRange { get; private set; } = new List<int>();

        /// <summary>
        /// ゲームを始める
        /// </summary>
        /// <param name="threshold">ｎ目ならべ</param>
        /// <param name="boardSize">ボードサイズ</param>
        public void Start(int threshold, int boardSize)
        {
            Threshold = threshold;
            BoardSizeRange = Enumerable.Range(1, boardSize).ToList();

            // 初期化
            StoneList.Clear();
            Winner = null;
            MyColor = ColorType.Black;

            // 盤づくり
            foreach (var row in BoardSizeRange)
            {
                foreach (var column in BoardSizeRange)
                {
                    StoneList.Add(new Stone(ColorType.None, row, column));
                }
            }
        }

        /// <summary>
        /// 選択した列に石をセットできるか否か
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool CanSet(int column) 
        {
            return StoneList
                .Where(x => x.Column == column)
                .Where(x => x.Row == 1)
                .Where(x => x.Color == ColorType.None)
                .Any();
        }

        /// <summary>
        /// 選択した列に石をセットする。（石を落とす仕様）
        /// </summary>
        /// <param name="column">列番号（左から１）</param>
        public void Set(int column) 
        {
            // 配置情報のクリア
            foreach (var stone in StoneList) stone.ClearPutInfo();

            // おとす（指定カラムの一番底のマスに石をおく）
            var putStone =  StoneList
                .Where(x => x.Column == column)
                .Where(x => x.Color == ColorType.None)
                .OrderByDescending(x => x.Row)
                .FirstOrDefault();
            if (putStone == null) return;
            putStone.SetStone(StoneList, MyColor, isPut: true);
            if (MyColor == ColorType.Black) MainWindow.DebugLog("[黒] 行列:(" + putStone.Row.ToString("D2") + "," + putStone.Column.ToString("D2") + ")");

            // 終了判定
            if (IsFinish(putStone, out var finishStoneList))
            {
                Winner = MyColor;
                foreach (var finishStone in finishStoneList) finishStone.SetStone(StoneList, MyColor, isFinish: true);    // ｎ目の石に★マークつけとく
            }
            else
            {
                TakeTurn();
            }
        }

        /// <summary>
        /// 指定カラムに色をつける
        /// </summary>
        /// <param name="column">カラム番号</param>
        public void SelectColumn(int column) 
        {
            var columnStoneList = StoneList.Where(x => x.Column == column).Where(x => x.Color == ColorType.None).ToList();
            foreach (var stone in StoneList) stone.ClearPutInfo();
            foreach (var columnStone in columnStoneList) columnStone.Put();
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

        private bool IsFinish(Stone putStone, out IEnumerable<Stone> finisStoneList)
        {
            finisStoneList = new List<Stone>();
            List<IEnumerable<DirectionType>> checkDirectionInfo = new()
            {
                new List<DirectionType>() { DirectionType.Top, DirectionType.Bottom },
                new List<DirectionType>() { DirectionType.Left, DirectionType.Right },
                new List<DirectionType>() { DirectionType.LeftTop, DirectionType.RightBottom },
                new List<DirectionType>() { DirectionType.LeftBottom, DirectionType.RightTop },
            };
            foreach (var checkDirectionList in checkDirectionInfo)
            {
                var myStoneList = new List<Stone>
                {
                    putStone   // このターンで置く石を追加する
                };
                foreach (var checkDirection in checkDirectionList)
                {
                    myStoneList.AddRange(BoardSizeRange
                        .Select(x => putStone.GetStone(this.StoneList, checkDirection, x))
                        .TakeWhile(x => x.Color == MyColor)
                        .ToList());
                }
                if (Threshold <= myStoneList.Count) 
                {
                    // 該当プレイヤーの石の数がｎ目以上なので勝ち
                    finisStoneList = myStoneList;
                    return true;
                }
            }
            return false;
        }
    }
}
