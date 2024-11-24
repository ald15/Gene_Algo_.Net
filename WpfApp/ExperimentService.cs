using lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public class ExperimentService
    {
        public async Task SaveExperiment(string title, int nodesAmount, TSPConfig config, string best, TSPPath bestPath, string matrix, string population)
        {





            using var context = new ExperimentContext();

            var experiment = new Experiment
            {
                Title = title,
                NodeaAmount = nodesAmount,
                Epochs = config.Epochs,
                PopulationSize = config.PopulationSize,
                MutationProbability = config.MutationProbability,
                CrossoverProbability = config.CrossoverProbability,
                SurvivorsPart = config.SurvivorsPart,
                Best = best,
                BestFScore = bestPath.FScore,
                Matrix = matrix,
                Population = population
            };
            context.Experiments.Add(experiment);
            await context.SaveChangesAsync();
        }
    }
}
