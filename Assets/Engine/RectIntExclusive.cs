using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Like a RectInt but xMax and yMax do not include width and height
// So a Rect at 0, 0 with width 1 and height 1 will have xMax and yMax of 0
// This is designed to work more like arrays which have a max index one less than the array length
public class RectIntExclusive
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

    public RectIntExclusive() { }

    public RectIntExclusive(int x, int y, int w, int h)
    {
        xMin = x;
        width = w;
        yMin = y;
        height = h;
    }

    public bool Contains(Vector2Int pos)
    {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y)
    {
        return x >= xMin && x <= xMax && y >= yMin && y <= yMax;
    }
}
