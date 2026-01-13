namespace JasmDebugger.Client.View;

/// <summary>
///     Represents a pair of double x- and y-coordinates that defines a point in a two-dimensional plane.
/// </summary>
[Serializable]
public struct Point : IEquatable<Point>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:Point" /> class with the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal position of the point.</param>
    /// <param name="y">The vertical position of the point.</param>
    public Point(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:Point" /> class with the specified point.
    /// </summary>
    /// <param name="point">The point to copy.</param>
    public Point(Point point) : this(point.X, point.Y)
    {
    }

    /// <summary>
    ///     Gets the x-coordinate of this <see cref="T:Point" />.
    /// </summary>
    public double X { get; }

    /// <summary>
    ///     Gets the y-coordinate of this <see cref="T:Point" />.
    /// </summary>
    public double Y { get; }

    /// <summary>
    ///     Compares two <see cref="T:Point" /> structures. The result specifies whether the values of the
    ///     <see cref="P:Point.X" /> and <see cref="P:Point.Y" /> properties of the two <see cref="T:Point" /> structures
    ///     are equal.
    /// </summary>
    /// <param name="left">A <see cref="T:Point" /> to compare.</param>
    /// <param name="right">A <see cref="T:Point" /> to compare.</param>
    /// <returns>
    ///     Returns <see langword="true" /> if the <see cref="P:Point.X" /> and <see cref="P:Point.Y" /> values of the left
    ///     and right
    ///     <see cref="T:Point" /> structures are equal; otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator ==(Point left, Point right) => Math.Abs(left.X - right.X) < double.Epsilon &&
                                                               Math.Abs(left.Y - right.Y) < double.Epsilon;

    /// <summary>
    ///     Determines whether the coordinates of the specified points are not equal.
    /// </summary>
    /// <param name="left">A <see cref="T:Point" /> to compare.</param>
    /// <param name="right">A <see cref="T:Point" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> to indicate the <see cref="P:Point.X" /> and <see cref="P:Point.Y" /> values of
    ///     <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator !=(Point left, Point right) => !(left == right);

    /// <summary>
    ///     Specifies whether this <see cref="T:Point" /> contains the same coordinates as the specified
    ///     <see cref="T:System.Object" />.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
    /// <returns>
    ///     This method returns <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:Point" /> and has
    ///     the same coordinates as this <see cref="T:Point" />.
    /// </returns>
    public override readonly bool Equals(object? obj) => obj is Point other && this.Equals(other);

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///     <see langword="true" /> if the current object is equal to <paramref name="other" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public readonly bool Equals(Point other) => this == other;

    /// <summary>
    ///     Returns a hash code for this <see cref="T:Point" /> structure.
    /// </summary>
    /// <returns>An integer value that specifies a hash value for this <see cref="T:Point" /> structure.</returns>
    public override readonly int GetHashCode() => HashCode.Combine(this.X.GetHashCode(), this.Y.GetHashCode());

    /// <summary>
    ///     Converts this <see cref="T:Point" /> to a human-readable string.
    /// </summary>
    /// <returns>A string that represents this <see cref="T:Point" />.</returns>
    public override readonly string ToString() => $"{{X={this.X}, Y={this.Y}}}";
}

/// <summary>
///     Represents a pair of two points (base point and offset).
/// </summary>
[Serializable]
public struct PointWithOffset : IEquatable<PointWithOffset>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:PointWithOffset" /> class with the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal position of the base point.</param>
    /// <param name="y">The vertical position of the base point.</param>
    /// <param name="offsetX">The horizontal offset.</param>
    /// <param name="offsetY">The vertical offset.</param>
    public PointWithOffset(double x, double y, double offsetX = 0.0, double offsetY = 0.0) : this(
        new Point(x, y),
        new Point(offsetX, offsetY))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:PointWithOffset" /> class with the specified coordinates.
    /// </summary>
    /// <param name="point">The position of the base point.</param>
    /// <param name="offsetX">The horizontal offset.</param>
    /// <param name="offsetY">The vertical offset.</param>
    public PointWithOffset(Point point, double offsetX = 0.0, double offsetY = 0.0) : this(
        point,
        new Point(offsetX, offsetY))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:PointWithOffset" /> class with the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal position of the base point.</param>
    /// <param name="y">The vertical position of the base point.</param>
    /// <param name="offset">The offset.</param>
    public PointWithOffset(double x, double y, Point offset) : this(new Point(x, y), offset)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:PointWithOffset" /> class with the specified coordinates.
    /// </summary>
    /// <param name="point">The position of the base point.</param>
    /// <param name="offset">The offset.</param>
    public PointWithOffset(Point point, Point offset)
    {
        this.Point = point;
        this.Offset = offset;
    }

    /// <summary>
    ///     Gets the base point.
    /// </summary>
    public Point Point { get; }

    /// <summary>
    ///     Gets the offset.
    /// </summary>
    public Point Offset { get; }

    /// <summary>
    ///     Gets the x-coordinate of this <see cref="T:PointWithOffset" /> (point + offset).
    /// </summary>
    public readonly double X => this.Point.X + this.Offset.X;

    /// <summary>Gets the y-coordinate of this <see cref="T:PointWithOffset" /> (point + offset).</summary>
    public readonly double Y => this.Point.Y + this.Offset.Y;

    /// <summary>
    ///     Compares two <see cref="T:PointWithOffset" /> structures. The result specifies whether the values of the
    ///     <see cref="P:PointWithOffset.X" /> and <see cref="P:PointWithOffset.Y" /> properties of the two
    ///     <see cref="T:PointWithOffset" /> structures
    ///     are equal.
    /// </summary>
    /// <param name="left">A <see cref="T:PointWithOffset" /> to compare.</param>
    /// <param name="right">A <see cref="T:PointWithOffset" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if the <see cref="P:PointWithOffset.X" /> and <see cref="P:PointWithOffset.Y" /> values of
    ///     the left and right
    ///     <see cref="T:PointWithOffset" /> structures are equal; otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator ==(PointWithOffset left, PointWithOffset right) =>
        Math.Abs(left.X - right.X) < double.Epsilon && Math.Abs(left.Y - right.Y) < double.Epsilon;

    /// <summary>
    ///     Determines whether the coordinates of the specified points are not equal.
    /// </summary>
    /// <param name="left">A <see cref="T:PointWithOffset" /> to compare.</param>
    /// <param name="right">A <see cref="T:PointWithOffset" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> to indicate the <see cref="P:PointWithOffset.X" /> and <see cref="P:PointWithOffset.Y" />
    ///     values of
    ///     <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator !=(PointWithOffset left, PointWithOffset right) => !(left == right);

    /// <summary>
    ///     Specifies whether this <see cref="T:PointWithOffset" /> contains the same coordinates as the specified
    ///     <see cref="T:System.Object" />.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
    /// <returns>
    ///     This method returns <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:PointWithOffset" /> and
    ///     has
    ///     the same coordinates as this <see cref="T:PointWithOffset" />.
    /// </returns>
    public override readonly bool Equals(object? obj) => obj is PointWithOffset other && this.Equals(other);

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///     <see langword="true" /> if the current object is equal to <paramref name="other" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public readonly bool Equals(PointWithOffset other) => this == other;

    /// <summary>
    ///     Returns a hash code for this <see cref="T:PointWithOffset" /> structure.
    /// </summary>
    /// <returns>An integer value that specifies a hash value for this <see cref="T:PointWithOffset" /> structure.</returns>
    public override int GetHashCode() => HashCode.Combine(this.Point.GetHashCode(), this.Offset.GetHashCode());

    /// <summary>
    ///     Converts this <see cref="T:PointWithOffset" /> to a human-readable string.
    /// </summary>
    /// <returns>A string that represents this <see cref="T:PointWithOffset" />.</returns>
    public override readonly string ToString() => $"{{X={this.X}, Y={this.Y}}}";
}