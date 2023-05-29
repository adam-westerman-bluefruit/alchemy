using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point
{
    public int x;
    public int y;
    public Point(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    public Vector2 toVector()
    {
        return new Vector2(x,y);
    }

    public void multiply(int n)
    {
        x*=n;
        y*=n;
    }

    public void add(Point p)
    {
        x += p.x;
        y += p.y;
    }

    public bool isEqual(Point p)
    {
        return (p.x == x && p.y == y);
    }

    public static Point fromVector(Vector2 v)
    {
        return new Point((int)v.x, (int)v.y);
    }

    public static Point fromVector(Vector3 v)
    {
        return new Point((int)v.x, (int)v.y);
    }

    public static Point multiply(Point p, int n)
    {
        return new Point(p.x*n, p.y*n);
    }

    public static Point add(Point p1, Point p2)
    {
        return new Point(p1.x + p2.x, p1.y +p2.y);
    }

    public static Point clone(Point p)
    {
        return new Point(p.x,p.y);
    }

    public static Point zero()
    {
        return new Point(0,0);
    }

    public static Point one()
    {
        return new Point(1,1);
    }

    public static Point up()
    {
        return new Point(0,1);
    }

    public static Point down()
    {
        return new Point(0,-1);
    }

    public static Point left()
    {
        return new Point(-1,0);
    }

    public static Point right()
    {
        return new Point(1,0);
    }
}
