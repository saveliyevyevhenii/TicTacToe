using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicTacToe
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Player, ImageSource> _imageSources = new()
        {
            { Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
            { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
        };

        private readonly Image[,] _imageControls = new Image[3, 3];
        private readonly GameState _gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            SetupGameGrid();

            _gameState.MoveMade += OnMoveMade;
            _gameState.GameEnded += OnGameEnded;
            _gameState.GameRestarted += OnGameRestarted;
        }

        private void TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            TurnPannel.Visibility = GameCanvas.Visibility = Visibility.Hidden;
            ResultText.Text = text;
            WinnerImage.Source = winnerImage;
            EndScreen.Visibility = Visibility.Visible;
        }

        private void TransitionToGameScreen()
        {
            EndScreen.Visibility = Line.Visibility = Visibility.Hidden;
            TurnPannel.Visibility = GameCanvas.Visibility = Visibility.Visible;
        }

        private void SetupGameGrid()
        {
            Enumerable.Range(0, 3)
                .SelectMany(row => Enumerable.Range(0, 3)
                    .Select(col => _imageControls[row, col] = new Image()))
                .ToList()
                .ForEach(imageControl => GameGrid.Children.Add(imageControl));
        }

        private (Point start, Point end) FindLinePoints(WinInfo winInfo)
        {
            var squareSize = GetSquareSize();
            var margin = squareSize / 2;

            return winInfo.Type switch
            {
                WinType.Row => (new Point(0, winInfo.Number * squareSize + margin),
                    new Point(GameGrid.Width, winInfo.Number * squareSize + margin)),
                WinType.Column => (new Point(winInfo.Number * squareSize + margin, 0),
                    new Point(winInfo.Number * squareSize + margin, GameGrid.Height)),
                WinType.MainDiagonal => (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height)),
                WinType.AntiDiagonal => (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void ShowLine(WinInfo winInfo)
        {
            var (start, end) = FindLinePoints(winInfo);

            Line.X1 = start.X;
            Line.Y1 = start.Y;
            Line.X2 = end.X;
            Line.Y2 = end.Y;
            Line.Visibility = Visibility.Visible;
        }

        private void OnMoveMade(int row, int col)
        {
            if (_gameState == null || _imageControls == null || _imageSources == null || PlayerImage == null)
            {
                return;
            }

            UpdateGameGrid(row, col);
            UpdatePlayerImage();
        }

        private void UpdateGameGrid(int row, int col)
        {
            var currentPlayer = _gameState.GameGrid[row, col];
            _imageControls[row, col].Source = _imageSources[currentPlayer];
        }

        private void UpdatePlayerImage()
        {
            if (_imageSources.TryGetValue(_gameState.CurrentPlayer, value: out var source))
            {
                PlayerImage.Source = source;
            }
        }

        private async void OnGameEnded(GameResult gameResult)
        {
            await Task.Delay(500);

            if (gameResult.Winner != Player.None)
            {
                ShowLine(gameResult.WinInfo);
                await Task.Delay(500);
            }

            TransitionToEndScreen(
                gameResult.Winner == Player.None ? "Draw!" : "Winner:",
                gameResult.Winner == Player.None ? null : _imageSources[gameResult.Winner]
            );
        }

        private void OnGameRestarted()
        {
            foreach (var imageControl in _imageControls.Cast<Image>())
            {
                imageControl.Source = null;
            }

            PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];
            TransitionToGameScreen();
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var squareSize = GetSquareSize();
            var (row, column) = GetClickedSquare(e.GetPosition(GameGrid), squareSize);

            _gameState.MakeMove(row, column);
        }

        private double GetSquareSize()
        {
            return GameGrid.Width / 3;
        }

        private (int row, int column) GetClickedSquare(Point clickPosition, double squareSize)
        {
            return ((int)(clickPosition.Y / squareSize), (int)(clickPosition.X / squareSize));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _gameState.Reset();
        }
    }
}