using System;
using UnityEngine;

// Like a RectInt but xMax and yMax do not include width and height
// So a Rect at 0, 0 with width 1 and height 1 will have xMax and yMax of 0
// This is designed to work more like arrays which have a max index one less than the array length
public struct RectIntExclusive
{
    public int xMin, yMin;
    int _xMax, _yMax;
    int _width, _height;

    public int xMax
    {
        get
        {
            return _xMax;
        }
        set
        {
            _xMax = value;
            _width = _xMax - xMin + 1;
        }
    }

    public int yMax
    {
        get
        {
            return _yMax;
        }
        set
        {
            _yMax = value;
            _height = _yMax - yMin + 1;
        }
    }

    public int width
    {
        get
        {
            return _width;
        }
        set
        {
            _width = value;
            _xMax = xMin + _width - 1;
        }
    }

    public int height
    {
        get
        {
            return _height;
        }
        set
        {
            _height = value;
            _yMax = yMin + _height - 1;
        }
    }

    public RectIntExclusive(int x, int y, int w, int h)
    {
        xMin = x;
        yMin = y;
        _width = w;
        _height = h;
        _xMax = xMin + _width - 1;
        _yMax = yMin + _height - 1;
    }

    public void SetMinMax(int xMin, int xMax, int yMin, int yMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
    }

    public int Min(bool horizontal)
    {
        if (horizontal) return this.xMin;
        else return this.yMin;
    }

    public int Max(bool horizontal)
    {
        if (horizontal) return this.xMax;
        else return this.yMax;
    }

    public bool Contains(Vector2Int pos)
    {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y)
    {
        return x >= xMin && x <= xMax && y >= yMin && y <= yMax;
    }

    public override string ToString()
    {
        return xMin + " " + xMax + " " + yMin + " " + yMax;
    }

    public bool IsZero()
    {
        return xMin == 0 && xMax == 0 && yMin == 0 && yMax == 0;
    }
}
