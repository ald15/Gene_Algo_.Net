using lib;

namespace app;

public class Solver
{
    bool infinity;
    TSPConfig config;
    TSPPath[] population;
    TSPSolver solver;
    ConsoleKeyInfo cki;
    bool toShowDetails = false;
    public Solver(long n, double[,] data, TSPConfig config, TSPPath[] population = null)
    {
        infinity = false;
        this.config = config;
        this.population = population;
        solver = new(n, data, config, population);
    }
    public void Solve()
    {
        bool exit = false;
        bool stop = true;
        if (config.Epochs == -1)
        {
            infinity = true;
        }
        
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            stop = true;
            Console.WriteLine("\n\n>>> [INFO] The solver has been paused. Press <C> to continue or <X> to exit...");
        };

        StartMessage();
        HelpMessage();
        while (!exit) 
        {
            cki = Console.ReadKey(true);
            switch (cki.Key)
            {
                case ConsoleKey.Enter:
                case ConsoleKey.C:
                    stop = false;
                    break;
                case ConsoleKey.X:
                    exit = true;
                    break;
                case ConsoleKey.D:
                    toShowDetails = !toShowDetails;
                    Console.WriteLine(">>> [INFO] Details mode = " + toShowDetails);
                    break;
                case ConsoleKey.H:
                    HelpMessage();
                    break;
            }
            for (long i = 0; !stop && ((!infinity && i < config.Epochs) || infinity); i++)
            {
                IEntity current = solver.Evolve();
                Console.Clear();
                StartMessage();
                Console.WriteLine("\n\t  [Generation (G): " + (solver.Generation - 1) +  "/" + config.Epochs + "]");
                Console.WriteLine("\t   [Populatioion size: " + config.PopulationSize + "]\n");
                PrintBest(current, solver.Best);
                if (!infinity && (solver.Generation > config.Epochs))
                {
                    exit = true;
                    break;
                }
            }
        }
        ExitMessage();
    }
    public async Task SolveAsync()
    {
        int processorCount = Environment.ProcessorCount;
        Console.WriteLine("P:" + processorCount);
        int i = 5000;
        while (i>0)
            {

                await solver.EvolveAsync();
                Console.WriteLine("Все задачи завершены.");
                Console.Clear();
                Console.WriteLine("\n\t  [Generation (G): " + (solver.Generation - 1) +  "/" + config.Epochs + "]");
                Console.WriteLine("\t   [Populatioion size: " + config.PopulationSize + "]\n");
                Console.WriteLine(solver.Best.FScore);
                i--;
            }
        Console.WriteLine("Все поколения завершены.");
        Console.WriteLine(solver.Best);
    }
    void StartMessage()
    {
        string version = "0.1";
        string msg = "\n\t|| TSP Solver (v " + version + ") by ald15 ||";
        Console.WriteLine(msg);
    }
    void HelpMessage()
    {
        string help = "\n\n\t\t[Commands]\n\n";
        string c1 = "\t1. <Enter> - start solving;\n";
        string c2 = "\t2. <X> - exit program;\n";
        string c3 = "\t3. <Ctr+C> - pause solving;\n";
        string c4 = "\t4. <C> - continue solving;\n";
        string c5 = "\t5. <D> - show/hide solution details;\n";
        string c6 = "\t6. <H> - show this commands.\n";
        string commands = c1 + c2 + c3 + c4 + c5 + c6;
        string msg = help + commands + "\n";
        Console.WriteLine(msg);
    }
    void ExitMessage()
    {
        string msg = ">>> [INFO] The solver was stopped...\n";
        Console.WriteLine(msg);
    }
    void PrintBest(IEntity path, IEntity best)
    {
        string curMsg = ">>> {current} \n\t[G: " + path.Generation + "] Score = " + path.FScore; 
        string bestMsg = "\n    {best} \n\t[G:" + best.Generation + "] Score = " + best.FScore; 
        if (toShowDetails)
        {
            curMsg = ">>> {current}\n    \t" + path;
            bestMsg = "\n    {best}\n    \t" + best;
        }
        string msg = curMsg + bestMsg;
        Console.WriteLine(msg);
    }
}
