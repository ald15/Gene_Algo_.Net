using lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfApp
{
    internal class JSONConverter
    {
        private class TempTSPPath
        {
            public long Generation { get; set; }
            public long Mutations { get; set; }
            public double FScore { get; set; }
            public long[] Genes { get; set; }
        }
        public static string MatrixToJson(double[,] matrix)
        {
            int rows = matrix.GetLength(0), cols = matrix.GetLength(1);
            double[][] array = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                array[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    array[i][j] = matrix[i, j];
                }
            }
            return JsonSerializer.Serialize(array);
        }
        public static double[,] JsonToMatrix(string json)
        {
            var array = JsonSerializer.Deserialize<double[][]>(json);
            int rows = array.Length, cols = array[0].Length;
            var matrix = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = array[i][j];
                }
            }
            return matrix;
        }
        public static string TSPPathToJson(TSPPath path)
        {
            var temp = new TempTSPPath
            {
                Generation = path.Generation,
                Mutations = path.Mutations,
                FScore = path.FScore,
                Genes = path.Genes
            };
            return JsonSerializer.Serialize(temp, new JsonSerializerOptions { WriteIndented = true });
        }
        public static TSPPath JsonToTSPPath(string json)
        {
            var temp = JsonSerializer.Deserialize<TempTSPPath>(json);
            var tspPath = new TSPPath(temp.Genes.Length, temp.Generation);
            tspPath.Mutations = temp.Mutations;
            tspPath.FScore = temp.FScore;
            tspPath.Genes = temp.Genes;
            return tspPath;
        }
        public static string PopulationToJson(TSPPath[] population)
        {
            var array = population.Select(path => new TempTSPPath
            {
                Generation = path.Generation,
                Mutations = path.Mutations,
                FScore = path.FScore,
                Genes = path.Genes
            }).ToArray();

            return JsonSerializer.Serialize(array, new JsonSerializerOptions { WriteIndented = true });
        }
        public static TSPPath[] JsonToPopulation(string json)
        {
            var array = JsonSerializer.Deserialize<TempTSPPath[]>(json);
            return array.Select(temp => {
                var tspPath = new TSPPath(temp.Genes.Length, temp.Generation);
                tspPath.Mutations = temp.Mutations;
                tspPath.FScore = temp.FScore;
                tspPath.Genes = temp.Genes;
                return tspPath;
            }).ToArray();
        }
    }
}
