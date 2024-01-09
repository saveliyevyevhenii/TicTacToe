using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class GameState
    {
        public Player[,] GameGrid { get; private set; }

        public Player CurrentPlayer { get; private set; }

        public int TurnsPassed { get; private set; }

        public bool GameOver { get; private set; }

        public event Action<int, int> MoveMade;

        public event Action<GameResult> GameEnded;

        public event Action GameRestarted;

        public GameState()
        {
            GameGrid = new Player[3,3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
        }


    }
}
