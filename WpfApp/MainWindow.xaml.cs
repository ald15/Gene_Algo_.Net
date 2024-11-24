using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using lib;
using OxyPlot.Series;
using OxyPlot;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    public class Experiment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int NodeaAmount { get; set; }
        public long Epochs { get; set; }
        public long PopulationSize { get; set; }
        public double MutationProbability { get; set; }
        public double CrossoverProbability { get; set; }
        public double SurvivorsPart { get; set; }
        public string Best { get; set; } = string.Empty;
        public double BestFScore { get; set; }
        public string Matrix { get; set; } = string.Empty;
        public string Population { get; set; } = string.Empty;

        // TODO
        // Здесь нужно добавить сохранение лучшей популяции -> написать функцию перевода IEntyty Popolutaion -> strting JSON
        // Соответсвенно, string JSON -> IEntyty Popolutaion
    }

    public class ExperimentContext : DbContext
    {
        public DbSet<Experiment> Experiments { get; set; }

        public ExperimentContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ExperimentDB;Trusted_Connection=True;");
        }
    }


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        int nodesAmount;

        // Vars for graph
        string[] nodesLables;
        Line[] edges;
        double[] centre;
        Ellipse[] nodes;
        double nodeWidth, nodeHeight, r, fontSize;
        double baseNodeWidth, baseNodeHeight, baseR, baseFontSise, baseNodesAmount;
        double margin;
        double[] scale;
        long[] path;

        // Vars for GUI
        long epochs;
        long curEpoch;
        long populationSize;
        double mutationProbability;
        double crossoverProbability;
        double survivorsPart;
        double maxDistance;
        double[,] matrix;
        long statusBarValue;
        private CancellationTokenSource _cancellationTokenSource;
        bool visualization;
        string best;


        // TSPSolver
        TSPConfig config;
        int processorCount;
        TSPSolver[] solvers;
        TSPPath bestPath;

        // OxyPlot

        private LineSeries fitnessSeries;
        private PlotModel plotModel;
        private ObservableCollection<DataPoint> dataPoints;
        private DispatcherTimer timerOxy;

        // DB
        private readonly ExperimentService _experimentService = new ExperimentService();

        public int NodesAmount
        {
            set
            {
                bool success = int.TryParse(value.ToString(), out nodesAmount);
                ButtonGen_Click(null, null);
            }
            get => nodesAmount;
        }
        public long Epochs
        {
            set
            {
                bool success = long.TryParse(value.ToString(), out epochs);
                if (success) config.Epochs = epochs;
                OnPropertyChanged(nameof(Epochs));
            }
            get => epochs;
        }
        public long PopulationSize
        {
            set
            {
                bool success = long.TryParse(value.ToString(), out populationSize);
                if (success) config.PopulationSize = populationSize;
                OnPropertyChanged(nameof(PopulationSize));
            }
            get => populationSize;
        }
        public double MutationProbability
        {
            set
            {
                bool success = double.TryParse(value.ToString(), out mutationProbability);
                if (success) config.MutationProbability = mutationProbability;
                OnPropertyChanged(nameof(MutationProbability));
            }
            get => mutationProbability;
        }
        public double CrossoverProbability
        {
            set
            {
                bool success = double.TryParse(value.ToString(), out crossoverProbability);
                if (success) config.CrossoverProbability = crossoverProbability;
                OnPropertyChanged(nameof(CrossoverProbability));
            }
            get => crossoverProbability;
        }
        public double SurvivorsPart
        {
            set
            {
                bool success = double.TryParse(value.ToString(), out survivorsPart);
                if (success) config.SurvivorsPart = survivorsPart;
                OnPropertyChanged(nameof(SurvivorsPart));
            }
            get => survivorsPart;
        }
        public double MaxDistance
        {
            set
            {
                bool success = double.TryParse(value.ToString(), out maxDistance);
            }
            get => maxDistance;
        }
        public string Matrix
        {
            set 
            {
                //OnPropertyChanged(nameof(Matrix));
            }
            get
            {
                string s = "", row = "";
                for (int i = 0; i < nodesAmount; i++)
                {
                    row = "";
                    for (int j = 0; j < nodesAmount; j++)
                    {
                        row += matrix[i, j].ToString().PadRight(maxDistance.ToString().Length) + " ";
                    }
                    s += row + "\n";
                }
                return s;
            }
        }
        public string Best
        {
            set
            { 
                best = value.ToString();
            }
            get => best;
        }
        public long StatusBarValue
        {
            set
            {
                statusBarValue = value;
                OnPropertyChanged(nameof(StatusBarValue));
            }
            get => statusBarValue;
        }
        public long CurEpoch
        {
            set
            {
                curEpoch = value;
                OnPropertyChanged(nameof(CurEpoch));
            }
            get => curEpoch;
        }
        public bool Visualization
        {
            set
            {
                visualization = value;
            }
            get => visualization;
        }

        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Base settings of Canvas
            Visualization = true;
            baseNodeWidth = 35;
            baseNodeHeight = baseNodeWidth;
            baseFontSise = 15;
            baseR = 140;
            baseNodesAmount = 20;
            margin = 0.10;
            scale = new double[2] { 1, 1 };

            nodesLables = [];
            edges = [];
            centre = [];
            nodes = [];
            path = [];


            nodesAmount = 20;
            maxDistance = 100;
            matrix = TSPMatrix.Generate(nodesAmount, (int) maxDistance);

            config = new();
            config.Epochs = 1000;
            
            processorCount = Environment.ProcessorCount;
            solvers = new TSPSolver[processorCount];
            best = string.Empty;

            for (int i = 0; i < processorCount; i++)
            {
                solvers[i] = new TSPSolver(nodesAmount, matrix, config);
            }

            bestPath = (TSPPath)solvers[0].Best;
            epochs = config.Epochs;
            populationSize = config.PopulationSize;
            mutationProbability = config.MutationProbability;
            crossoverProbability = config.CrossoverProbability;
            survivorsPart = config.SurvivorsPart;

            drawStatistics();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void drawNodes()
        {
            Plot.Children.Clear();
            nodes = new Ellipse[NodesAmount];
            nodesLables = new string[0];
            double angle = (double)(2 * Math.PI / nodesAmount);
            for (int i = 0; i < nodesAmount; i++)
            {
                nodes[i] = new Ellipse()
                {
                    Width = nodeWidth,
                    Height = nodeHeight,
                    Fill = Brushes.Khaki,
                    Stroke = Brushes.Crimson,
                    StrokeThickness = 1
                };
                double x = centre[0] + r * Math.Cos(i * angle) - nodeWidth / 2;
                double y = centre[1] + r * Math.Sin(i * angle) - nodeWidth / 2;
                Canvas.SetLeft(nodes[i], x);
                Canvas.SetTop(nodes[i], y);

                Plot.Children.Add(nodes[i]);
                TextBlock textBlock = new TextBlock
                {
                    Height = nodeHeight,
                    Width = nodeWidth,
                    //Background = Brushes.Green,
                    Text = "" + i.ToString(),
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.Black,
                    FontSize = fontSize,
                };
                Canvas.SetZIndex(textBlock, 1);
                Canvas.SetLeft(textBlock, x);
                Canvas.SetTop(textBlock, y);
                Plot.Children.Add(textBlock);
            }
        }

        public void drawEdges()
        {
            // Draw all edges
            long x = 0;
            for (long i = 1; i < nodesAmount; i++)
            {
                for (long j = 0; j < i; j++)
                {
                    edges[x] = new Line()
                    {
                        X1 = Canvas.GetLeft(nodes[i]) + nodeWidth / 2,
                        Y1 = Canvas.GetTop(nodes[i]) + nodeHeight / 2,
                        X2 = Canvas.GetLeft(nodes[j]) + nodeWidth / 2,
                        Y2 = Canvas.GetTop(nodes[j]) + nodeHeight / 2,
                        Stroke = Brushes.Crimson,
                        StrokeThickness = 2
                    };
                    Canvas.SetZIndex(edges[x], -1);
                    Plot.Children.Add(edges[x]);
                    x++;
                }
            }
        }
        private void ButtonGen_Click(object sender, RoutedEventArgs e)
        {
            matrix = TSPMatrix.Generate(nodesAmount, (int) MaxDistance);
            OnPropertyChanged(nameof(Matrix));
            for (int i = 0; i < processorCount; i++)
            {
                solvers[i] = new TSPSolver(nodesAmount, matrix, config);
            }
            drawNodes();
        }
        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            StatusBarValue = 0;
            _cancellationTokenSource = new CancellationTokenSource();
            btStart.IsEnabled = false;
            btGen.IsEnabled = false;
            btDbSave.IsEnabled = false;
            btDbLoad.IsEnabled = false;
            btDbDelete.IsEnabled = false;
            tbNodesAmount.IsEnabled = tbNodesEpochs.IsEnabled = tbPopulationSize.IsEnabled =
                tbMutationProbability.IsEnabled = tbCrossoverProbability.IsEnabled = tbSurvivorsPart.IsEnabled = false;
            dataPoints.Clear();
            timerOxy.Start();
            try
            {
                await RunGenAlgoAsync(_cancellationTokenSource.Token);
                MessageBox.Show("Эволюция была успешно завершена!\n" +
                    $"Найдена особь с оценкой приспособленности: {bestPath.FScore}.", "Эволюция завершена", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Эволюция была остановлена!\n" +
                    $"Найдена особь с оценкой приспособленности: {bestPath.FScore}.", "Эволюция остановлена", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            finally
            {
                btStart.IsEnabled = true;
                btGen.IsEnabled = true;
                btDbSave.IsEnabled = true;
                btDbLoad.IsEnabled = true;
                btDbDelete.IsEnabled = true;
                tbNodesAmount.IsEnabled = tbNodesEpochs.IsEnabled = tbPopulationSize.IsEnabled =
                    tbMutationProbability.IsEnabled = tbCrossoverProbability.IsEnabled = tbSurvivorsPart.IsEnabled = true;
                timerOxy.Stop();
            }
        }
        private async Task RunGenAlgoAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                var tasks = new Task[processorCount];
                Debug.WriteLine("Processcors: " + processorCount);
                for (long i = 0; (epochs == -1 || i < epochs); i++)
                {
                        for (int j = 0; j < processorCount; j++)
                        {
                            int index = j;
                            tasks[j] = Task.Factory.StartNew(() =>
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                solvers[index].Evolve();
                            });
                        }

                        await Task.WhenAll(tasks);
                        double max = 0;
                        bestPath = (TSPPath) solvers[0].Best;
                        for (int j = 0; j < processorCount; j++)
                        {
                            if (solvers[j].Best.FScore > max)
                            {
                                best = ((TSPPath) solvers[j].Best).ToString();
                                bestPath = (TSPPath) solvers[j].Best;
                            }
                        }
                        OnPropertyChanged(nameof(Best));
                        CurEpoch = i + 1;
                        StatusBarValue = (i + 1) * 100 / epochs;
                        Application.Current.Dispatcher.Invoke(() => dataPoints.Add(new DataPoint(CurEpoch, bestPath.FScore)));
                    }
                }, cancellationToken);
        }
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите закрыть программу?", "Подтверждение закрытия",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private async void btDbSave_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение результатов в БД
            string title = tbDbTitle.Text;
            await _experimentService.SaveExperiment(title, nodesAmount, config, best, bestPath, "", "");
            Debug.WriteLine("Результаты сохранены в базу данных.");
        }

        private void btDbLoad_Click(object sender, RoutedEventArgs e)
        {
            using var context = new ExperimentContext();
            var experiments = context.Experiments.ToList();
            lbDbList.ItemsSource = experiments.Select(exp => $"#: {exp.Id}, Title: {exp.Title}, Score: {exp.BestFScore}");
        }

        private void lbDbList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using var context = new ExperimentContext();
            var experiments = context.Experiments.ToList();
            var experimentFromDb = context.Experiments.FirstOrDefault(x => x.Id == lbDbList.SelectedIndex + 1);
            Debug.WriteLine($"Selected index: {lbDbList.SelectedIndex}");
                if (experimentFromDb != null)
                {
                    // Загрузка данных из записи
                    tbDbTitle.Text = experimentFromDb.Title;
                    
                    

                    Epochs = experimentFromDb.Epochs;
                    config.Epochs = experimentFromDb.Epochs;

                    nodesAmount = experimentFromDb.NodeaAmount;
                    OnPropertyChanged(nameof(NodesAmount));
                    drawNodes();

                    PopulationSize = experimentFromDb.PopulationSize;
                    config.PopulationSize = experimentFromDb.PopulationSize;

                    MutationProbability = experimentFromDb.MutationProbability;
                    config.MutationProbability = experimentFromDb.MutationProbability;

                    CrossoverProbability = experimentFromDb.CrossoverProbability;
                    config.CrossoverProbability = experimentFromDb.CrossoverProbability;

                    SurvivorsPart = experimentFromDb.SurvivorsPart;
                    config.SurvivorsPart = experimentFromDb.SurvivorsPart;

                    
                    
                    //Best = experimentFromDb.Best;
                    //BestFScore = bestPath.FScore
                }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!visualization) return;
            pathDraw();
        }

        private void CheckBoxVisualization_Click(object sender, RoutedEventArgs e)
        {
            if (!visualization)
            {
                Plot.Children.Clear();
            }
            else
            {
                Plot_SizeChanged(sender, null);
            }
            
        }
        public void pathDraw()
        {
            edges = new Line[nodesAmount];
            drawNodes();
            long prev = 0;
            if (best == string.Empty) return;
            for (long i = 0; i < bestPath.Genes.Length; i++)
            {
                edges[i] = new Line()
                {
                    X1 = Canvas.GetLeft(nodes[prev]) + nodeWidth / 2,
                    Y1 = Canvas.GetTop(nodes[prev]) + nodeHeight / 2,
                    X2 = Canvas.GetLeft(nodes[bestPath.Genes[i]]) + nodeWidth / 2,
                    Y2 = Canvas.GetTop(nodes[bestPath.Genes[i]]) + nodeHeight / 2,
                    Stroke = Brushes.Green,
                    StrokeThickness = 2
                };
                //Canvas.SetZIndex(edges[i], -1);
                Plot.Children.Add(edges[i]);
                prev = bestPath.Genes[i];
            }
            Line temp = new Line()
            {
                X1 = Canvas.GetLeft(nodes[prev]) + nodeWidth / 2,
                Y1 = Canvas.GetTop(nodes[prev]) + nodeHeight / 2,
                X2 = Canvas.GetLeft(nodes[0]) + nodeWidth / 2,
                Y2 = Canvas.GetTop(nodes[0]) + nodeHeight / 2,
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };
            //Canvas.SetZIndex(temp, -1);
            Plot.Children.Add(temp);
        }
        public void drawCenter()
        {
            // Draw the center of the graph
            Ellipse node = new Ellipse()
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.Blue,
                StrokeThickness = 3
            };
            Canvas.SetLeft(node, centre[0]);
            Canvas.SetTop(node, centre[1]);
            Plot.Children.Add(node);
        }
        private void Plot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!visualization) return;
            double temp = Math.Max(Plot.ActualHeight, Plot.ActualWidth);
            Plot.MinHeight = Plot.MinWidth = temp;
            scale[0] = Plot.ActualWidth / 400;
            scale[1] = Plot.ActualHeight / 400;
            double minScale = Math.Min(scale[0], scale[1]);
            nodeWidth = baseNodeWidth * Math.Min(1, baseNodesAmount / nodesAmount);
            nodeHeight = nodeWidth;
            r = (1 + margin) * baseR * minScale;
            fontSize = baseFontSise * Math.Min(1, baseNodesAmount / nodesAmount);
            centre = new double[2] { (Plot.ActualWidth) / 2, Plot.ActualHeight / 2 };
            pathDraw();
            //drawNodes();
            //drawCenter();
            //drawEdges();
        }
        private void drawStatistics()
        {
            dataPoints = new ObservableCollection<DataPoint>();
            plotModel = new PlotModel { Title = "Оценка приспособленности лучшей особи от эпохи" };
            fitnessSeries = new LineSeries
            {
                Title = "Приспособленность",
                MarkerType = MarkerType.Diamond,
                Color = OxyColors.Red
                //LineStyle = LineStyle.Solid
            };
            plotModel.Series.Add(fitnessSeries);
            PlotView.Model = plotModel;
            timerOxy = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timerOxy.Tick += (s, e) => UpdatePlot();
        }
        private void UpdatePlot()
        {
            fitnessSeries.Points.Clear();
            fitnessSeries.Points.AddRange(dataPoints);
            plotModel.InvalidatePlot(true);
        }
    }
}
