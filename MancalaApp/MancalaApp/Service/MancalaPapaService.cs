using MancalaApp.Dto;
using MancalaApp.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MancalaApp.Dto.Pocket;

namespace MancalaApp.Service
{
    public class MancalaPapaService
    {
        public int GetGoodPocketPosition(IEnumerable<Pocket> pocketList) 
        {
            // 連続して打てるなら、そこを返す。
            // 次に連続して打てるようになるなら、そこを返す。
            // 何もないなら、一番多いところを返す。

            // パパが置ける場所を取得
            var papaPocketList = pocketList
                .Where(x => x.Player == PlayerType.Player2)
                .Where(x => !x.IsGoal())
                .Where(x => x.StoneCount != 0)
                .ToList();

            var onceAgainPocket = papaPocketList.Where(x => x.StoneCount == x.Position).FirstOrDefault();
            if (onceAgainPocket != null) 
            {
                Log(onceAgainPocket, "連続して打てる位置");  // 自分の打つ回数を増やすことが、勝つコツ。最優先。
                return onceAgainPocket.Position;
            }

            var onceAgainPocket2 = papaPocketList.Where(x => ((x.StoneCount - x.Position) % pocketList.Count()) == 0).FirstOrDefault();
            if (onceAgainPocket2 != null)
            {
                Log(onceAgainPocket2, "連続して打てる位置（１周判定）");
                return onceAgainPocket2.Position;
            }


            var edgePocket = papaPocketList.FirstOrDefault(x => x.Position == 1);
            if (edgePocket != null)
            {
                Log(edgePocket, "ゴール手間の位置");    // ゴール手間を空にすると、次に連続して打てる可能性がアップ
                return edgePocket.Position;
            }

            var otherPocket = papaPocketList
                .OrderByDescending(x => x.StoneCount)
                .ThenByDescending(x => x.Position)
                .First();
            Log(otherPocket, "取り合えず、一番多い数");
            return otherPocket.Position;
        }

        private static void Log(Pocket pocket, string comment) 
        {
            Debug.WriteLine("位置:" + pocket.Position + ", 石数:" + pocket.StoneCount + ", " + comment);
        }

    }
}
