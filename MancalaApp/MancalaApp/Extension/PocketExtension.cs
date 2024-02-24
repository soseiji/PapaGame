using MancalaApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MancalaApp.Extension
{
    public static class PocketExtension
    {
        public static bool IsGoal(this Pocket pocket) => pocket.Position == 0;

        public static Pocket Next(this Pocket pocket, IEnumerable<Pocket> pocketList)
        {
            if (pocketList == null) throw new ArgumentNullException(nameof(pocketList));
            if (!pocketList.Any()) throw new ArgumentNullException(nameof(pocketList));

            return pocket.IsGoal() ? 
                pocketList.Where(x => x.Player != pocket.Player).OrderByDescending(x => x.Position).First() :
                pocketList.Where(x => x.Player == pocket.Player).Single(x => x.Position == (pocket.Position - 1));
        }
    }
}
