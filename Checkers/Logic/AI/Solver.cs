using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Checkers.Logic.AI;

using HeuristicFunction = Func<Piece[], double>;

/// <summary>
///     MiniMax algorithm with alpha-beta pruning for evaluating the best move.
/// </summary>
public class Solver
{
    /// <summary>
    ///     A search depth
    /// </summary>
    private readonly int _depth = 6;

    /// <summary>
    ///     A heuristic function used to evaluate the board state.
    /// </summary>
    private readonly HeuristicFunction _heuristic;

    /// <summary>
    ///     A given board state.
    /// </summary>
    private readonly GameState _state;

    private Solver(Color player, Piece[] board, HeuristicFunction heuristic, int depth)
    {
        _state = new GameState { Player = player, Board = board };
        _heuristic = heuristic;
        _depth = depth;
    }

    /// <summary>
    ///     Creates a new instance of the solver with a given heuristic function.
    /// </summary>
    public Solver(Game game, HeuristicFunction heuristic, int depth) :
        this(game.CurrentPlayer, game.Board.ToArray(), heuristic, depth)
    { }

    /// <summary>
    ///     Runs the solver and returns the best move.
    ///     This method is slow and should be called in a separate thread.
    /// </summary>
    public Move FindBestMove()
    {
        // The minimax algorithm itself.
        double Minimax(
            int depthLeft, GameState state,
            double alpha, double beta,
            out Move? bestMove)
        {
            bestMove = null;

            if (depthLeft == 0)
                return _heuristic(state.Board);

            var moves = state.GetMoves();
            if (moves.Count == 0)
                return _heuristic(state.Board);

            var bestScore = state.Player == Color.White ? double.MinValue : double.MaxValue;

            foreach (var move in moves)
            {
                var newState = state.MakeMove(move);
                var score = Minimax(
                    depthLeft - (move.Jumped == null ? 1 : 0),
                    newState, alpha, beta, out _);

                if (state.Player == Color.White)
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }

                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }

                    beta = Math.Min(beta, bestScore);
                }

                if (beta <= alpha)
                    break;
            }

            return bestScore;
        }

        if (_state.GetMoves().Count == 0)
            throw new InvalidOperationException("No moves available.");

        if (_state.GetMoves().Count == 1)
            return _state.GetMoves().First();

        Minimax(_depth, _state, double.MinValue, double.MaxValue, out var foundMove);
        return (Move)foundMove!;
    }

    /// <summary>
    ///     Asynchronously runs the solver and returns the best move.
    ///     If searching for the move takes too long, method fallbacks
    ///     to a faster searcher with lower depth.
    /// </summary>
    public static void FindBestMoveAsyncAdaptive(Action<Move> callback, Game game,
        HeuristicFunction heuristic)
    {
        var timeStart = DateTime.Now;
        var solver4 = new Solver(game, heuristic, 4);
        var solver6 = new Solver(game, heuristic, 6);
        var solver8 = new Solver(game, heuristic, 8);

        var callbackLock = new object();
        var callbackInvoked = false;

        var task8 = new Thread(() =>
        {
            try
            {
                var move = solver8.FindBestMove();
                if (DateTime.Now - timeStart < TimeSpan.FromMilliseconds(500))
                    Thread.Sleep(500);

                lock (callbackLock)
                {
                    if (callbackInvoked) return;
                    callback(move);
                    callbackInvoked = true;
                    Console.WriteLine($"Made move of depth 8 after {DateTime.Now - timeStart}");
                }
            }
            catch (ThreadInterruptedException)
            { }
            catch (AggregateException)
            { }
        });

        var task6 = new Thread(() =>
        {
            try
            {
                var move = solver6.FindBestMove();
                Thread.Sleep(2500);
                lock (callbackLock)
                {
                    if (callbackInvoked) return;
                    callback(move);
                    callbackInvoked = true;
                    task8.Interrupt();
                    Console.WriteLine($"Made move of depth 6 after {DateTime.Now - timeStart}");
                }
            }
            catch (ThreadInterruptedException)
            { }
            catch (AggregateException)
            { }
        });

        var task4 = new Thread(() =>
        {
            var move = solver4.FindBestMove();
            Thread.Sleep(3000);
            lock (callbackLock)
            {
                if (callbackInvoked) return;
                callback(move);
                callbackInvoked = true;
                task8.Interrupt();
                task6.Interrupt();
                Console.WriteLine($"Made move of depth 4 after {DateTime.Now - timeStart}");
            }
        });

        task8.Start();
        task6.Start();
        task4.Start();
    }

    /// <summary>
    ///     GameState is a lightweight representation of the game.
    ///     It uses some optimizations to make the algorithm faster,
    ///     ignoring some of the game rules.
    /// </summary>
    private record GameState
    {
        private Position? _forcedChainPiece;
        public Piece[] Board = null!;
        public Color Player;

        public List<Move> GetMoves() => new MoveFinder(Player, Board, _forcedChainPiece).GetMoves();

        public GameState MakeMove(Move move)
        {
            var newBoard = Board.ToArray();
            newBoard[move.To.Index] = newBoard[move.From.Index];
            newBoard[move.From.Index] = Piece.Empty;

            if (move.Jumped == null)
                return new GameState
                {
                    Board = newBoard,
                    Player = Player.Opposite()
                };

            newBoard[move.Jumped.Value.Index] = Piece.Empty;
            var newMoveFinder = new MoveFinder(Player, newBoard, move.To);
            if (newMoveFinder.GetForcedMoves().Count > 0)
                return new GameState
                {
                    Player = Player,
                    Board = newBoard,
                    _forcedChainPiece = move.To
                };

            return new GameState
            {
                Board = newBoard,
                Player = Player.Opposite()
            };
        }
    }
}
