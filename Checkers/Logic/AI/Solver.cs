using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers.Logic.AI;

using HeuristicFunction = Func<Piece[], float>;

public class Solver
{
    private const int Depth = 8;
    private readonly HeuristicFunction _heuristic;
    private readonly GameState _state;

    private record GameState
    {
        public Color Player;
        public Piece[] Board = null!;
        private Position? _forcedChainPiece;

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
                    Player = Player.Opposite(),
                };

            newBoard[move.Jumped.Value.Index] = Piece.Empty;
            var newMoveFinder = new MoveFinder(Player, newBoard, move.To);
            if (newMoveFinder.GetForcedMoves().Count > 0)
            {
                return new GameState
                {
                    Player = Player,
                    Board = newBoard,
                    _forcedChainPiece = move.To
                };
            }

            return new GameState
            {
                Board = newBoard,
                Player = Player.Opposite(),
            };
        }
    }

    public Solver(Color player, Piece[] board, HeuristicFunction heuristic)
    {
        _state = new GameState { Player = player, Board = board };
        _heuristic = heuristic;
    }

    public Solver(Game game, HeuristicFunction heuristic) :
        this(game.CurrentPlayer, game.Board.ToArray(), heuristic)
    { }

    public Move FindBestMove()
    {
        float Minimax(int depthLeft, GameState state, float alpha, float beta, out Move? bestMove)
        {
            bestMove = null;

            if (depthLeft == 0)
                return _heuristic(state.Board);

            var moves = state.GetMoves();
            if (moves.Count == 0)
                return _heuristic(state.Board);

            var bestScore = state.Player == Color.White ? float.MinValue : float.MaxValue;

            foreach (var move in moves)
            {
                var newState = state.MakeMove(move);
                var score = Minimax(depthLeft - 1, newState, alpha, beta, out _);

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

        Minimax(Depth, _state, float.MinValue, float.MaxValue, out var foundMove);
        return (Move)foundMove!;
    }
}
