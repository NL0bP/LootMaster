using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;

public class LootDatabase
{
    private string connectionString;

    public LootDatabase(string dbPath)
    {
        connectionString = $"Data Source={dbPath};Version=3;";
    }

    public DataTable ReadLoot()
    {
        var items = LoadItemsData();

        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var sql = "SELECT * FROM loots;";
            var command = new SQLiteCommand(sql, conn);
            using (var reader = command.ExecuteReader())
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("id", typeof(int));
                dataTable.Columns.Add("loot_pack_id", typeof(int));
                dataTable.Columns.Add("item_id", typeof(int));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("drop_rate", typeof(int));
                dataTable.Columns.Add("min_amount", typeof(int));
                dataTable.Columns.Add("max_amount", typeof(int));
                dataTable.Columns.Add("always_drop", typeof(string));
                dataTable.Columns.Add("grade_id", typeof(int));
                dataTable.Columns.Add("group", typeof(int));

                while (reader.Read())
                {
                    var row = dataTable.NewRow();
                    row["id"] = reader["id"];
                    row["loot_pack_id"] = reader["loot_pack_id"];
                    row["item_id"] = reader["item_id"];
                    row["Name"] = items.ContainsKey((int)reader["item_id"]) ? items[(int)reader["item_id"]] : "Unknown";
                    row["drop_rate"] = reader["drop_rate"];
                    row["min_amount"] = reader["min_amount"];
                    row["max_amount"] = reader["max_amount"];
                    row["always_drop"] = reader["always_drop"];
                    row["grade_id"] = reader["grade_id"];
                    row["group"] = reader["group"];
                    dataTable.Rows.Add(row);
                }
                return dataTable;
            }
        }
    }

    public int GetMaxId()
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var sql = "SELECT MAX(id) FROM loots;";
            var command = new SQLiteCommand(sql, conn);
            var result = command.ExecuteScalar();
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
    }

    public void AddLoot(int id, int group, int itemId, int dropRate, int minAmount, int maxAmount, int lootPackId, int gradeId, bool alwaysDrop)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var sql = @"
                INSERT INTO loots (id, [group], item_id, drop_rate, min_amount, max_amount, loot_pack_id, grade_id, always_drop)
                VALUES (@id, @group, @itemId, @dropRate, @minAmount, @maxAmount, @lootPackId, @gradeId, @alwaysDrop);
            ";
            var command = new SQLiteCommand(sql, conn);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@group", group);
            command.Parameters.AddWithValue("@itemId", itemId);
            command.Parameters.AddWithValue("@dropRate", dropRate);
            command.Parameters.AddWithValue("@minAmount", minAmount);
            command.Parameters.AddWithValue("@maxAmount", maxAmount);
            command.Parameters.AddWithValue("@lootPackId", lootPackId);
            command.Parameters.AddWithValue("@gradeId", gradeId);
            command.Parameters.AddWithValue("@alwaysDrop", alwaysDrop ? "t" : "f");
            command.ExecuteNonQuery();
        }
    }

    public void UpdateLoot(int id, int group, int itemId, int dropRate, int minAmount, int maxAmount, int lootPackId, int gradeId, bool alwaysDrop)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var sql = @"
                UPDATE loots
                SET [group] = @group, item_id = @itemId, drop_rate = @dropRate, min_amount = @minAmount, max_amount = @maxAmount, loot_pack_id = @lootPackId, grade_id = @gradeId, always_drop = @alwaysDrop
                WHERE id = @id;
            ";
            var command = new SQLiteCommand(sql, conn);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@group", group);
            command.Parameters.AddWithValue("@itemId", itemId);
            command.Parameters.AddWithValue("@dropRate", dropRate);
            command.Parameters.AddWithValue("@minAmount", minAmount);
            command.Parameters.AddWithValue("@maxAmount", maxAmount);
            command.Parameters.AddWithValue("@lootPackId", lootPackId);
            command.Parameters.AddWithValue("@gradeId", gradeId);
            command.Parameters.AddWithValue("@alwaysDrop", alwaysDrop ? "t" : "f");
            command.ExecuteNonQuery();
        }
    }

    public void DeleteLoot(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var sql = @"
                DELETE FROM loots
                WHERE id = @id;
            ";
            var command = new SQLiteCommand(sql, conn);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }
    }

    private Dictionary<int, string> LoadItemsData()
    {
        var items = new Dictionary<int, string>();
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "items.txt");
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(';');
                if (parts.Length == 2 && int.TryParse(parts[0], out int itemId))
                {
                    items[itemId] = parts[1];
                }
            }
        }
        return items;
    }
}