using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TicTacToe.Enums;
using TicTacToe.Models;

namespace TicTacToe
{
    public partial class MainWindow
    {
        private readonly Dictionary<Player, ImageSource> _imageSources = new()
        {
            { Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
            { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
        };

        private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> _animations = new()
        {
            { Player.X, new ObjectAnimationUsingKeyFrames() },
            { Player.O, new ObjectAnimationUsingKeyFrames() }
        };

        private readonly DoubleAnimation _fadeOutAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(0.5),
            From = 1,
            To = 0
        };

        private readonly DoubleAnimation _fadeInAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(0.5),
            From = 0,
            To = 1
        };

        private readonly Image[,] _imageControls = new Image[3, 3];
        private readonly GameState _gameState = new();

        public MainWindow()
        {
            InitializeComponent();
            SetupGameGrid();
            SetupAnimations();

            _gameState.MoveMade += OnMoveMade;
            _gameState.GameEnded += OnGameEnded;
            _gameState.GameRestarted += OnGameRestarted;
        }

        private async Task AnimateOpacity(UIElement uiElement, AnimationTimeline animation)
        {
            uiElement.BeginAnimation(OpacityProperty, animation);
            await Task.Delay(animation.Duration.TimeSpan);
        }

        private async Task FadeOut(UIElement uiElement)
        {
            await AnimateOpacity(uiElement, _fadeOutAnimation);
            uiElement.Visibility = Visibility.Hidden;
        }

        private async Task FadeIn(UIElement uiElement)
        {
            uiElement.Visibility = Visibility.Visible;
            await AnimateOpacity(uiElement, _fadeInAnimation);
        }

        private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
            ResultText.Text = text;
            WinnerImage.Source = winnerImage;
            await FadeIn(EndScreen);
        }

        private async Task TransitionToGameScreen()
        {
            await FadeOut(EndScreen);
            Line.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
        }

        private void SetupAnimations()
        {
            _animations[Player.X].Duration = TimeSpan.FromSeconds(0.25);
            _animations[Player.O].Duration = TimeSpan.FromSeconds(0.25);

            for (var i = 0; i < 16; i++)
            {
                SetAnimationKeyFrame(Player.X, $"x{i}.png");
                SetAnimationKeyFrame(Player.O, $"o{i}.png");
            }
        }

        private void SetAnimationKeyFrame(Player player, string imageName)
        {
            var imageUri = new Uri($"pack://application:,,,/Assets/{imageName}");
            var image = new BitmapImage(imageUri);
            var keyFrame = new DiscreteObjectKeyFrame(image);
            _animations[player].KeyFrames.Add(keyFrame);
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
                WinType.AlterDiagonal => (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async Task ShowLine(WinInfo winInfo)
        {
            var (start, end) = FindLinePoints(winInfo);

            Line.X1 = start.X;
            Line.Y1 = start.Y;

            var x2Animation = CreateAnimation(start.X, end.X);
            var y2Animation = CreateAnimation(start.Y, end.Y);

            Line.Visibility = Visibility.Visible;
            Line.BeginAnimation(Line.X2Property, x2Animation);
            Line.BeginAnimation(Line.Y2Property, y2Animation);

            await Task.Delay(x2Animation.Duration.TimeSpan);
        }

        private static DoubleAnimation CreateAnimation(double from, double to)
        {
            return new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.25),
                From = from,
                To = to
            };
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
            _imageControls[row, col].BeginAnimation(Image.SourceProperty, _animations[currentPlayer]);
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
                await ShowLine(gameResult.WinInfo);
                await Task.Delay(500);
            }

            await TransitionToEndScreen(
                gameResult.Winner == Player.None ? "Draw!" : "Winner:",
                gameResult.Winner == Player.None ? null : _imageSources[gameResult.Winner]
            );
        }

        private async void OnGameRestarted()
        {
            foreach (var imageControl in _imageControls.Cast<Image>())
            {
                imageControl.BeginAnimation(Image.SourceProperty, null);
                imageControl.Source = null;
            }

            PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];
            await TransitionToGameScreen();
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

        private static (int row, int column) GetClickedSquare(Point clickPosition, double squareSize)
        {
            return ((int)(clickPosition.Y / squareSize), (int)(clickPosition.X / squareSize));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _gameState?.Reset();
        }
    }
}