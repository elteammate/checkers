using System;
using System.Collections.Generic;
using System.Linq;

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
    private const int Depth = 6;

    /// <summary>
    ///     A heuristic function used to evaluate the board state.
    /// </summary>
    private readonly HeuristicFunction _heuristic;

    /// <summary>
    ///     A given board state.
    /// </summary>
    private readonly GameState _state;

    private Solver(Color player, Piece[] board, HeuristicFunction heuristic)
    {
        _state = new GameState { Player = player, Board = board };
        _heuristic = heuristic;
    }

    /// <summary>
    ///     Creates a new instance of the solver with a given heuristic function.
    /// </summary>
    public Solver(Game game, HeuristicFunction heuristic) :
        this(game.CurrentPlayer, game.Board.ToArray(), heuristic)
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

        Minimax(Depth, _state, double.MinValue, double.MaxValue, out var foundMove);
        return (Move)foundMove!;
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
