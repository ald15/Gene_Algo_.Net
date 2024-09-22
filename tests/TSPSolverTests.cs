using lib;

namespace tests;

public class TSPSolverTests
{
    TSPSolver solver;
    TSPConfig config;
    public TSPSolverTests()
    {
        double[,] data = 
        {
            {0, 1, 7, 4},
            {1, 0, 9, 5},
            {7, 9, 0, 8},
            {4, 5, 8, 0}
        };
        config = new();
        solver = new(4, data, config);
    }
    [Fact]
    public void TestNewPopulation()
    {
        long start = 0, end = 10;
        var initialPopulation = (IEntity[])solver.Population.Clone();
        solver.NewPopulation(start, end);
        Assert.NotNull(solver.Population);
        Assert.NotEqual(initialPopulation, solver.Population);
    }
    [Fact]
    public void TestSelection()
    {
        long survivorsAmount = (long) (solver.Population.Length * config.SurvivorsPart);
        long victimsAmount = solver.Population.Length - survivorsAmount;
        solver.NewPopulation(0, solver.Population.Length);
        var initialPopulation = (TSPPath[])solver.Population.Clone();
        Array.Sort(initialPopulation);
        solver.Selection();
        var survivors1 = new ArraySegment<TSPPath>(initialPopulation, 0, (int) survivorsAmount);
        var survivors2 = new ArraySegment<TSPPath>((TSPPath[])solver.Population, 0, (int) survivorsAmount);
        var victims1 = new ArraySegment<TSPPath>(initialPopulation, (int) survivorsAmount, (int) victimsAmount);
        var victims2 = new ArraySegment<TSPPath>((TSPPath[])solver.Population, (int) survivorsAmount, (int) victimsAmount);
        Assert.Equal(survivors1, survivors2);
        Assert.Equal(victims1, victims2);
    }
    [Fact]
    public void TestEvolve()
    {
        solver.NewPopulation(0, solver.Population.Length);
        var initialPopulation = solver.Population.Clone();
        solver.Evolve();
        Assert.NotEqual(initialPopulation, solver.Population);
    }
}




