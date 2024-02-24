using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GomokuApp.Dto
{
    public class Stone
    {
        public Stone()
        {
            Color = ColorType.None;
            Row = -1;
            Column = -1;
        }
        public Stone(ColorType color,  int row, int column)
        {
            Color = color;
            Row = row; 
            Column = column;
        }
        public Stone(Stone stoneTmpe)
        {
            Color = stoneTmpe.Color;
            Row = stoneTmpe.Row;
            Column = stoneTmpe.Column;
            IsPutted = stoneTmpe.IsPutted;
            IsFinished = stoneTmpe.IsFinished;
        }


    public enum ColorType
        {
            None = 0,
            Black,
            White,
        }
        public ColorType Color { get; private set; } = ColorType.None;
        public void SetColor(ColorType color) => Color = color;

        public int Row { get; } = 0;
        public int Column { get; } = 0;

        public bool IsPutted { get; private set; } = false;             // UI色付け用
        public bool IsFinished { get; private set; } = false;       // UI色付け用
        public void Put() => IsPutted = true;
        public void Finish() => IsFinished = true;
        public void ClearPutInfo() 
        {
            IsPutted = false;
            IsFinished = false;
        }

    }
}
