namespace lib;
public class TSPPath : IEntity
{
    readonly long generation;
    long mutations;
    double fitness;
    long[] genes;

    public long Generation 
    {
        get => generation;
    }
    public long Mutations 
    {
        set
        {
            mutations = value;
        }
        get => mutations;
    }
    public double FScore
    {
        set
        {
            fitness = value;
        }
        get => fitness;
    }
    public long[] Genes
    {
        set
        {
            genes = value;
        }
        get => genes;
    }
    public TSPPath(TSPPath path)
    {
        generation = path.generation;
        mutations = path.mutations;
        fitness = path.fitness;
        genes = path.genes;
    }
    public TSPPath(long n = 0, long generation = 0) 
    {
        this.generation = generation;
        mutations = 0;
        fitness = 0;
        genes = new long[n];
        for (long i = 0; i < n; i++)
        {
            genes[i] = i + 1;
        }
        Random.Shared.Shuffle(genes);
    }
    public int CompareTo(Object? path) 
    {
        return FScore.CompareTo(((TSPPath) path).FScore);
    }

    public static void Mutate(ref IEntity path, double probability) 
    {
        Random rand = new();
        bool toMutate = rand.Next(0, 99) <= (int) 100 * probability;
        if (path.Genes.Length > 0 && toMutate)
        {
            long i = rand.Next(1, path.Genes.Length);
            long j = rand.Next(1, path.Genes.Length);
            while (i == j)
            {
                i = rand.Next(1, path.Genes.Length);
                j = rand.Next(1, path.Genes.Length);
            }
            (path.Genes[j], path.Genes[i]) = (path.Genes[i], path.Genes[j]);
            path.Mutations += 1;
        }
    }
    public static void Crossover(ref IEntity entity1, ref IEntity entity2, double probability) 
    {
        // It is not used in this task
    }
    public static double Fitness(ref IEntity path, ref IWorld matrix) {
        TSPMatrix? Matrix = matrix as TSPMatrix;
        double length = 0;
        long i = 0;
        foreach (long j in path.Genes)
        {
            length += Matrix.Data[i, j];
            i = j;
        }
        length += Matrix.Data[i, 0];
        path.FScore = length;
        return length;
    }
    public override string ToString()
    {
        string s = "[G: " + generation.ToString() + "] Path: {0} -> ";
        foreach (var gene in genes)
        {
            s += "{" + gene.ToString()  + "} -> ";
        }
        s += "{0}; Length: " + fitness.ToString();
        return s;
    }
}
