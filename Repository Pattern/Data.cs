using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using Web.Controller;
using Web.Models.Class.User;
using Web.Models.ClassModel;

namespace Web.Repository_Pattern
{
    public class Data : IData
    {
        private static string? connect;
        private IMapper _map;

        public Data(IConfiguration configuration,IMapper map )
        {
            connect = configuration.GetConnectionString("DefaultConnection") ?? "";
            _map = map;

        }

        public async Task createTable()
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {

                await connection.OpenAsync();
                string query = @"
                            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Users')
                            BEGIN
                                CREATE TABLE Users(
                                    CCCD NVARCHAR(50) PRIMARY KEY,
                                    usersName VARCHAR(50),
                                    pass VARCHAR(50),
                                    typeOfAcc bit,
                                    FOREIGN KEY (CCCD) REFERENCES Human(CCCD) ON DELETE CASCADE
                                )
                            END";//ON DELETE CASCADE
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task Delete()
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                Human_Controller.Connect = connect;
                await Human_Controller.DeleteHuman();
                connection.Open();
                string query = "DELETE FROM Users ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //command.Parameters.AddWithValue("@CCCD",cccd);
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task Delete(string usersName)
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                Human_Controller.Connect = connect;
                string humanquery = "SELECT CCCD FROM Users Where usersName=@usersName";
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(humanquery, connection))
                {
                    command.Parameters.AddWithValue("@usersName", usersName);
                    var cccd = command.ExecuteScalar();
                    if (cccd != null)
                    {
                        await Human_Controller.DeleteCCCD(cccd.ToString());
                        string query = "DELETE FROM Users WHERE usersName=@usersName";
                        using (SqlCommand commandUser = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@usersName", usersName);
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy người dùng với tên: " + cccd);
                    }

                }
                //Models_Human.DeleteCCCD(cccd);
                //if (usersName != null)
                //{

                //    return;
                //}
                //throw new Exception("Không tìm thấy người dùng với tên: " + usersName);

            }
        }

        public async Task<EnumerableRowCollection<Dictionary<string, object>>> Display(string columnName, string sortOrder)
        {

            var validColumns = new HashSet<string> { "CCCD", "Name", "Birth", "usersName", "typeOfAcc" };
            if (!validColumns.Contains(columnName))
                throw new ArgumentException($"Invalid column name: {columnName}");
            if (!string.Equals(sortOrder, "ASC", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sortOrder, "DESC", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid sort order: {sortOrder}");


            using (SqlConnection connection = new SqlConnection(connect))
            {
                await connection.OpenAsync();
                string query = $@"SELECT h.CCCD,h.Name, h.Birth, U.usersName, U.typeOfAcc
                            FROM Users U
                            LEFT JOIN Human h ON h.CCCD=U.CCCD
                            ORDER BY TRY_CAST(h.CCCD AS INT) {sortOrder},
                            {columnName} {sortOrder}";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        var result = dataTable.AsEnumerable()
                            .Select(row => dataTable.Columns.Cast<DataColumn>()
                            .ToDictionary(col => col.ColumnName, col => row[col]));
                        return result;
                    }
                }
            }
        }

        public async Task Insert(User_Model user)
        {
            var newHuman = _map.Map<Usercs>(user);// dùng user thì mới dùng đến
            await Human_Controller.InsertHuman(newHuman.Cccd, newHuman.Name, newHuman.Birthday);
            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                                IF NOT EXISTS (SELECT * FROM Users WHERE CCCD = @CCCD)
                                BEGIN
                                    INSERT INTO Users (CCCD, usersName, pass, typeOfAcc)
                                    VALUES (@CCCD, @usersName, @pass, @typeOfAcc);
                                END
                                ";
                        // Chỉ kiểm tra sự tồn tại của CCCD, tác dụng chính là tránh lỗi PRIMARY KEY
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CCCD", newHuman.Cccd);
                            //command.Parameters.AddWithValue("@name", name);
                            //command.Parameters.AddWithValue("");
                            command.Parameters.AddWithValue("@usersName", newHuman.User);
                            command.Parameters.AddWithValue("@pass", newHuman.Password);
                            command.Parameters.AddWithValue("@typeOfAcc", User_Model.TypeOfAccount);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task InsertPut(string? cccd, string? name, string? birth, string? usersName, string? pass, Boolean typeOfAcc)
        {
            await Human_Controller.InsertHuman(cccd, name, birth);
            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                            MERGE INTO Users AS target
                            USING (VALUES (@CCCD, @usersName, @pass, @typeOfAcc)) AS Source(CCCD, usersName, pass, typeOfAcc)
                            ON target.CCCD = Source.CCCD
                            WHEN MATCHED AND (target.usersName <> Source.usersName OR target.pass <> Source.pass OR target.typeOfAcc <> Source.typeOfAcc) THEN
                                UPDATE SET 
                                    usersName = Source.usersName, 
                                    pass = Source.pass, 
                                    typeOfAcc = Source.typeOfAcc
                            WHEN NOT MATCHED THEN
                                INSERT (CCCD, usersName, pass, typeOfAcc)
                                VALUES (Source.CCCD, Source.usersName, Source.pass, Source.typeOfAcc);
";
                        // Kiểm tra sự giống nhau của bản ghi cũ và mới
                        // Nếu giống thì bỏ qua
                        // Nếu không giống hoàn toàn thì cập nhật.
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CCCD", cccd);
                            //command.Parameters.AddWithValue("@name", name);
                            //command.Parameters.AddWithValue("");
                            command.Parameters.AddWithValue("@usersName", usersName);
                            command.Parameters.AddWithValue("@pass", pass);
                            command.Parameters.AddWithValue("@typeOfAcc", typeOfAcc);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<EnumerableRowCollection<Dictionary<string, object>>> Search(string usersName)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connect))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT h.CCCD,h.Name, h.Birth, U.usersName, U.typeOfAcc
                    FROM Human h
                    LEFT JOIN Users U ON U.CCCD = h.CCCD
                    WHERE U.usersName = @usersName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@usersName", usersName);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                        var result = dataTable.AsEnumerable()
                            .Select(row => dataTable.Columns.Cast<DataColumn>()
                                .ToDictionary(col => col.ColumnName, col => row[col]));

                        return result;
                    }
                }
            }
        }
    }
}
