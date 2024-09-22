namespace lib;

public class TSPConfig : IConfig
{
    /// <summary>
    /// Количество эпох.
    /// </summary>
    public long Epochs;
    /// <summary>
    /// Размер популяции.
    /// -1 для бесконечной.
    /// </summary>
    public long PopulationSize;
    /// <summary>
    /// Вероятность мутации особи.
    /// </summary>
    public double MutationProbability;
    /// <summary>
    /// Вероятность скрещивания особей.
    /// </summary>
    public double CrossoverProbability;
    /// <summary>
    /// Доля выживших особей.
    /// </summary>
    public double SurvivorsPart;
    public TSPConfig()
    {
        Epochs = 100000;
        PopulationSize = 1000;
        MutationProbability = 0.5;
        CrossoverProbability = 0.5;
        SurvivorsPart = 0.5;
    }
}
