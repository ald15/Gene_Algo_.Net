using Microsoft.EntityFrameworkCore;

namespace WpfApp
{
    public  class Experiment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int NodeaAmount { get; set; }
        public long Epochs { get; set; }
        public long PopulationSize { get; set; }
        public double MutationProbability { get; set; }
        public double CrossoverProbability { get; set; }
        public double SurvivorsPart { get; set; }
        public string Best { get; set; } = string.Empty;
        public string BestPath { get; set; } = string.Empty;
        public double BestFScore { get; set; }
        public string Matrix { get; set; } = string.Empty;
        public string Population { get; set; } = string.Empty;
    }

    public class ExperimentContext : DbContext
    {
        public DbSet<Experiment> Experiments { get; set; }

        public ExperimentContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ExperimentDB;Trusted_Connection=True;");
        }
    }
}
