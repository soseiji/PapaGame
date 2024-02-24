using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiOthelloApp
{
    public class Stone
    {
        public Stone()
        {
            Color = ColorType.None;
            Row = -1;
            Column = -1;
        }
        public Stone(ColorType color, int row, int column)
        {
            Color = color;
            Row = row;
            Column = column;
        }

        public enum ColorType
        {
            None = 0,
            Black,
            White,
            Red,
            Blue,
        }
        public ColorType Color { get; private set; } = ColorType.None;
        public void SetColor(ColorType color) => Color = color;

        public int Row { get; } = 0;
        public int Column { get; } = 0;

        public bool IsPutted { get; private set; } = false;
        public bool IsReversed { get; private set; } = false;
        public bool CanReverse { get; private set; } = false;
        public void Put() => IsPutted = true;
        public void Reverse() => IsReversed = true;
        public void SetCanReverse() => CanReverse = true;
        public void ClearPutInfo()
        {
            IsPutted = false;
            IsReversed = false;
            CanReverse = false;
        }
    }
}
