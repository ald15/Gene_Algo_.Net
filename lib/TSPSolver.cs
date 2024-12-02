namespace lib;

public class TSPSolver : IGeneAlgo
{
    // Amount of towns
    readonly long n;
    long generation;
    IEntity[] population; 
    IWorld matrix;  
    IConfig config; 
    IEntity best;

    public long Generation
    {
        get => generation;
    }
    public IConfig Config
    {
        get => config;
    }
    public IEntity[] Population
    {
        get => population;
    }
    public IWorld Matrix
    {
        get => matrix;
    }
    public IEntity Best
    {
        get => best;
    }

    public TSPSolver(long n, double[,] matrix, IConfig? config = null, IEntity[]? population = null)
    {
        this.n = n;
        this.matrix = new TSPMatrix(ref matrix);
        this.generation = 0;

        if (config == null)
        {
            this.config = new TSPConfig();
        }
        else
        {
            this.config = config;
        }

        this.population = new TSPPath[(this.config as TSPConfig).PopulationSize];
        if (population != null)
        {
            this.population = population;
        }
        else
        {
            NewPopulation(0, this.population.Length);
        }
        best = this.population[0];
        TSPPath.Fitness(ref best, ref this.matrix);
    }
    public void NewPopulation(long start, long end)
    {
        for (long i = start; i < end; i++)
        {
            population[i] = (IEntity)(new TSPPath(n - 1, generation));
            IEntity entity = population[i];
            TSPPath.Fitness(ref entity, ref matrix);
            population[i] = entity;
        }
        if (start < end)
        {
             generation++;
        }
    }
    public IEntity Selection()
    {
        TSPConfig? Config = config as TSPConfig;
        QSort.QuickSort(population, 0, population.Length - 1);
        if (population[0].FScore < best.FScore)
        {
            best = new TSPPath((TSPPath) population[0]);
        }
        long start = (long) (population.Length * Config.SurvivorsPart);
        NewPopulation(start, population.Length);
        return population[0];
    }
    public IEntity Evolve()
    {
        TSPConfig? Config = config as TSPConfig;
        for(long j = 0; j < population.Length; j++)
        {
            IEntity entity = population[j];
            TSPPath.Fitness(ref entity, ref matrix);
            TSPPath.Mutate(ref entity, Config.MutationProbability);
            TSPPath.Fitness(ref entity, ref matrix);
            population[j] = entity;
        }
        IEntity best = Selection();
        return best;
    }
    public async Task EvolveAsync()
    {
        TSPConfig? Config = config as TSPConfig;
        int processorCount = Environment.ProcessorCount;
        int lengthBase = population.Length / processorCount;
        int length_n = lengthBase + (population.Length % processorCount);
        var tasks = new Task[processorCount];
        for (int i = 0; i < processorCount; i++)
        {
            int index = i;
            tasks[i] = Task.Factory.StartNew(() =>
            {
                int lengthCurrent = (index == processorCount - 1) ? length_n : lengthBase;
                for(long j = 0; j < lengthCurrent; j++)
                {
                    IEntity entity = population[index * lengthBase + j];
                    TSPPath.Mutate(ref entity, Config.MutationProbability);
                    TSPPath.Fitness(ref entity, ref matrix);
                    population[index * lengthBase + j] = entity;
                }
            }, TaskCreationOptions.LongRunning);
        }
        await Task.WhenAll(tasks);
        Selection();
    }
}
