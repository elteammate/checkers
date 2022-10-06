namespace Checkers.Logic;

/// <summary>
///     Represents a move in the game.
/// </summary>
/// <param name="Color">A player who made that move</param>
/// <param name="From">A starting position of the piece</param>
/// <param name="To">A position where piece ended up</param>
/// <param name="Jumped">If not null, a position of a piece jumped over</param>
public record struct Move(Color Color, Position From, Position To, Position? Jumped = null);
