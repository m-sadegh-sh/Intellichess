namespace Intellichess.UI {
    using System.Media;
    using System.Windows;
    using System.Windows.Input;

    using Intellichess.Core;

    public partial class ParametersWindow : Window {
        private bool _parametersProvided;

        public ParametersWindow() {
            InitializeComponent();
        }

        public bool ParametersProvided {
            get { return _parametersProvided; }
        }

        public int SelectedRowsCount {
            get { return (int) rowsSelectionSlider.Value; }
        }

        public int SelectedColumnsCount {
            get { return (int) columnsSelectionSlider.Value; }
        }

        public int TargetRow {
            get { return targetRowTextBox.Text.ToInt(0); }
        }

        public int TargetColumn {
            get { return targetColumnTextBox.Text.ToInt(0); }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (!e.Key.IsValid()) {
                SystemSounds.Hand.Play();
                e.Handled = true;
            }
        }

        private void okayButton_Click(object sender, RoutedEventArgs e) {
            int value;

            if (!int.TryParse(targetRowTextBox.Text, out value) || value < 1) {
                MessageBox.Show(this, "Enter a positive numeric value for target row.", "Invalid Parameter(s)", MessageBoxButton.OK, MessageBoxImage.Hand);
                targetRowTextBox.Focus();
                targetRowTextBox.SelectAll();
                return;
            }

            if (value > rowsSelectionSlider.Value) {
                MessageBox.Show(this, "Target row is greater than rows count.", "Invalid Parameter(s)", MessageBoxButton.OK, MessageBoxImage.Hand);
                targetRowTextBox.Focus();
                targetRowTextBox.SelectAll();
                return;
            }

            if (!int.TryParse(targetColumnTextBox.Text, out value) || value < 1) {
                MessageBox.Show(this, "Enter a positive numeric value for target column.", "Invalid Parameter(s)", MessageBoxButton.OK, MessageBoxImage.Hand);
                targetColumnTextBox.Focus();
                targetColumnTextBox.SelectAll();
                return;
            }

            if (value > columnsSelectionSlider.Value) {
                MessageBox.Show(this, "Target column is greater than columns count.", "Invalid Parameter(s)", MessageBoxButton.OK, MessageBoxImage.Hand);
                targetColumnTextBox.Focus();
                targetColumnTextBox.SelectAll();
                return;
            }

            _parametersProvided = true;
            Close();
        }
    }
}