namespace Intellichess.Core {
    using System.Collections.Generic;
    using System.Windows;

    public class RouteLogData {
        public string Path { get; set; }
        public IList<Point> Route { get; set; }
    }
}