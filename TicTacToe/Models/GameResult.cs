using TicTacToe.Enums;

namespace TicTacToe.Models
{
    public class GameResult
    {
        public Player Winner { get; set; }

        public WinInfo WinInfo { get; set; }
    }
}