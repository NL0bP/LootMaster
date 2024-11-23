using System;
using System.Data;
using System.Windows;

namespace LootMaster
{
    public partial class AddLootWindow : Window
    {
        private LootDatabase db;
        private DataRowView selectedRow;
        private int newId;

        public AddLootWindow(LootDatabase db, DataRowView selectedRow, int newId, bool asAdd)
        {
            InitializeComponent();
            this.db = db;
            this.selectedRow = selectedRow;
            this.newId = newId;

            textBoxId.Text = newId.ToString();

            if (selectedRow != null)
            {
                textBoxId.Text = selectedRow["id"].ToString();
                textBoxLootPackId.Text = selectedRow["loot_pack_id"].ToString();
                textBoxItemId.Text = selectedRow["item_id"].ToString();
                textBoxDropRate.Text = selectedRow["drop_rate"].ToString();
                textBoxMinAmount.Text = selectedRow["min_amount"].ToString();
                textBoxMaxAmount.Text = selectedRow["max_amount"].ToString();
                textBoxGradeId.Text = selectedRow["grade_id"].ToString();
                textBoxGroup.Text = selectedRow["group"].ToString();

                var alwaysDropValue = selectedRow["always_drop"].ToString().ToLower();
                checkBoxAlwaysDrop.IsChecked = alwaysDropValue == "t" || alwaysDropValue == "true";
            }

            buttonAdd.IsEnabled = asAdd;
            buttonUpdate.IsEnabled = !asAdd;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxLootPackId.Text) ||
                string.IsNullOrWhiteSpace(textBoxItemId.Text) ||
                string.IsNullOrWhiteSpace(textBoxDropRate.Text) ||
                string.IsNullOrWhiteSpace(textBoxMinAmount.Text) ||
                string.IsNullOrWhiteSpace(textBoxMaxAmount.Text) ||
                string.IsNullOrWhiteSpace(textBoxGradeId.Text) ||
                string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("All fields must be filled.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var id = newId;
            var lootPackId = int.Parse(textBoxLootPackId.Text);
            var itemId = int.Parse(textBoxItemId.Text);
            var dropRate = int.Parse(textBoxDropRate.Text);
            var minAmount = int.Parse(textBoxMinAmount.Text);
            var maxAmount = int.Parse(textBoxMaxAmount.Text);
            var gradeId = int.Parse(textBoxGradeId.Text);
            var group = int.Parse(textBoxGroup.Text);
            var alwaysDrop = checkBoxAlwaysDrop.IsChecked ?? false;

            db.AddLoot(id, group, itemId, dropRate, minAmount, maxAmount, lootPackId, gradeId, alwaysDrop);
            this.Close();
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRow == null)
            {
                MessageBox.Show("No row selected for update.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxLootPackId.Text) ||
                string.IsNullOrWhiteSpace(textBoxItemId.Text) ||
                string.IsNullOrWhiteSpace(textBoxDropRate.Text) ||
                string.IsNullOrWhiteSpace(textBoxMinAmount.Text) ||
                string.IsNullOrWhiteSpace(textBoxMaxAmount.Text) ||
                string.IsNullOrWhiteSpace(textBoxGradeId.Text) ||
                string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("All fields must be filled.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var id = int.Parse(textBoxId.Text);
            var lootPackId = int.Parse(textBoxLootPackId.Text);
            var itemId = int.Parse(textBoxItemId.Text);
            var dropRate = int.Parse(textBoxDropRate.Text);
            var minAmount = int.Parse(textBoxMinAmount.Text);
            var maxAmount = int.Parse(textBoxMaxAmount.Text);
            var gradeId = int.Parse(textBoxGradeId.Text);
            var group = int.Parse(textBoxGroup.Text);
            var alwaysDrop = checkBoxAlwaysDrop.IsChecked ?? false;

            db.UpdateLoot(id, group, itemId, dropRate, minAmount, maxAmount, lootPackId, gradeId, alwaysDrop);
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}