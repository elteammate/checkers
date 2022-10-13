using Checkers.Logic;
using Checkers.Logic.AI;
using ShellProgressBar;

namespace Checkers.Tutor;

/// <summary>
///     The evolution algorithm.
/// </summary>
public static class Evolution
{
    private const int PopulationSize = 300;
    private const int GamesPerIndividual = 8;
    private const int ReinforcementGamesPerIndividual = 10;

    private const string LastGenerationNumberPath = "data/last-generation-number.txt";

    public static void Main()
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        var population = new List<NeuralNetwork>();
        for (var i = 0; i < PopulationSize; i++) population.Add(LoadOrCreate(i));

        for (var generation = LoadLastGenerationNumber();; generation++)
        {
            Console.WriteLine($"Generation {generation}");

            population = Generation(population, generation);

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

    private static List<NeuralNetwork> Generation(IReadOnlyList<NeuralNetwork> population, int gen)
    {
        var random = new Random();
        var score = new int[PopulationSize];

        var totalGamesPerIndividual =
            GamesPerIndividual + (gen > 10 ? ReinforcementGamesPerIndividual : 0);
        var waitingForGames = PopulationSize * totalGamesPerIndividual;
        var progressBar = new ProgressBar(waitingForGames, "Playing games...");

        using (var resetEvent = new ManualResetEvent(false))
        {
            for (var player = 0; player < PopulationSize; player++)
            for (var gameNumber = 0;
                 gameNumber < totalGamesPerIndividual;
                 gameNumber++)
            {
                var currentPlayer = player;
                var number = gameNumber;

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    NeuralNetwork player1, player2;
                    int? opponent = null;

                    if (number < GamesPerIndividual)
                    {
                        opponent = random.Next(PopulationSize - 1);
                        if (opponent >= currentPlayer)
                            opponent++;
                        player1 = population[currentPlayer];
                        player2 = population[opponent!.Value];
                    }
                    else
                    {
                        player1 = population[currentPlayer];
                        player2 = LoadFromPopulation(
                            random.Next(gen / 2),
                            random.Next(PopulationSize));
                    }

                    var result = Play(player1, player2);

                    lock (score)
                    {
                        if (result == Game.GameResult.WhiteWins)
                        {
                            score[currentPlayer] += 1;
                            if (opponent.HasValue) score[opponent.Value] -= 2;
                        }
                        else if (result == Game.GameResult.BlackWins)
                        {
                            if (opponent.HasValue) score[opponent.Value] += 1;
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
            .ToArray();

        for (var i = 0; i < PopulationSize / 2; i++)
        {
            bestHalf[i].s -= bestHalf[^1].s;
        }

        var newGen = bestHalf.Select(x => x.network).ToList();
        for (var i = 0; i < PopulationSize / 2; i++)
        {
            var parent1 =
                bestHalf[RandomWeightedIndex(random, bestHalf.Select(x => x.s).ToArray())].network;
            var parent2 =
                bestHalf[RandomWeightedIndex(random, bestHalf.Select(x => x.s).ToArray())].network;
            newGen.Add(NeuralNetwork.Crossover(parent1, parent2));
        }

        for (var mutation = 0; mutation < 50; mutation++)
        {
            var index = random.Next(PopulationSize);
            newGen[index] = newGen[index].Mutate();
        }

        return newGen;
    }

    private static int RandomWeightedIndex(Random random, int[] weights)
    {
        var total = weights.Sum();
        var r = random.Next(total);
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (sum > r)
                return i;
        }

        throw new Exception("Should not happen");
    }

    public static Game.GameResult Play(
        NeuralNetwork white,
        NeuralNetwork black
    )
    {
        var game = GameFactory.Create();
        var moveCount = 0;

        while (game.Result == Game.GameResult.None && moveCount < 100)
        {
            var heuristic = game.CurrentPlayer == Color.White ? white : black;
            var move = new Solver(game, heuristic.GetEvaluator(), 3).FindBestMove();

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
        for (var id = 0; id < population.Count; id++)
        {
            var path = $"data/history/gen-{generation}";
            Directory.CreateDirectory(path);
            population[id].Save(path + $"/{id}.json");
            File.Copy(path + $"/{id}.json", $"../Checkers/Assets/networks/{id}.json", true);
        }
    }

    private static NeuralNetwork LoadFromPopulation(int generation, int id) =>
        NeuralNetwork.Load($"data/history/gen-{generation}/{id}.json");

    private static int LoadLastGenerationNumber() =>
        int.Parse(File.ReadAllText(LastGenerationNumberPath));

    private static void SaveLastGenerationNumber(int generation) =>
        File.WriteAllText(LastGenerationNumberPath, generation.ToString());
}
