using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GomokuApp.Extension.StoneExtensions;

namespace GomokuApp.Dto
{
    public class StoneAnalysis
    {
        public int Column { get; set; }
        public int Row { get; set; }

        public DirectionType Direction1 { get; set; }
        public DirectionType Direction2 { get; set; }

        public int RepeatStoneCount { get; set; }                          // 最優先：連続マスの数（これが一番大事！！！ ｎ目ならべなので）勝利判定用。
        public int SettableStoneCount { get; set; }                        // 前提チェック：連続マスの先に配置できる数（その先に空きマスがないと意味ないので確認。これが 0 の場所には置かない方が良い）
        // 以下、おすすめマス判定用の要素
        public int DirectionMyStoneCount { get; set; }                   // 優先度1：指定方向にある自身のマス数（一つ飛びも考慮した数）
        public int TotalDirectionMyStoneCount { get; set; }            // 優先度2：各方向の自身のマス数の合計値
        public int TotalSettableStoneCount { get; set; }                 // 優先度3：連続マスの先に配置できる数の合計値

        public void DebugLog(string comment) 
        {
            var debugComment = "[白] 行列:(" + Row.ToString("D2") + "," + Column.ToString("D2") + ")" +
                ", Count[rep]:" + RepeatStoneCount +
                ", Count[dic]:" + DirectionMyStoneCount +
                ", TotalCount[dic]:" + TotalDirectionMyStoneCount +
                ", TotalSetEmp:" + TotalSettableStoneCount +
                ", " + comment;
            MainWindow.DebugLog(debugComment);
        }
    }
}
