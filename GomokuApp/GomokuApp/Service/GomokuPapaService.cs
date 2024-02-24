using GomokuApp.Dto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GomokuApp.Dto.Stone;
using static GomokuApp.Extension.StoneExtensions;

namespace GomokuApp.Service
{
    public class GomokuPapaService
    {
        public int GetGoodColumnNum(IEnumerable<Stone> stoneList, int threshold, int boardSize) 
        {
            var baseStoneList = stoneList.CloneStoneList();

            var firstColumn = GetInitialColumn(baseStoneList);
            if (firstColumn.HasValue) return firstColumn.Value;

            // 解析結果をオススメ順にソート（先頭に勝ちやすい石情報がある）
            var whiteGoodStoneList = GetGoodStoneList(baseStoneList, boardSize, ColorType.White);
            var blackGoodStoneList = GetGoodStoneList(baseStoneList, boardSize, ColorType.Black);

            // 白視点：勝利判定
            var whiteWinStone = GetWinStone(whiteGoodStoneList, threshold);
            if (whiteWinStone != null)
            {
                whiteWinStone.DebugLog("白の勝ち！！！");
                return whiteWinStone.Column;
            }

            // 黒視点：黒の勝利を防ぐ（※ 閾値-1 の状態で対処しても遅い。-2 で危機に対処しておく）
            // ※ただし、そこに置くことで、黒が勝利できる状況になるパターンもある。注意。
            var blackMaybeWinStoneList = blackGoodStoneList.Where(x => x.DirectionMyStoneCount >= threshold - 2).ToList();
            foreach (var blackMaybeWinStone in blackMaybeWinStoneList)
            {
                if (IsWinBlack(blackMaybeWinStone, baseStoneList, boardSize, threshold))
                {
                    blackMaybeWinStone.DebugLog("防御モード: リトライ ここを防ぐと黒の勝利になる");
                    continue;
                }
                blackMaybeWinStone.DebugLog("防御モード: 黒の勝利を防ぐマス");
                return blackMaybeWinStone.Column;
            }

            // 白視点：オススメ（白視点のオススメ）
            foreach (var whiteGoodStone in whiteGoodStoneList)
            {
                if (IsWinBlack(whiteGoodStone, baseStoneList, boardSize, threshold))
                {
                    whiteGoodStone.DebugLog("攻撃モード: リトライ ここを攻めると黒の勝利になる");
                    continue;
                }
                whiteGoodStone.DebugLog("攻撃モード: おすすめマス");
                return whiteGoodStone.Column;
            }

            // 白視点：まけ
            whiteGoodStoneList.First().DebugLog("白の負け...");
            return whiteGoodStoneList.First().Column;
        }

        /// <summary>
        /// 白を打つことで、それが黒の勝利に繋がるのか確認する
        /// </summary>
        /// <param name="nextPutStone"></param>
        /// <param name="stoneList"></param>
        /// <param name="boardSize"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private bool IsWinBlack(StoneAnalysis nextPutStone, IEnumerable<Stone> stoneList, int boardSize, int threshold) 
        {
            var baseStoneList = stoneList.CloneStoneList();
            var targetStone = baseStoneList
                  .Where(x => x.Row == nextPutStone.Row)
                  .Where(x => x.Column == nextPutStone.Column)
                  .Single();
            targetStone.SetStone(baseStoneList, ColorType.White, isPut: true);

            var blackGoodStoneList = GetGoodStoneList(baseStoneList, boardSize, ColorType.Black);
            var blackWinStone = GetWinStone(blackGoodStoneList, threshold);
            return blackWinStone != null;
        }

        /// <summary>
        /// 初回用のカラム番号を取得する（パパモードが毎回同じマスに打つと同じ流れになる。つまらないので）
        /// </summary>
        /// <param name="stoneList"></param>
        /// <returns></returns>
        private int? GetInitialColumn(IEnumerable<Stone> stoneList)
        {
            if (stoneList.Count(x => x.Color == ColorType.Black) != 1) return null;

            var firstStone = stoneList.Single(x => x.Color == ColorType.Black);
            foreach (var retry in Enumerable.Range(0, 10))
            {
                // 以下、ミス。
                // 初回はどこに打っても特に影響ないので、初回だけランダムに実行して流れを変える。
                // 相手の位置による、1左、2上、3右 のどこかにセットする

                // 初回、上に打つと、下側取られて負けるので、左 or 右 のどちらかにセットする
                var directionDic = new Dictionary<int, DirectionType>()
                {
                    [1] = DirectionType.Left,
                    //[2] = DirectionType.Top,
                    [2] = DirectionType.Right,
                };
                var randomDirection = directionDic[new Random().Next(minValue: 1, maxValue: 3)];    // 1～2の乱数
                var randomStone = firstStone.GetNextStone(stoneList, randomDirection);
                if (randomStone.IsValid()) 
                {
                    MainWindow.DebugLog("[白] 初手, random:" + randomDirection.ToString());
                    return randomStone.Column;
                }
                Debug.WriteLine("リトライ");
            }
            return null;
        }

        /// <summary>
        /// オススメ順の石リストを取得する（先頭ほどオススメ石あり）
        /// </summary>
        /// <param name="baseStoneList"></param>
        /// <param name="boardSize"></param>
        /// <param name="colorType"></param>
        /// <returns></returns>
        private IEnumerable<StoneAnalysis> GetGoodStoneList(IEnumerable<Stone> baseStoneList,int boardSize,ColorType colorType) 
        {
            // 配置可能なマス情報を解析
            var placeableStoneList = new List<Stone>();
            foreach (var noneStone in baseStoneList.Where(x => x.Color == ColorType.None))
            {
                // 一番底の場合、配置可能
                if (noneStone.Row == boardSize) 
                {
                    placeableStoneList.Add(noneStone);
                }
                // 一つ下のマスが配置済みの場合、該当マスに配置可能
                if (noneStone.GetNextStone(baseStoneList, DirectionType.Bottom).Color != ColorType.None) 
                {
                    placeableStoneList.Add(noneStone);
                }
            }

            // StoneAnalysis リストを生成（解析用の石情報）
            var checkDirectionList = new List<Tuple<DirectionType, DirectionType>>()
            {
                Tuple.Create(DirectionType.Top, DirectionType.Bottom),
                Tuple.Create(DirectionType.Left, DirectionType.Right),
                Tuple.Create(DirectionType.LeftTop, DirectionType.RightBottom),
                Tuple.Create(DirectionType.LeftBottom, DirectionType.RightTop),
            };
            var stoneAnalysisList = new List<StoneAnalysis>();
            foreach (var placeableStone in placeableStoneList)
            {
                foreach (var checkDirection in checkDirectionList)
                {
                    stoneAnalysisList.Add(GetStoneAnalysis(
                        baseStoneList,
                        placeableStone,
                        boardSize,
                        colorType,
                        checkDirection));
                }
            }
            FinalizeStoneAnalysis(stoneAnalysisList);

            // ※優先度の高い順にソート条件を指定する。
            var sortedStoneAnalysisList = stoneAnalysisList
                .OrderByDescending(x => x.DirectionMyStoneCount)                     // 自身のマス数が多いほどGood！
                .ThenByDescending(x => x.TotalDirectionMyStoneCount)               // 合計値が高いほど次の攻撃に繋がるのでGood
                .ThenByDescending(x => x.TotalSettableStoneCount)                    // 連続マスの先に配置可能マスが多いほど、より有利
                .ThenBy(x => x.Direction1)                                                         // 斜め方向を狙う。その方が相手が混乱しがち。
                .ToList();

            // 配置可能な石が０件の場合、優先度を一番下に調整（リストの一番下に追加）
            var notSettableStoneList = sortedStoneAnalysisList.Where(x => x.SettableStoneCount == 0).ToList();
            foreach (var notSettableStone in notSettableStoneList) sortedStoneAnalysisList.Remove(notSettableStone);
            sortedStoneAnalysisList.AddRange(notSettableStoneList);

            return sortedStoneAnalysisList;
        }

        /// <summary>
        /// 打てば勝てるマスを取得する（閾値-1 の状態なら、あとは打って勝つのみ！）
        /// </summary>
        /// <param name="stoneAnalyseList"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private StoneAnalysis GetWinStone(IEnumerable<StoneAnalysis> stoneAnalyseList, int threshold) => 
            stoneAnalyseList.FirstOrDefault(x => x.RepeatStoneCount >= threshold - 1);      // RepeatStoneCount で -1 が純粋に連続マスの情報になる

        /// <summary>
        /// マス解析情報を取得する
        /// </summary>
        /// <param name="stoneList"></param>
        /// <param name="stone"></param>
        /// <param name="boardSize"></param>
        /// <param name="colorType"></param>
        /// <param name="checkDirection">2方向分のタプル情報</param>
        /// <returns></returns>
        private StoneAnalysis GetStoneAnalysis(
            IEnumerable<Stone> stoneList,
            Stone stone,
            int boardSize, 
            ColorType colorType,
            Tuple<DirectionType, DirectionType> checkDirection)
        {
            var directionList = new List<DirectionType>
            {
                checkDirection.Item1,
                checkDirection.Item2
            };

            var repeatStoneCount = 0;
            var repeatStoneCount_WithOneSpace = 0;
            var settableStoneCount = 0;
            foreach (var direction in directionList)
            {
                // 指定方向に対して連続する指定タイプの石の数を取得
                //   ※ 落とす形式のゲームなので、底 or 一つ下にマスがないと置けない。
                var repeatStoneList = Enumerable.Range(1, boardSize)
                    .Select(x => stone.GetStone(stoneList, direction, x))
                    .TakeWhile(x => x.Color == colorType)
                    .ToList();
                repeatStoneCount += repeatStoneList.Count;

                // 空きマスの先に自分のマスがあり、空きを埋めることで並ぶケースもある！！！
                var directionStoneList = Enumerable.Range(1, boardSize)
                    .Select(x => stone.GetStone(stoneList, direction, x))
                    .ToList();
                var repeatStoneList_WithOneSpace = new List<Stone>();
                var oneChanceFlag = false;
                foreach (var directionStone in directionStoneList)
                {
                    if (directionStone.Color == colorType) 
                    {
                        // 指定方向に対して連続する指定タイプの石の数を取得
                        repeatStoneList_WithOneSpace.Add(directionStone);
                        oneChanceFlag = false;
                    }
                    else if (directionStone.Color == ColorType.None)
                    {
                        // 空きマスの先に自分のマスがあり、空きを埋めることで並ぶケースもある！！！
                        if (oneChanceFlag) break;
                        oneChanceFlag = true;  // ワンチャンス。空きマス検知の場合も、再チェック実施
                    }
                    else 
                    {
                        // 敵の石の場合、そこで探索終了
                        break;
                    }
                }
                repeatStoneCount_WithOneSpace += repeatStoneList_WithOneSpace.Count;

                // 連続したマスの先が空きマスであることを確認（※ 空きマスなほどオススメなマス）
                var baseStone = (repeatStoneList.Count == 0) ? stone : repeatStoneList.Last();
                var nextStone = baseStone.GetNextStone(stoneList, direction);
                if (nextStone.CanSetStone()) 
                {
                    settableStoneCount++;
                }
            }

            return new StoneAnalysis() 
            {
                Column = stone.Column,
                Row = stone.Row,
                Direction1 = checkDirection.Item1,
                Direction2 = checkDirection.Item2,
                RepeatStoneCount = repeatStoneCount,
                DirectionMyStoneCount = repeatStoneCount_WithOneSpace,
                SettableStoneCount = settableStoneCount,
            };
        }

        private void FinalizeStoneAnalysis(IEnumerable<StoneAnalysis> stoneAnalyseList) 
        {
            foreach (var stoneAnalyse in stoneAnalyseList) 
            {
                 var totalRepeatStoneCount = stoneAnalyseList
                    .Where(x => x.Column == stoneAnalyse.Column)
                    .Where(x => x.Row == stoneAnalyse.Row)
                    .Select(x => x.DirectionMyStoneCount)
                    .Sum();
                var totalSettableStoneCount = stoneAnalyseList
                    .Where(x => x.Column == stoneAnalyse.Column)
                    .Where(x => x.Row == stoneAnalyse.Row)
                    .Select(x => x.SettableStoneCount)
                    .Sum();
                stoneAnalyse.TotalDirectionMyStoneCount = totalRepeatStoneCount;
                stoneAnalyse.TotalSettableStoneCount = totalSettableStoneCount;
            }
        }
    }
}
