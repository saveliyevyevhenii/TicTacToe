using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Enums;
using TicTacToe.Models;

namespace TicTacToe
{
    public class GameState
    {
        public Player[,] GameGrid { get; private set; } = new Player[3, 3];

        public Player CurrentPlayer { get; private set; } = Player.X;

        private int TurnsPassed { get; set; } = 0;

        private bool GameOver { get; set; } = false;

        public event Action<int, int> MoveMade;

        public event Action<GameResult> GameEnded;

        public event Action GameRestarted;

        private bool CanMakeMove(int row, int column) => !GameOver && GameGrid[row, column] == Player.None;

        private bool IsGridFull() => TurnsPassed == 9;

        private void SwitchPlayer() => CurrentPlayer ^= Player.X ^ Player.O;

        private bool AreSquaresMarkered((int, int)[] squares, Player player)
            => squares.All(square => GameGrid[square.Item1, square.Item2] == player);

        private bool DidMoveWin(int row, int column, out WinInfo winInfo)
        {
            var winPatterns = new[]
            {
                new { Type = WinType.Row, Squares = new[] { (row, 0), (row, 1), (row, 2) } },
                new { Type = WinType.Column, Squares = new[] { (0, column), (1, column), (2, column) } },
                new { Type = WinType.MainDiagonal, Squares = new[] { (0, 0), (1, 1), (2, 2) } },
                new { Type = WinType.AlterDiagonal, Squares = new[] { (0, 2), (1, 1), (2, 0) } }
            };

            var winningPattern = winPatterns.FirstOrDefault(pattern
                => AreSquaresMarkered(pattern.Squares, CurrentPlayer));

            if (winningPattern != null)
            {
                winInfo = new WinInfo
                    { Type = winningPattern.Type, Number = winningPattern.Type == WinType.Row ? row : column };
                return true;
            }

            winInfo = null;
            return false;
        }

        private bool DidMoveEndGame(int row, int column, out GameResult gameResult)
        {
            if (DidMoveWin(row, column, out var winInfo))
            {
                gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }

            if (IsGridFull())
            {
                gameResult = new GameResult { Winner = Player.None };
                return true;
            }

            gameResult = null;
            return false;
        }

        public void MakeMove(int row, int column)
        {
            if (!CanMakeMove(row, column))
                return;

            GameGrid[row, column] = CurrentPlayer;
            TurnsPassed++;

            if (DidMoveEndGame(row, column, out var gameResult))
            {
                GameOver = true;
                GameEnded?.Invoke(gameResult);
            }
            else
            {
                SwitchPlayer();
            }

            MoveMade?.Invoke(row, column);
        }

        public void Reset()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
            GameRestarted?.Invoke();
        }
    }
}