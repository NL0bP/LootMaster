using System.Data;
using System.IO;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;

namespace LootMaster
{
    public partial class MainWindow : Window
    {
        private LootDatabase db;
        private string configFilePath = "config.txt";
        private bool IsDatabaseSelected => db != null;
        private int lastSelectedIndex = -1; // Переменная для хранения последнего выбранного индекса

        public MainWindow()
        {
            InitializeComponent();
            this.MaxWidth = 800; // Устанавливаем максимальную ширину окна

            var dbPath = ReadConfig();
            if (!string.IsNullOrEmpty(dbPath) && File.Exists(dbPath))
            {
                db = new LootDatabase(dbPath);
                RefreshDataGrid();
            }
            else
            {
                MessageBox.Show("Please select the folder containing the 'compact.server.table.sqlite3' file.", "Database Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Деактивируем кнопки, если база данных не выбрана
            buttonAddLoot.IsEnabled = IsDatabaseSelected;
            buttonUpdateLoot.IsEnabled = IsDatabaseSelected;
            buttonDeleteLoot.IsEnabled = IsDatabaseSelected;
            buttonFirst.IsEnabled = IsDatabaseSelected;
            buttonPrevious.IsEnabled = IsDatabaseSelected;
            buttonNext.IsEnabled = IsDatabaseSelected;
            buttonLast.IsEnabled = IsDatabaseSelected;
        }

        private string ReadConfig()
        {
            if (File.Exists(configFilePath))
            {
                return File.ReadAllText(configFilePath).Trim();
            }
            return null;
        }

        private void WriteConfig(string dbPath)
        {
            File.WriteAllText(configFilePath, dbPath);
        }

        private void RefreshDataGrid()
        {
            var dataTable = db.ReadLoot();
            dataGridLoot.ItemsSource = dataTable.DefaultView;

            // Восстанавливаем выбранную строку
            if (lastSelectedIndex >= 0 && lastSelectedIndex < dataGridLoot.Items.Count)
            {
                dataGridLoot.SelectedIndex = lastSelectedIndex;
                dataGridLoot.ScrollIntoView(dataGridLoot.Items[lastSelectedIndex]);
            }

            // Активируем кнопки после выбора базы данных
            buttonAddLoot.IsEnabled = IsDatabaseSelected;
            buttonUpdateLoot.IsEnabled = IsDatabaseSelected;
            buttonDeleteLoot.IsEnabled = IsDatabaseSelected;
            buttonFirst.IsEnabled = IsDatabaseSelected;
            buttonPrevious.IsEnabled = IsDatabaseSelected;
            buttonNext.IsEnabled = IsDatabaseSelected;
            buttonLast.IsEnabled = IsDatabaseSelected;

            // Обновляем информацию о количестве записей
            UpdateInfoText();
        }

        private void buttonAddLoot_Click(object sender, RoutedEventArgs e)
        {
            var count = dataGridLoot.Items.Count;
            lastSelectedIndex = dataGridLoot.SelectedIndex; // Сохраняем индекс выбранной строки
            var maxId = db.GetMaxId();
            var selectedRow = dataGridLoot.SelectedItem as DataRowView;
            var addLootWindow = new AddLootWindow(db, selectedRow, maxId + 1, true);
            addLootWindow.Owner = this; // Устанавливаем текущее окно как родительское
            addLootWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner; // Устанавливаем позицию окна по центру родительского окна
            addLootWindow.ShowDialog();
            RefreshDataGrid();

            // Выбираем последнюю добавленную строку
            if (dataGridLoot.Items.Count > 0 && dataGridLoot.Items.Count > count)
            {
                dataGridLoot.SelectedIndex = dataGridLoot.Items.Count - 1;
                dataGridLoot.ScrollIntoView(dataGridLoot.Items[dataGridLoot.Items.Count - 1]);
            }
        }

        private void buttonUpdateLoot_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridLoot.SelectedItem is DataRowView selectedRow)
            {
                lastSelectedIndex = dataGridLoot.SelectedIndex; // Сохраняем индекс выбранной строки
                var updateLootWindow = new AddLootWindow(db, selectedRow, (int)selectedRow["id"], false);
                updateLootWindow.Owner = this; // Устанавливаем текущее окно как родительское
                updateLootWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner; // Устанавливаем позицию окна по центру родительского окна
                updateLootWindow.ShowDialog();
                RefreshDataGrid();
            }
            else
            {
                MessageBox.Show("Please select a row to update.", "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonDeleteLoot_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridLoot.SelectedItem is DataRowView selectedRow)
            {
                var result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var id = (int)selectedRow["id"];
                    db.DeleteLoot(id);
                    RefreshDataGrid();
                }
            }
            else
            {
                MessageBox.Show("Please select a row to delete.", "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new CommonOpenFileDialog())
            {
                folderDialog.IsFolderPicker = true;
                var result = folderDialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(folderDialog.FileName))
                {
                    var dbPath = Path.Combine(folderDialog.FileName, "compact.server.table.sqlite3");
                    if (File.Exists(dbPath))
                    {
                        db = new LootDatabase(dbPath);
                        WriteConfig(dbPath);
                        RefreshDataGrid();
                    }
                    else
                    {
                        var openFileDialog = new OpenFileDialog();
                        openFileDialog.InitialDirectory = folderDialog.FileName;
                        openFileDialog.Filter = "SQLite Database (*.sqlite3)|*.sqlite3";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            dbPath = openFileDialog.FileName;
                            db = new LootDatabase(dbPath);
                            WriteConfig(dbPath);
                            RefreshDataGrid();
                        }
                        else
                        {
                            MessageBox.Show("The file 'compact.server.table.sqlite3' does not exist in the selected folder.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void dataGridLoot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateInfoText();
        }

        private void UpdateInfoText()
        {
            var totalRecords = dataGridLoot.Items.Count;
            var selectedRecord = dataGridLoot.SelectedItem != null ? (dataGridLoot.SelectedIndex + 1).ToString() : "None";
            textBlockInfo.Text = $"Total records: {totalRecords}, Selected record: {selectedRecord}";
        }

        private void buttonFirst_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridLoot.Items.Count > 0)
            {
                dataGridLoot.SelectedIndex = 0;
                dataGridLoot.ScrollIntoView(dataGridLoot.Items[0]);
            }
        }

        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridLoot.SelectedIndex > 0)
            {
                dataGridLoot.SelectedIndex--;
                dataGridLoot.ScrollIntoView(dataGridLoot.Items[dataGridLoot.SelectedIndex]);
            }
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridLoot.SelectedIndex < dataGridLoot.Items.Count - 1)
            {
                dataGridLoot.SelectedIndex++;
                dataGridLoot.ScrollIntoView(dataGridLoot.Items[dataGridLoot.SelectedIndex]);
            }
        }

        private void buttonLast_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridLoot.Items.Count > 0)
            {
                dataGridLoot.SelectedIndex = dataGridLoot.Items.Count - 1;
                dataGridLoot.ScrollIntoView(dataGridLoot.Items[dataGridLoot.Items.Count - 1]);
            }
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            var searchText = textBoxSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                var dataTable = db.ReadLoot();
                var view = dataTable.DefaultView;

                if (int.TryParse(searchText, out int numericValue))
                {
                    // Если searchText является числом, ищем по item_id, loot_pack_id и Name
                    view.RowFilter = $"(item_id = {numericValue} OR loot_pack_id = {numericValue} OR Name LIKE '%{searchText}%')";
                }
                else
                {
                    // Если searchText не является числом, ищем только по Name
                    view.RowFilter = $"Name LIKE '%{searchText}%'";
                }

                if (view.Count > 0)
                {
                    dataGridLoot.ItemsSource = view;
                    dataGridLoot.SelectedIndex = 0;
                    dataGridLoot.ScrollIntoView(dataGridLoot.Items[0]);
                }
                else
                {
                    MessageBox.Show("No matching records found.", "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid search term.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonShowAll_Click(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
        }
    }
}