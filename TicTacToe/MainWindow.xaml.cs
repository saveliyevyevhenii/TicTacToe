﻿using System;
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

        private void SetupGameGrid()
        {
            Enumerable.Range(0, 3)
                .SelectMany(row => Enumerable.Range(0, 3)
                    .Select(col => _imageControls[row, col] = new Image()))
                .ToList()
                .ForEach(imageControl => GameGrid.Children.Add(imageControl));
        }

        private void OnMoveMade(int row, int col)
        {
        }

        private void OnGameEnded(GameResult gameResult)
        {
        }

        private void OnGameRestarted()
        {
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var squareSize = GameGrid.Width / 3;
            var clickPosition = e.GetPosition(GameGrid);
            var row = (int)(clickPosition.Y / squareSize);
            var column = (int)(clickPosition.X / squareSize);
            
            _gameState.MakeMove(row, column);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}