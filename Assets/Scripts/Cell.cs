
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace conway.lib
{
    public class CellCoords
    {
        public int x { get; private set; }
        public int y { get; private set; }

        public CellCoords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class CellCoordsComparer : IEqualityComparer<CellCoords>
    {
        public bool Equals(CellCoords first, CellCoords second)
        {
            if (first == null && second == null)
                return true;
            else if (first == null || second == null)
                return false;
            else if (first.x == second.x && first.y == second.y)
                return true;
            else
                return false;
        }

        public int GetHashCode(CellCoords obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                //new hash introduced, see https://stackoverflow.com/a/13871379
                var x = obj.x;
                var y = obj.y;
                Int32 xHash = (Int32)(x >= 0 ? 2 * x : -2 * x - 1);
                Int32 yHash = (Int32)(y >= 0 ? 2 * y : -2 * y - 1);
                Int32 Hash = ((xHash >= yHash ? xHash * xHash + xHash + yHash : xHash + yHash * yHash) / 2);
                return x < 0 && y < 0 || x >= 0 && y >= 0 ? Hash : -Hash - 1;
                // int hash = (int)2166136261;
                // hash = (hash * 16777619) ^ obj.x.GetHashCode();
                // hash = (hash * 16777619) ^ obj.y.GetHashCode();
                // return hash;
            }
        }
    }

    public class Cell
    {
        public Cell()
        {
        }
    }
}