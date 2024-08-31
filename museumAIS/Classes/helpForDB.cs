using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace museumAIS
{
    public class helpForDB
    {
        // Строка подключения к базе данных MySQL
        private static string connectionStr = "host='localhost';database='museum';uid='root';pwd='root';charset='utf8'";

        // Метод для выполнения SQL-запроса SELECT и получения данных в DataTable
        public DataTable selectData(string query)
        {
            MySqlConnection con = new MySqlConnection(connectionStr);
            DataTable table = new DataTable();
            try
            {
                // Открытие соединения с базой данных
                con.Open();
                MySqlCommand command = new MySqlCommand(query, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                // Заполнение DataTable результатами выполнения запроса
                adapter.Fill(table);
            }
            catch (Exception)
            {
                table = null;
            }
            finally
            {
                // Закрытие соединения
                con.Close();
            }
            return table;
        }

        // Метод для выполнения SQL-запроса, изменяющего данные в базе (INSERT, UPDATE, DELETE)
        public bool editDBData(string query)
        {
            MySqlConnection con = new MySqlConnection(connectionStr);
            bool isOk = true;
            try
            {
                // Открытие соединения с базой данных
                con.Open();
                MySqlCommand command = new MySqlCommand(query, con);

                // Выполнение запроса
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                isOk = false;
            }
            finally
            {
                // Закрытие соединения
                con.Close();
            }
            return isOk;
        }

        // Метод для выполнения SQL-запроса, возвращающего одно значение (например, COUNT, MAX)
        public string executSclrQuey(string query)
        {
            MySqlConnection con = new MySqlConnection(connectionStr);
            try
            {
                // Открытие соединения с базой данных
                con.Open();
                MySqlCommand command = new MySqlCommand(query, con);
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command);

                // Возвращение результата запроса в виде строки
                return command.ExecuteScalar().ToString();
            }
            catch (Exception)
            {
                return "error";
            }
            finally
            {
                // Закрытие соединения
                con.Close();
            }
        }

        // Метод для выполнения транзакции по добавлению экскурсии и связанных с ней залов
        public bool transactionExcursion(string query, List<string> hallIDList)
        {
            bool isOk = true;
            MySqlConnection con = new MySqlConnection(connectionStr);
            MySqlTransaction transaction = null;

            try
            {
                // Открытие соединения
                con.Open();

                // Начало транзакции
                transaction = con.BeginTransaction();

                // Выполнение основного запроса (создание экскурсии)
                MySqlCommand command = new MySqlCommand(query, con);
                command.ExecuteNonQuery();

                // Получение ID созданной экскурсии
                int orderId = 0;
                if (query.StartsWith("UPDATE"))
                {
                    orderId = takeID(query);
                }
                else
                {
                    orderId = (int)command.LastInsertedId;
                }

                // Добавление залов в таблицу excursion_halls
                foreach (string hall_id in hallIDList)
                {
                    query = "INSERT INTO excursion_halls (id_excursion, num_hall) VALUES " +
                        "(" + orderId + ", " + hall_id + ")";
                    command = new MySqlCommand(query, con);
                    command.ExecuteNonQuery();
                }

                // Завершение транзакции
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Откат транзакции в случае ошибки
                transaction.Rollback();
                isOk = false;
            }
            finally
            {
                // Закрытие соединения
                con.Close();
            }
            return isOk;
        }

        // Метод для выполнения транзакции по добавлению экспоната и связанных с ним материалов
        public bool transactionEcsponat(string query, List<string> materialIDList)
        {
            bool isOk = true;
            MySqlConnection con = new MySqlConnection(connectionStr);
            MySqlTransaction transaction = null;

            try
            {
                // Открытие соединения
                con.Open();

                // Начало транзакции
                transaction = con.BeginTransaction();

                // Выполнение основного запроса (создание экспоната)
                MySqlCommand command = new MySqlCommand(query, con);
                command.ExecuteNonQuery();

                // Получение ID созданного экспоната
                int orderId = 0;
                if (query.StartsWith("UPDATE"))
                {
                    orderId = takeID(query);
                }
                else
                {
                    orderId = (int)command.LastInsertedId;
                }

                // Добавление материалов в таблицу material_ecsponat
                foreach (string material_name in materialIDList)
                {
                    query = "INSERT INTO material_ecsponat (ecsponatM_id, name_material) VALUES " +
                        "(" + orderId + ", '" + material_name + "')";
                    command = new MySqlCommand(query, con);
                    command.ExecuteNonQuery();
                }

                // Завершение транзакции
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Откат транзакции в случае ошибки
                transaction.Rollback();
                isOk = false;
            }
            finally
            {
                // Закрытие соединения
                con.Close();
            }
            return isOk;
        }

        public bool changeStatusTransaction(string query01, string query02, string query03, string query04)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionStr))
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();

                MySqlCommand command = conn.CreateCommand();
                command.Transaction = transaction;

                try
                {
                    if (query01 != "")
                    {
                        command.CommandText = query01;
                        command.ExecuteNonQuery();
                    }

                    if (query02 != "")
                    {
                        command.CommandText = query02;
                        command.ExecuteNonQuery();
                    }

                    if (query03 != "")
                    {
                        command.CommandText = query03;
                        command.ExecuteNonQuery();
                    }

                    if (query04 != "")
                    {
                        command.CommandText = query04;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return false;
                }

                return true;
            }
        }

        // Метод для получения ID из SQL-запроса
        private int takeID(string query)
        {
            string[] spl_query = query.Split('=');
            int id = Convert.ToInt32(spl_query[spl_query.Length - 1].Replace(" ", ""));
            return id;
        }

        // Метод для экспорта данных из таблицы базы данных в CSV файл
        public bool exportData(string tableName, string path)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionStr))
            {
                try
                {
                    connection.Open();

                    // Получение имен столбцов таблицы
                    List<string> columns = new List<string>();
                    using (MySqlCommand columnCommand = new MySqlCommand("SHOW columns FROM " + tableName, connection))
                    {
                        using (MySqlDataReader columnReader = columnCommand.ExecuteReader())
                        {
                            while (columnReader.Read())
                            {
                                columns.Add(columnReader[0].ToString());
                            }
                        }
                    }

                    // Удаление столбца id
                    //columns.RemoveAt(0);

                    // Создание команды для выборки данных без id
                    string commandText = "SELECT " + string.Join(",", columns) + " FROM " + tableName;
                    using (MySqlCommand command = new MySqlCommand(commandText, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            string filePath = Path.Combine(path, DateTime.Now.ToString("dd-MMM HH-mm-ss") + "_" + tableName + ".csv");

                            using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                // Запись заголовков столбцов в CSV файл
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    writer.Write(reader.GetName(i));
                                    if (i < reader.FieldCount - 1)
                                    {
                                        writer.Write(";");
                                    }
                                }
                                writer.WriteLine();

                                // Запись данных в CSV файл
                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        writer.Write(reader.GetValue(i).ToString());
                                        if (i < reader.FieldCount - 1)
                                        {
                                            writer.Write(";");
                                        }
                                    }
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    // Закрытие соединения
                    connection.Close();
                }
            }

            return true;
        }

        // Метод для импорта данных из CSV файла в таблицу базы данных
        public bool importData(string csvFilePath, string tableName)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionStr))
                {
                    connection.Open();

                    // Удалить все строки из таблицы перед импортом
                    using (MySqlCommand deleteCommand = new MySqlCommand($"DELETE FROM {tableName}", connection))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }

                    // Прочитать CSV файл
                    var csvData = File.ReadAllLines(csvFilePath);

                    if (csvData.Length == 0)
                    {
                        return true;
                    }

                    // Получить заголовки столбцов
                    var headers = csvData[0].Split(';');

                    // Создать команду для вставки данных
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = $"INSERT INTO {tableName} ({string.Join(", ", headers)}) VALUES (@{string.Join(", @", headers)})";

                    // Добавить параметры к команде
                    foreach (var header in headers)
                    {
                        command.Parameters.Add(new MySqlParameter($"@{header}", MySqlDbType.VarChar));
                    }

                    // Заполнить таблицу данными из CSV файла
                    for (int i = 1; i < csvData.Length; i++)
                    {
                        var values = csvData[i].Split(';');

                        for (int j = 0; j < headers.Length; j++)
                        {
                            string val = values[j].ToString();
                            DateTime dateVal;
                            if (DateTime.TryParse(val, out dateVal))
                            {
                                if (val.Length > 5)
                                {
                                    val = dateVal.ToString("yyyy-MM-dd");// Преобразование в формат MySQL 
                                }
                                
                            }
                            command.Parameters[$"@{headers[j]}"].Value = val;
                        }

                        // Выполнение команды вставки данных
                        command.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Метод для создания резервной копии базы данных
        public bool createBackup()
        {
            string currentDirectory = Directory.GetCurrentDirectory() + "\\backup";
            string backupFile = Path.Combine(currentDirectory, $"backup_{DateTime.Now.ToString("dd-MMM HH-mm-ss")}.sql");

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            conn.Open();
                            cmd.Connection = conn;

                            // Экспорт базы данных в файл
                            mb.ExportToFile(backupFile);
                            conn.Close();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Метод для восстановления базы данных из резервной копии
        public bool importBackup(string filePath)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionStr))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Чтение SQL-команд из файла
                            string sqlCommandText = File.ReadAllText(filePath);
                            MySqlCommand command = new MySqlCommand(sqlCommandText, connection, transaction);

                            // Выполнение SQL-команд
                            command.ExecuteNonQuery();

                            // Подтверждение транзакции
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            // Откат транзакции в случае ошибки
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
