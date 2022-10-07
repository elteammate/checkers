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

public class NeuralNetwork
{
    private static readonly int[] Layers = { 32, 39, 15, 1 };
    private static readonly int WightsCount = Layers.Zip(Layers.Skip(1), (a, b) => a * b).Sum();
    private static readonly double Tau = 1 / Math.Sqrt(2 * Math.Sqrt(WightsCount));

    private Layer[] _sigma;
    private Layer[] _weights;
    private double _k = 2;

    public NeuralNetwork()
    {
        _weights = new Layer[Layers.Length - 1];
        _sigma = new Layer[Layers.Length - 1];

        var normal = new Normal(0, 1);
        for (var i = 0; i < Layers.Length - 1; i++)
        {
            _weights[i] = DenseMatrix.CreateRandom(Layers[i + 1], Layers[i] + 1, normal);
            _sigma[i] = DenseMatrix.Create(Layers[i + 1], Layers[i] + 1, 0.05);
        }
    }

    public NeuralNetwork Mutate()
    {
        var newSigma = new Layer[_sigma.Length];
        var newWeights = new Layer[_weights.Length];

        var normal = new Normal(0, 1);

        for (var i = 0; i < _weights.Length; i++)
        {
            var random = DenseMatrix.CreateRandom(
                _weights[i].RowCount,
                _weights[i].ColumnCount,
                normal);

            newSigma[i] = _sigma[i].PointwiseMultiply((random * Tau).PointwiseExp());

            random = DenseMatrix.CreateRandom(
                _weights[i].RowCount,
                _weights[i].ColumnCount,
                normal);

            newWeights[i] = _weights[i] + newSigma[i].PointwiseMultiply(random);
        }

        var newK = _k * Math.Exp(1 / Math.Sqrt(2) * normal.Sample());
        if (newK < 1.2) newK = 1.2;
        if (newK > 3) newK = 3;

        return new NeuralNetwork
        {
            _sigma = newSigma,
            _weights = newWeights,
            _k = newK
        };
    }

    private double Evaluate(Vector<double> input)
    {
        var output = input;

        foreach (var layer in _weights)
        {
            output = layer * new DenseVector(output.Append(1).ToArray());
            output = output.PointwiseTanh();
        }

        return output[0];
    }

    public double Evaluate(IEnumerable<Piece> board) =>
        Evaluate(new DenseVector(board.Select(piece => piece switch
        {
            Piece.Empty => 0,
            Piece.White => 1,
            Piece.Black => -1,
            Piece.WhiteKing => _k,
            Piece.BlackKing => -_k,
            _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
        }).ToArray()));

    public Func<Piece[], double> GetEvaluator() => Evaluate;

    public record NeuralNetworkData
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public double K = 2;

        // ReSharper disable once MemberCanBePrivate.Global
        public double[][][] Weights = null!;

        // ReSharper disable once MemberCanBePrivate.Global
        public double[][][] Sigma = null!;

        public NeuralNetwork Load()
        {
            return new NeuralNetwork
            {
                _k = K,
                _weights = Weights.Select(m => (Layer)DenseMatrix.OfRowArrays(m)).ToArray(),
                _sigma = Sigma.Select(m => (Layer)DenseMatrix.OfRowArrays(m)).ToArray()
            };
        }

        public static NeuralNetworkData From(NeuralNetwork network)
        {
            return new NeuralNetworkData
            {
                K = network._k,
                Weights = network._weights.Select(x => x.ToRowArrays()).ToArray(),
                Sigma = network._sigma.Select(x => x.ToRowArrays()).ToArray()
            };
        }
    }

    public void Save(string path) =>
        File.WriteAllText(path, JSON.Serialize(NeuralNetworkData.From(this)));

    public static NeuralNetwork Load(string path) =>
        JSON.Deserialize<NeuralNetworkData>(File.ReadAllText(path)).Load();
}
