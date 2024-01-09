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
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
        }

        private bool CanMakeMove(int row, int column) => !GameOver && GameGrid[row, column] == Player.None;

        private bool IsGridFull() => TurnsPassed == 9;

        private void SwitchPlayer() => CurrentPlayer ^= Player.X ^ Player.O;

        private bool areSquaresMarkered((int, int)[] squares, Player player) 
            => squares.All(square => GameGrid[square.Item1, square.Item2] != player);


    }
}
