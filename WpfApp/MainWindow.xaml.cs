using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using lib;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
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
        long[] pathCurrent;
        long[] pathLast;

        // Vars for GUI
        long epochs;
        long populationSize;
        double mutationProbability;
        double crossoverProbability;
        double survivorsPart;
        double maxDistance;
        double[,] matrix;
        int statusBarMax;
        int statusBarValue;

        TSPConfig config;
        TSPSolver solver;
        TSPPath best;

        public int NodesAmount
        {
            set
            {
                bool success = int.TryParse(value.ToString(), out nodesAmount);
            }
            get => nodesAmount;
        }
        public long Epochs
        {
            set
            {
                bool success = long.TryParse(value.ToString(), out epochs);
            }
            get => epochs;
        }
        public long PopulationSize
        {
            set
            {
                bool success = long.TryParse(value.ToString(), out populationSize);
            }
            get => populationSize;
        }
        public double MutationProbability
        {
            set
            {
                bool success = double.TryParse(value.ToString(), out mutationProbability);
            }
            get => mutationProbability;
        }
        public double CrossoverProbability
        {
            set
            {
                bool success = double.TryParse(value.ToString(), out crossoverProbability);
            }
            get => crossoverProbability;
        }
        public double SurvivorsPart
        {
            set
            {
                bool success = double.TryParse(value.ToString(), out survivorsPart);
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
            set { }
            get
            {
                string s = "", row = "", temp = "";
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
            set { }
            get
            {
                return best.ToString();
            }
        }


        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Base settings of Canvas
            baseNodeWidth = 45;
            baseNodeHeight = baseNodeWidth;
            baseFontSise = 15;
            baseR = 200;
            baseNodesAmount = 20;
            margin = 0.20;
            scale = new double[2] { 1, 1 };

            nodesLables = [];
            edges = [];
            centre = [];
            nodes = [];
            pathCurrent = [];
            pathLast = [];



            
            nodesAmount = 20;
            maxDistance = 100;
            matrix = TSPMatrix.Generate(nodesAmount, (int) maxDistance);

            config = new();
            solver = new(nodesAmount, matrix, config);
            best = new();
      

            epochs = config.Epochs;
            populationSize = config.PopulationSize;
            mutationProbability = config.MutationProbability;
            crossoverProbability = config.CrossoverProbability;
            survivorsPart = config.SurvivorsPart;



        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void drawNodes()
        {
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
                    Text = "\n" + i.ToString(),
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.Black,
                    FontSize = fontSize,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(textBlock, x);
                Canvas.SetTop(textBlock, y);
                Plot.Children.Add(textBlock);
            }
        }

        public void drawEdges()
        {
            long x = 0;
            for (long i = 1; i < nodesAmount; i++)
            {
                for (long j = 0; j < i; j++)
                {
                    //Debug.WriteLine(i + " " + j);
                    double x0 = Canvas.GetLeft(nodes[i]) + nodeWidth / 2;
                    double y0 = Canvas.GetTop(nodes[i]) + nodeHeight / 2;
                    double x1 = Canvas.GetLeft(nodes[j]) + nodeWidth / 2;
                    double y1 = Canvas.GetTop(nodes[j]) + nodeHeight / 2;
                    edges[x] = new Line()
                    {
                        X1 = x0,
                        Y1 = y0,
                        X2 = x1,
                        Y2 = y1,
                        Stroke = Brushes.Crimson,
                        StrokeThickness = 2
                    };
                    Canvas.SetZIndex(edges[x], -1);
                    Plot.Children.Add(edges[x]);
                    x++;
                }
            }
        }
        public void randomEdgeSelect()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void ButtonGen_Click(object sender, RoutedEventArgs e)
        {
            matrix = TSPMatrix.Generate(nodesAmount, (int) maxDistance);
            OnPropertyChanged(nameof(Matrix));
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            for (long i = 0; (config.Epochs == -1 || i < config.Epochs); i++)
            {
                IEntity current = solver.Evolve();
                best = (TSPPath) solver.Best;
                OnPropertyChanged(nameof(Best));
            }
        }

        public void pathClear()
        {

        }
        public void pathSelect()
        {

        }
        public void pathParse(string path)
        {

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            // Генерация случайного цвета для линии
            edges[random.Next((int)edges.Length - 1)].Stroke = new SolidColorBrush(Color.FromRgb(
                (byte)random.Next(256),
                (byte)random.Next(256),
                (byte)random.Next(256)
            ));
        }
        public void drawCenter()
        {
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

        private void Plot_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("H:" + Plot.Height + " W:" + Plot.Width);
            nodes = new Ellipse[nodesAmount];
            nodesLables = new string[0];
            edges = new Line[nodesAmount * (nodesAmount - 1) / 2];
            centre = new double[2] { (Plot.ActualWidth) / 2, Plot.ActualHeight / 2 };
            Debug.WriteLine("Centre:" + centre[0] + " " + centre[1]);
            pathCurrent = new long[nodesAmount - 1];
            pathLast = new long[nodesAmount - 1];
            r *= 1 - margin;

            drawNodes();
            drawCenter();
            drawEdges();
            //randomEdgeSelect();
        }

        private void Plot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Plot.Children.Clear();
            scale[0] = Plot.ActualWidth / Plot.MinWidth;
            scale[1] = Plot.ActualHeight / Plot.MinHeight;
            double minScale = Math.Min(scale[0], scale[1]);

            nodeWidth = baseNodeWidth * minScale * Math.Min(1, baseNodesAmount / nodesAmount);
            nodeHeight = nodeWidth;
            r = (1 - margin) * baseR * minScale;
            fontSize = baseFontSise * minScale * Math.Min(1, baseNodesAmount / nodesAmount);

            Debug.WriteLine("H:" + Plot.ActualHeight + " W:" + Plot.ActualWidth);
            nodes = new Ellipse[nodesAmount];
            nodesLables = new string[0];
            edges = new Line[nodesAmount * (nodesAmount - 1) / 2];
            centre = new double[2] { (Plot.ActualWidth) / 2, Plot.ActualHeight / 2 };
            Debug.WriteLine("Centre:" + centre[0] + " " + centre[1]);
            pathCurrent = new long[nodesAmount - 1];
            pathLast = new long[nodesAmount - 1];


            drawNodes();
            drawCenter();
            drawEdges();
        }
    }
}
