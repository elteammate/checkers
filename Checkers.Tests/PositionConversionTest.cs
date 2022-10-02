using System.Diagnostics.CodeAnalysis;
using Checkers.Logic;
using NUnit.Framework;

namespace Checkers.Tests;

public class PositionConversionTest
{
    [Test]
    public void Position_RowColCtor_EqualsToRespectiveIndex()
    {
        Assert.That(new Position(7, 1).Index, Is.EqualTo(0));
        Assert.That(new Position(7, 7).Index, Is.EqualTo(3));
        Assert.That(new Position(6, 2).Index, Is.EqualTo(5));
        Assert.That(new Position(3, 3).Index, Is.EqualTo(17));
        Assert.That(new Position(2, 4).Index, Is.EqualTo(22));
        Assert.That(new Position(1, 3).Index, Is.EqualTo(25));
        Assert.That(new Position(0, 6).Index, Is.EqualTo(31));
    }

    [Test]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public void Position_RowColCtor_ThrowsIllegalArgument()
    {
        Assert.Throws<ArgumentException>(() => new Position(7, 0));
        Assert.Throws<ArgumentException>(() => new Position(6, 1));
        Assert.Throws<ArgumentException>(() => new Position(4, 7));

        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(8, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(-4, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(-4, 12));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(2, -10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(5, 11));
    }

    [Test]
    public void Position_IndexCtor_EqualsToRespectiveRowCol()
    {
        Assert.That(new Position(0).Row, Is.EqualTo(7));
        Assert.That(new Position(0).Column, Is.EqualTo(1));
        Assert.That(new Position(3).Row, Is.EqualTo(7));
        Assert.That(new Position(3).Column, Is.EqualTo(7));
        Assert.That(new Position(5).Row, Is.EqualTo(6));
        Assert.That(new Position(5).Column, Is.EqualTo(2));
        Assert.That(new Position(17).Row, Is.EqualTo(3));
        Assert.That(new Position(17).Column, Is.EqualTo(3));
        Assert.That(new Position(22).Row, Is.EqualTo(2));
        Assert.That(new Position(22).Column, Is.EqualTo(4));
        Assert.That(new Position(25).Row, Is.EqualTo(1));
        Assert.That(new Position(25).Column, Is.EqualTo(3));
        Assert.That(new Position(31).Row, Is.EqualTo(0));
        Assert.That(new Position(31).Column, Is.EqualTo(6));
    }

    [Test]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public void Position_IndexCtor_ThrowsIllegalArgument()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(32));
    }
}
