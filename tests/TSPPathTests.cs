using lib;

namespace tests;

public class TSPPathTests
{
    IEntity path;
    TSPMatrix matrix;

    public TSPPathTests()
    {
        double[,] data = 
        {
            {0, 1, 7, 4},
            {1, 0, 9, 5},
            {7, 9, 0, 8},
            {4, 5, 8, 0}
        };
        path = new TSPPath(4);
        matrix = new TSPMatrix(ref data);
    }
    [Fact]
    public void TestMutate()
    {
        var initialGenes = path.Genes.Clone();
        double mutationProbability = 1.0;
        TSPPath.Mutate(ref path, mutationProbability);
        Assert.NotEqual(initialGenes, path.Genes); 
        Assert.True(path.Mutations > 0);
    }
    [Fact]
    public void TestCompareTo()
    {
        var anotherPath = new TSPPath();
        anotherPath.FScore = 5;
        path.FScore = 10;
        int comparisonResult = path.CompareTo(anotherPath);
        Assert.True(comparisonResult > 0);
    }
    [Fact]
    public void TestFitness()
    {
        IEntity pathEntity = path;
        path.Genes = [1, 2, 3];
        IWorld world = matrix;
        var fScore = TSPPath.Fitness(ref pathEntity, ref world);
        Assert.Equal(22, fScore);
    }
}




