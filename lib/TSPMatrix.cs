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
    public static double[,] Generate(int size, int max)
    {
        Random rand = new Random();
        double[,] distances = new double[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j++)
            {
                distances[i, j] = distances[j, i] = rand.Next(1, max);
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    if (i != j && j != k && i != k)
                    {
                        double sumDistances = distances[i, k] + distances[k, j];
                        if (distances[i, j] > sumDistances)
                        {
                            distances[i, j] = distances[j, i] = sumDistances;
                        }
                    }
                }
            }
        }

        return distances;
    }
}
