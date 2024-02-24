using MancalaApp.Dto;
using MancalaApp.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MancalaApp.Dto.Pocket;

namespace MancalaApp.Service
{
    public class MancalaService
    {
        public enum FinishRule 
        {
            先に石をなくした人,
            自分のゴールに石が多い人        
        }

        public PlayerType NowPlayer { get; set; } = PlayerType.Player1;
        public List<Pocket> PocketList { get; set; } = new List<Pocket>();

        public List<Pocket> Player1AreaList { get; set; } = new List<Pocket>();
        public List<Pocket> Player2AreaList { get; set; } = new List<Pocket>();
        public Pocket Player1Goal { get; set; } = new Pocket(PlayerType.Player1, 0, 0);
        public Pocket Player2Goal { get; set; } = new Pocket(PlayerType.Player2, 0, 0);

        public int DefaultSize { get; private set; } = 4;
        public IEnumerable<int> SupportSizeList { get; private set; } = new List<int>() { 1, 2, 3, 4, 5 };

        public int DefaultStoneCount { get; private set; } = 3;
        public IEnumerable<int> SupportDefaultStoneList { get; private set; } = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 16, 25, 36, 49 };

        public FinishRule NowFinishRule { get; private set; } = FinishRule.先に石をなくした人;
        public FinishRule DefaultFinishRule { get; private set; } = FinishRule.先に石をなくした人;
        public IEnumerable<FinishRule> SupportFinishRuleList { get; private set; } = new List<FinishRule>() { FinishRule.先に石をなくした人, FinishRule.自分のゴールに石が多い人 };


        public void Initialize(int size = 4, int defaultStoneCount = 3, FinishRule finishRule= FinishRule.先に石をなくした人) 
        {
            NowFinishRule = finishRule;
            NowPlayer = PlayerType.Player1;
            PocketList.Clear();
            PocketList.AddRange(createPocketList(size, defaultStoneCount, PlayerType.Player1).ToList());
            PocketList.AddRange(createPocketList(size, defaultStoneCount, PlayerType.Player2).ToList());

            // UI 用の情報をここから抽出する        
            SetUiInfo(PocketList);

            IEnumerable<Pocket> createPocketList(int size, int defaultStoneCount, PlayerType player)
            {
                yield return new Pocket(player, position: 0, stoneCount: 0);    // ゴールマス用
                foreach (var position in Enumerable.Range(1, size))
                {
                    yield return new Pocket(player, position, defaultStoneCount);
                }
            }
        }

        private void SetUiInfo(IEnumerable<Pocket> pocketList) 
        {
            Player1Goal = pocketList.Where(x => x.IsGoal()).Single(x => x.Player == PlayerType.Player1);
            Player2Goal = pocketList.Where(x => x.IsGoal()).Single(x => x.Player == PlayerType.Player2);
            Player1AreaList = pocketList.Where(x => !x.IsGoal()).Where(x => x.Player == PlayerType.Player1).ToList();
            Player2AreaList = pocketList.Where(x => !x.IsGoal()).Where(x => x.Player == PlayerType.Player2).OrderByDescending(x => x.Position).ToList();
        }

        private void ClearAddedStoneInfo() 
        {
            foreach (var pocket in PocketList) pocket.ClearAddStoneInfo();
        }

        public bool Move(PlayerType player, int position) 
        {
            ClearAddedStoneInfo();

            var pocket = PocketList.Where(x => x.Player == player).Where(x => x.Position == position).Single();
            var stoneCount = pocket.ClearStone();
            foreach (var index in Enumerable.Range(0, stoneCount))
            {
                pocket = pocket.Next(PocketList);
                pocket.AddStone();
            }

            // 再度うてるか確認
            var isOnceAgain = false;
            if (pocket.IsGoal() && pocket.Player == NowPlayer) isOnceAgain = true;

            // プレイヤーの切替
            if (!isOnceAgain) NowPlayer = NowPlayer == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;

            // UI用データのセット
            SetUiInfo(PocketList);

            return isOnceAgain;
        }

        public bool IsFinish(out PlayerType winPlayer) 
        {
            winPlayer = PlayerType.None;
            if (isEmptyPocket(PlayerType.Player1)) winPlayer = PlayerType.Player1;
            if (isEmptyPocket(PlayerType.Player2)) winPlayer = PlayerType.Player2;

            if(winPlayer == PlayerType.None) return false;

            if (NowFinishRule == FinishRule.自分のゴールに石が多い人) 
            {
                winPlayer = getGoalStoneCount(PlayerType.Player1) < getGoalStoneCount(PlayerType.Player2) ? 
                    PlayerType.Player2 : 
                    PlayerType.Player1;
            } 
            return true;

            bool isEmptyPocket(PlayerType player) => PocketList.Where(x => x.Player == player).Where(x => !x.IsGoal()).Select(x => x.StoneCount).Sum() == 0;
            int getGoalStoneCount(PlayerType player) => PocketList.Where(x => x.Player == player).Single(x => x.IsGoal()).StoneCount;
        }

    }
}
