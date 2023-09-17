using System;
using System.Linq;

namespace DOL.GS.Geometry;

public class LinePath
{
    private Coordinate[] wayPoints = Array.Empty<Coordinate>();
    private int indexCounter = 0;

    public static LinePath Create(Coordinate[] wayPoints)
        => new LinePath() { wayPoints = wayPoints };

    public Coordinate Start => wayPoints.Length == 0 ? Coordinate.Nowhere : wayPoints.First();

    public Coordinate End => wayPoints.Length == 0 ? Coordinate.Nowhere : wayPoints.Last();

    public void SelectNextWayPoint() => indexCounter++;

    public Coordinate CurrentWayPoint => (indexCounter < PointCount - 1) ? wayPoints[indexCounter] : Coordinate.Nowhere;

    public int PointCount => wayPoints.Length;
}