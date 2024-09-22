namespace lib;

public class TSPMatrix : IWorld
{
    double[,] data;
    public double[,] Data
    {
        set
        {
            data = value;
        }
        get => data;
    }
    public TSPMatrix(ref double[,] matrix) {
        data = matrix;
    }
}
