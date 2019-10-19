namespace Intellichess.UI {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Intellichess.Core;

    public partial class MainWindow : Window {
        private int _rowsCount;
        private int _columnsCount;
        private Point _targetPosition;
        private BackgroundWorker _worker;

        private readonly Stopwatch _watch = new Stopwatch();
        private readonly IList<IList<Point>> _foundRoutes = new List<IList<Point>>();

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ShowParametersSelector();
        }

        private void ShowParametersSelector() {
            var selector = new ParametersWindow {
                Owner = this
            };

            selector.ShowDialog();

            if (selector.ParametersProvided) {
                _rowsCount = selector.SelectedRowsCount;
                _columnsCount = selector.SelectedColumnsCount;
                _targetPosition = new Point(selector.TargetColumn, selector.TargetRow);

                Start();
            } else
                secTryAgain.IsEnabled = true;
        }

        private void Start() {
            _watch.Restart();

            Reset();
            InitializeGrid();

            var targetWeight = _targetPosition.ComputeWeight();

            var gridData = tilesContainer.Children.GetData(new Point(1, 1));

            _worker = new BackgroundWorker {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += (s, e) => MoveForward(e, gridData, null, targetWeight);

            _worker.ProgressChanged += (s, e) => {
                                           foundRoutCount.Content = routesLog.Items.Count;
                                           currentTime.Content = _watch.Elapsed;

                                           var currentRoute = (List<Point>) e.UserState;
                                           LogRoute(currentRoute);
                                       };

            _worker.RunWorkerCompleted += (s, e) => {
                                              _watch.Stop();

                                              if (e.Cancelled)
                                                  status.Text = "Operation cancelled!";
                                              else
                                                  status.Text = "Operation completed!";

                                              tryAgain.Visibility = Visibility.Visible;
                                              requestCancellation.Visibility = Visibility.Collapsed;
                                              noThanks.Visibility = Visibility.Visible;

                                              foundRoutCount.Content = routesLog.Items.Count;
                                              currentTime.Content = _watch.Elapsed;

                                              Cursor = null;
                                          };

            _worker.RunWorkerAsync();
        }

        private void Reset() {
            tilesContainer.Children.Clear();
            tilesContainer.ColumnDefinitions.Clear();
            tilesContainer.RowDefinitions.Clear();
            _foundRoutes.Clear();
            routesLog.Items.Clear();
            status.Text = "Generating possible routes. Please wait...";
            currentTime.Content = "00:00:00:0000";
            notifyPanel.Visibility = Visibility.Visible;
            tryAgain.Visibility = Visibility.Collapsed;
            requestCancellation.Visibility = Visibility.Visible;
            requestCancellation.Content = "Request cancellation";
            requestCancellation.IsEnabled = true;
            noThanks.Visibility = Visibility.Collapsed;
            Cursor = Cursors.AppStarting;
        }

        private void InitializeGrid() {
            for (int i = 0; i <= _rowsCount + 1; i++) {
                tilesContainer.RowDefinitions.Add(new RowDefinition {
                    Height = new GridLength(1, GridUnitType.Star)
                });
            }

            for (int j = 0; j <= _columnsCount + 1; j++) {
                tilesContainer.ColumnDefinitions.Add(new ColumnDefinition {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }

            for (int i = 1; i <= _rowsCount; i++) {
                for (int j = 1; j <= _columnsCount; j++) {
                    var gridData = new GridData {
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(1),
                        Position = new Point(j, i)
                    };

                    var textBlock = new TextBlock {
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Text = string.Format("X: {0}, Y: {1}, W: {2}", gridData.Position.X, gridData.Position.Y, gridData.Position.ComputeWeight())
                    };

                    gridData.Content = textBlock;

                    if (gridData.Position.IsBetween(new Point(1, 1), new Point(_targetPosition.X, _targetPosition.Y))) {
                        gridData.Background = Utilities.InnerAreaTileColor();
                        gridData.BorderBrush = Utilities.InnerAreaBorderColor();
                    } else {
                        gridData.Background = Utilities.OuterAreaTileColor();
                        gridData.BorderBrush = Utilities.OuterAreaBorderColor();
                    }

                    tilesContainer.Children.Add(gridData);

                    Grid.SetRow(gridData, i);
                    Grid.SetColumn(gridData, j);
                }
            }
        }

        private void MoveForward(CancelEventArgs e, GridData gridData, ICollection<Point> walkedRoute, int targetWeight) {
            if (_worker.CancellationPending) {
                e.Cancel = true;
                return;
            }

            var currentRoute = new List<Point>();

            AppendPreviouslyWalkedRoute(currentRoute, walkedRoute);

            var moveDetected = false;

            if (ValidateMove(gridData.Position.X, 1, _targetPosition.X)) {
                var nextPoint = new Point(gridData.Position.X + 1, gridData.Position.Y);

                if (ThisRouteIsNotWalkedYet(currentRoute, nextPoint)) {
                    GridData nextData = null;

                    Dispatcher.Invoke(new Action(() => { nextData = tilesContainer.Children.GetData(nextPoint); }));

                    var nextRoute = currentRoute.ToList();
                    nextRoute.Add(nextPoint);

                    MoveForward(e, nextData, nextRoute, targetWeight);

                    moveDetected = true;
                }
            }

            if (ValidateMove(gridData.Position.Y, 1, _targetPosition.Y)) {
                var nextPoint = new Point(gridData.Position.X, gridData.Position.Y + 1);

                if (ThisRouteIsNotWalkedYet(currentRoute, nextPoint)) {
                    GridData nextData = null;

                    Dispatcher.Invoke(new Action(() => { nextData = tilesContainer.Children.GetData(nextPoint); }));

                    var nextRoute = currentRoute.ToList();
                    nextRoute.Add(nextPoint);

                    MoveForward(e, nextData, nextRoute, targetWeight);

                    moveDetected = true;
                }
            }

            if (!moveDetected && WeightsAreIdentical(currentRoute, targetWeight) && !WasTargetWalked(currentRoute)) {
                _foundRoutes.Add(currentRoute);
                _worker.ReportProgress(1, currentRoute);
            }
        }

        private void AppendPreviouslyWalkedRoute(ICollection<Point> currentRoute, ICollection<Point> walkedRoutes) {
            if (walkedRoutes == null) {
                currentRoute.Add(new Point {
                    X = 1,
                    Y = 1
                });

                return;
            }

            if (walkedRoutes.Count == 0)
                return;

            foreach (var walkedRoute in walkedRoutes)
                currentRoute.Add(walkedRoute);
        }

        private bool ValidateMove(double source, int amount, double target) {
            return source + amount <= target;
        }

        private bool ThisRouteIsNotWalkedYet(IEnumerable<Point> currentRoute, Point nextPoint) {
            return true;

            var combinedRoute = currentRoute.ToList();

            combinedRoute.Add(nextPoint);

            foreach (var foundRoute in _foundRoutes)
                foreach (var point in foundRoute) {}
        }

        private bool WeightsAreIdentical(IList<Point> route, int targetWeight) {
            if (route == null || !route.Any())
                return false;

            return route.Last().ComputeWeight() == targetWeight;
        }

        private bool WasTargetWalked(IList<Point> currentRoute) {
            var pointComparer = new PointComparer();

            foreach (var foundRoute in _foundRoutes) {
                if (foundRoute.Count == currentRoute.Count) {
                    var areSame = true;

                    for (int i = 0; i < foundRoute.Count; i++) {
                        if (!pointComparer.Equals(foundRoute[i], currentRoute[i])) {
                            areSame = false;
                            break;
                        }
                    }

                    if (areSame)
                        return true;
                }
            }

            return false;
        }

        private void LogRoute(IList<Point> currentRoute) {
            var logData = new RouteLogData {
                Route = currentRoute,
                Path = currentRoute.ToRoutePath()
            };

            routesLog.Items.Add(logData);
        }

        private void routesLog_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (routesLog.SelectedIndex == -1)
                return;

            HighlightRoute(((RouteLogData) routesLog.SelectedItem).Route);
        }

        private void HighlightRoute(IList<Point> route) {
            var pointComparer = new PointComparer();

            for (int i = 1; i <= _targetPosition.Y; i++) {
                for (int j = 1; j <= _targetPosition.X; j++) {
                    var gridData = tilesContainer.Children.GetData(new Point {
                        X = j,
                        Y = i
                    });

                    if (route.Any(r => pointComparer.Equals(r, gridData.Position))) {
                        gridData.Background = Utilities.CurrentAreaTileColor();
                        gridData.BorderBrush = Utilities.CurrentAreaBorderColor();
                    } else {
                        gridData.Background = Utilities.InnerAreaTileColor();
                        gridData.BorderBrush = Utilities.InnerAreaBorderColor();
                    }
                }
            }
        }

        private void tryAgain_Click(object sender, RoutedEventArgs e) {
            ShowParametersSelector();
        }

        private void noThanks_Click(object sender, RoutedEventArgs e) {
            notifyPanel.Visibility = Visibility.Collapsed;
            secTryAgain.IsEnabled = true;
        }

        private void secTryAgain_Click(object sender, RoutedEventArgs e) {
            secTryAgain.IsEnabled = false;
            ShowParametersSelector();
        }

        private void requestCancellation_Click(object sender, RoutedEventArgs e) {
            if (_worker.IsBusy && !_worker.CancellationPending) {
                _worker.CancelAsync();
                status.Text = "Operation is under cancellation...";
                requestCancellation.IsEnabled = false;
                requestCancellation.Content = "Cancelling...";
            }
        }
    }
}