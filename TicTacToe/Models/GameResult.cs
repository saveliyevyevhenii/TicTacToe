using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Enums;

namespace TicTacToe
{
    public class GameResult
    {
        public Player Winner { get; set; }

        public WinInfo WinInfo { get; set; }
    }
}
