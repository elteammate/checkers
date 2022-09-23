namespace Checkers.Logic;

public record Move(Color Color, Position From, Position To, Position? Eaten = null);
