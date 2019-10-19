namespace Intellichess.Core {
    using System.Collections.Generic;
    using System.Windows;

    public class PointComparer : IEqualityComparer<Point> {
        public bool Equals(Point x, Point y) {
            return (int) x.X == (int) y.X && (int) x.Y == (int) y.Y;
        }

        public int GetHashCode(Point obj) {
            return obj.X.GetHashCode() ^ obj.Y.GetHashCode();
        }
    }
}