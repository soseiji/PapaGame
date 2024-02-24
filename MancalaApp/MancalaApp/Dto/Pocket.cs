using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MancalaApp.Dto
{
    public class Pocket
    {
        public enum PlayerType 
        {
            None,
            Player1,
            Player2,
        }

        public PlayerType Player { get; private set; }
        public int Position { get; private set; }        //   int Position  0:(Goal )  1,2,3,4  (1～ Area)
        public int StoneCount { get; private set; }
        public int AddedStoneCount { get; private set; }

        public Pocket(PlayerType player, int position, int stoneCount)
        {
            Player = player;
            Position = position;
            StoneCount = stoneCount;
            AddedStoneCount = 0;
        }

        public int ClearStone() 
        {
            ClearAddStoneInfo();
            var tmp = StoneCount;
            StoneCount = 0;
            return tmp;
        }

        public void AddStone()
        {
            StoneCount++;
            AddedStoneCount++;
        }

        public void ClearAddStoneInfo() 
        {
            AddedStoneCount = 0;
        }

    }
}
