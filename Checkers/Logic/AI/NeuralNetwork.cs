using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jil;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Checkers.Logic.AI;

using Layer = Matrix<double>;

/// <summary>
///     A neural network used to evaluate heuristics.
///     Based on https://www.researchgate.net/publication/3302690
/// </summary>
public class NeuralNetwork
{
    /// <summary>
    ///     A number of neurons in each layer.
    ///     Similar to the article, but slightly different for better performance on modern hardware.
    /// </summary>
    private static readonly int[] Layers = { 32, 60, 10, 1 };

    /// <summary>
    ///     A total count of weights in the network.
    /// </summary>
    private static readonly int WightsCount =
        Layers.Zip(Layers.Skip(1), (a, b) => a * (b + 1)).Sum();

    /// <summary>
    ///     Constant used to normalize the mutation rate.
    /// </summary>
    private const double Tau = 1.0 / 30.0;

    /// <summary>
    ///     A value given to the king upon evaluation.
    ///     Black king is evaluated as -K, white king is evaluated as K,
    ///     black piece is evaluated as -1, white piece is evaluated as 1.
    ///     Clamped between 1.2 and 3.
    /// </summary>
    private double _k = 2;

    /// <summary>
    ///     Weights (and biases) of the network.
    /// </summary>
    private Layer[] _weights;

    /// <summary>
    ///     Creates a new neural network with random weights and mutation rates of 0.05
    /// </summary>
    public NeuralNetwork()
    {
        _weights = new Layer[Layers.Length - 1];

        var normal = new Normal(0, 1);
        for (var i = 0; i < Layers.Length - 1; i++)
        {
            _weights[i] = DenseMatrix.CreateRandom(Layers[i] + 1, Layers[i + 1], normal);
        }
    }

    /// <summary>
    ///     Mutates the network.
    /// </summary>
    public NeuralNetwork Mutate()
    {
        var newWeights = new Layer[_weights.Length];

        var normal = new Normal(0, 1);

        for (var i = 0; i < _weights.Length; i++)
        {
            var random = DenseMatrix.CreateRandom(
                _weights[i].RowCount,
                _weights[i].ColumnCount,
                normal);

            newWeights[i] = _weights[i] + random * Tau;
        }

        var newK = _k * Math.Exp(1 / Math.Sqrt(2) * normal.Sample());
        if (newK < 1.2) newK = 1.2;
        if (newK > 3) newK = 3;

        return new NeuralNetwork
        {
            _weights = newWeights,
            _k = newK
        };
    }

    public static NeuralNetwork[] MutatePopulation(NeuralNetwork[] population)
    {
        var normal = new Normal(0, 1);

        var newPopulation = new NeuralNetwork[population.Length * 2];

        var sigmaRandom = new DenseMatrix[Layers.Length - 1];
        for (var i = 0; i < population[0]._weights.Length; i++)
        {
            sigmaRandom[i] = DenseMatrix.CreateRandom(
                population[0]._weights[i].RowCount,
                population[0]._weights[i].ColumnCount,
                normal);
        }

        for (var populationIndex = 0; populationIndex < population.Length; populationIndex++)
        {
            var nn = population[populationIndex];
            newPopulation[populationIndex] = population[populationIndex];

            var newWeights = new Layer[nn._weights.Length];

            for (var i = 0; i < nn._weights.Length; i++)
            {
                var random = DenseMatrix.CreateRandom(
                    nn._weights[i].RowCount,
                    nn._weights[i].ColumnCount,
                    normal);

                newWeights[i] = nn._weights[i] + random * Tau;
            }

            var newK = nn._k * Math.Exp(1 / Math.Sqrt(2) * normal.Sample());
            if (newK < 1.2) newK = 1.2;
            if (newK > 3) newK = 3;

            newPopulation[populationIndex + population.Length] = new NeuralNetwork
            {
                _weights = newWeights,
                _k = newK
            };
        }

        return newPopulation;
    }

    public static NeuralNetwork Crossover(NeuralNetwork nn1, NeuralNetwork nn2)
    {
        var random = new Random();
        var newK = random.Next(2) == 0 ? nn1._k : nn2._k;
        var newWeights = new Layer[nn1._weights.Length];

        for (var i = 0; i < nn1._weights.Length; i++)
        {
            newWeights[i] = nn1._weights[i];
            for (var j = 0; j < nn1._weights[i].RowCount; j++)
            {
                for (var k = 0; k < nn1._weights[i].ColumnCount; k++)
                {
                    if (random.Next(2) == 0)
                    {
                        newWeights[i][j, k] = nn1._weights[i][j, k];
                    }
                    else
                    {
                        newWeights[i][j, k] = nn2._weights[i][j, k];
                    }
                }
            }
        }

        return new NeuralNetwork
        {
            _weights = newWeights,
            _k = newK
        };
    }

    /// <summary>
    ///     Runs the network on the given board.
    /// </summary>
    private double Evaluate(Vector<double> input)
    {
        var output = input;

        foreach (var layer in _weights)
        {
            output = layer.TransposeThisAndMultiply(new DenseVector(output.Append(1).ToArray()));
            output = output.PointwiseTanh();
        }

        return output[0];
    }

    /// <summary>
    ///     Runs the network on the given board.
    /// </summary>
    private double Evaluate(IEnumerable<Piece> board) =>
        Evaluate(new DenseVector(board.Select(piece => piece switch
        {
            Piece.Empty => 0,
            Piece.White => 1,
            Piece.Black => -1,
            Piece.WhiteKing => _k,
            Piece.BlackKing => -_k,
            _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
        }).ToArray()));

    /// <summary>
    ///     Returns the heuristic function
    /// </summary>
    public Func<Piece[], double> GetEvaluator() => Evaluate;

    /// <summary>
    ///     Writes the network to the given file.
    /// </summary>
    public void Save(string path) =>
        File.WriteAllText(path, JSON.Serialize(NeuralNetworkData.From(this)));

    /// <summary>
    ///     Loads the network from the given file.
    /// </summary>
    public static NeuralNetwork Load(string path) =>
        JSON.Deserialize<NeuralNetworkData>(File.ReadAllText(path)).Load();

    /// <summary>
    ///     A helper class for serialization.
    /// </summary>
    public record NeuralNetworkData
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public double K = 2;

        // ReSharper disable once MemberCanBePrivate.Global
        public double[][][] Weights = null!;

        /// <summary>
        ///     Returns the neural network represented by this data.
        /// </summary>
        public NeuralNetwork Load()
        {
            return new NeuralNetwork
            {
                _k = K,
                _weights = Weights.Select(m => (Layer)DenseMatrix.OfRowArrays(m)).ToArray(),
            };
        }

        /// <summary>
        ///     Generates serializable data from the given neural network.
        /// </summary>
        public static NeuralNetworkData From(NeuralNetwork network)
        {
            return new NeuralNetworkData
            {
                K = network._k,
                Weights = network._weights.Select(x => x.ToRowArrays()).ToArray(),
            };
        }
    }
}
