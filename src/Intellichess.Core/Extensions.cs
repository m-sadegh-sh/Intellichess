namespace Intellichess.Core {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    public static class Extensions {
        public static int ComputeWeight(this Point value) {
            return (int) (value.X + value.Y);
        }

        public static bool IsBetween(this Point value, Point start, Point end) {
            return value.X >= start.X && value.Y >= start.Y &&
                   value.X <= end.X && value.Y <= end.Y;
        }

        public static GridData GetData(this UIElementCollection childs, Point target) {
            return childs.Cast<UIElement>().OfType<GridData>().First(c => c.Position == target);
        }

        public static string ToRoutePath(this IList<Point> route) {
            if (route == null || route.Count == 0)
                return "No route.";

            var path = new StringBuilder();
            foreach (var point in route)
                path.AppendFormat("[{0}, {1}], ", point.X, point.Y);

            return path.ToString().Trim(' ', ',');
        }
    }
}