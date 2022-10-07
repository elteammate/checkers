using Checkers.Logic;
using Checkers.Logic.AI;
using ShellProgressBar;

namespace Checkers.Tutor;

/// <summary>
///     The evolution algorithm.
/// </summary>
public static class Evolution
{
    private const int PopulationSize = 30;
    private const int GamesPerIndividual = 10;

    private const string LastGenerationNumberPath = "data/last-generation-number.txt";

    public static void Main()
    {
        var population = new List<NeuralNetwork>();
        for (var i = 0; i < PopulationSize; i++) population.Add(LoadOrCreate(i));

        for (var generation = LoadLastGenerationNumber();; generation++)
        {
            Console.WriteLine($"Generation {generation}");

            population = Generation(population);

            SavePopulation(population, generation);
            for (var i = 0; i < PopulationSize; i++)
                Save(population[i], i);

            SaveLastGenerationNumber(generation + 1);

            var someIndividual = population[0];
            var previousGenerationPerformance = "";

            for (var previousGeneration = 0;
                 previousGeneration < generation;
                 previousGeneration += Math.Max(1, generation / 10))
            {
                var res = Play(someIndividual, LoadFromPopulation(previousGeneration, 0));
                previousGenerationPerformance += res switch
                {
                    Game.GameResult.None => "D",
                    Game.GameResult.WhiteWins => "W",
                    Game.GameResult.BlackWins => "L",
                    Game.GameResult.Draw => "D",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            Console.WriteLine(previousGenerationPerformance);
        }
    }

    private static List<NeuralNetwork> Generation(IReadOnlyList<NeuralNetwork> population)
    {
        var random = new Random();
        var score = new int[PopulationSize];

        var waitingForGames = PopulationSize * GamesPerIndividual;

        var progressBar = new ProgressBar(waitingForGames, "Playing games...");

        using (var resetEvent = new ManualResetEvent(false))
        {
            for (var player = 0; player < PopulationSize; player++)
            for (var gameNumber = 0; gameNumber < GamesPerIndividual; gameNumber++)
            {
                var currentPlayer = player;

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var opponent = random.Next(PopulationSize - 1);
                    if (opponent >= currentPlayer)
                        opponent++;

                    var player1 = population[currentPlayer];
                    var player2 = population[opponent];

                    var result = Play(player1, player2);

                    lock (score)
                    {
                        if (result == Game.GameResult.WhiteWins)
                        {
                            score[currentPlayer] += 1;
                            score[opponent] -= 2;
                        }
                        else if (result == Game.GameResult.BlackWins)
                        {
                            score[opponent] += 1;
                            score[currentPlayer] -= 2;
                        }

                        progressBar.Tick();
                    }

                    if (Interlocked.Decrement(ref waitingForGames) == 0)
                        // ReSharper disable once AccessToDisposedClosure
                        resetEvent.Set();
                });
            }

            resetEvent.WaitOne();

            Console.WriteLine(string.Join(" ", score));
        }

        var bestHalf = population
            .Zip(score, (network, s) => (network, s))
            .OrderByDescending(x => x.s)
            .Take(PopulationSize / 2)
            .Select(x => x.network)
            .ToList();

        var newPopulation = new List<NeuralNetwork>();
        foreach (var parent in bestHalf)
        {
            newPopulation.Add(parent);
            newPopulation.Add(parent.Mutate());
        }

        return newPopulation;
    }

    private static Game.GameResult Play(
        NeuralNetwork white,
        NeuralNetwork black
    )
    {
        var game = GameFactory.Create();
        var moveCount = 0;

        while (game.Result == Game.GameResult.None && moveCount < 100)
        {
            var heuristic = game.CurrentPlayer == Color.White ? white : black;
            var move = new Solver(game, heuristic.GetEvaluator()).FindBestMove();

            game.MakeMove(move);
            moveCount++;
        }

        return game.Result;
    }

    private static NeuralNetwork LoadOrCreate(int id)
    {
        Directory.CreateDirectory("data/current");
        var path = $"data/current/{id}.json";

        if (File.Exists(path)) return NeuralNetwork.Load(path);

        var nn = new NeuralNetwork();
        nn.Save(path);
        return nn;
    }

    private static void Save(NeuralNetwork nn, int id)
    {
        Directory.CreateDirectory("data/current");
        nn.Save($"data/current/{id}.json");
    }

    private static void SavePopulation(IReadOnlyList<NeuralNetwork> population, int generation)
    {
        for (var i = 0; i < population.Count; i++)
        {
            var path = $"data/history/gen-{generation}";
            Directory.CreateDirectory(path);
            population[i].Save(path + "/{i}.json");
            File.Copy(path + "/{i}.json", $"../Checkers/Assets/networks/{i}.json", true);
        }
    }

    private static NeuralNetwork LoadFromPopulation(int generation, int id) =>
        NeuralNetwork.Load($"data/history/gen-{generation}/{id}.json");

    private static int LoadLastGenerationNumber() =>
        int.Parse(File.ReadAllText(LastGenerationNumberPath));

    private static void SaveLastGenerationNumber(int generation) =>
        File.WriteAllText(LastGenerationNumberPath, generation.ToString());
}
