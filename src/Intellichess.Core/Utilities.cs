namespace Intellichess.Core {
    using System;
    using System.Linq;
    using System.Windows.Input;
    using System.Windows.Media;

    public static class Utilities {
        private static readonly Random _rnd = new Random();

        public static bool IsValid(this Key key) {
            return new[] {
                Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9,
                Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4,
                Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9,
                Key.Tab, Key.Enter, Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt
            }.Contains(key);
        }

        public static int ToInt(this string source, int fallbackValue) {
            int value;

            if (int.TryParse(source, out value))
                return value;

            return fallbackValue;
        }

        public static Brush RandomColor() {
            return new SolidColorBrush(Color.FromRgb((byte) _rnd.Next(0, 255), (byte) _rnd.Next(0, 255), (byte) _rnd.Next(0, 255)));
        }

        public static Brush OuterAreaTileColor() {
            return new SolidColorBrush(Colors.Gray);
        }

        public static Brush OuterAreaBorderColor() {
            return new SolidColorBrush(Colors.DarkGray);
        }

        public static Brush InnerAreaTileColor() {
            return new SolidColorBrush(Colors.DarkGray);
        }

        public static Brush InnerAreaBorderColor() {
            return new SolidColorBrush(Colors.Gray);
        }

        public static Brush CurrentAreaTileColor() {
            return new SolidColorBrush(Colors.DarkCyan);
        }

        public static Brush CurrentAreaBorderColor() {
            return new SolidColorBrush(Colors.Cyan);
        }
    }
}