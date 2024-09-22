namespace lib;

public interface IWorld;

public interface IConfig;

public interface IEntity : IComparable
{
    long Generation 
    {
        get;
    }
    long Mutations 
    {
        set;
        get;
    }
    // Fitness score
    double FScore
    {
        set;
        get;
    }
    long[] Genes
    {
        set;
        get;
    }
    public new int CompareTo(Object? entity); 
    abstract public static void Mutate(ref IEntity entity, double probability);
    abstract public static void Crossover(ref IEntity entity1, ref IEntity entity2, double probability);
    abstract public static double Fitness(ref IEntity entity, ref IWorld data);
}

public interface IGeneAlgo
{
    IConfig Config 
    {
        get;
    }
    IEntity[] Population 
    {
        get;
    }
   IEntity Best {
        get;
    }
    void NewPopulation(long start, long end);
    IEntity Selection();
    IEntity Evolve();
}
