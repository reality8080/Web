using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Web.Controller;
using Web.Models.Class;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class User_Controllers : ControllerBase
    {

        private readonly IConfiguration _configuration;
        static string? connect;
        public User_Controllers(IConfiguration configuration)
        {
            _configuration = configuration;
            connect = _configuration.GetConnectionString("DefaultConnection") ?? "";
            Models_Human.Connect = connect;
            Models_Human.createTable();
            createTable();
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var result = Display("Name", "ASC");
                return Ok(new { result, Success = true, });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }
        [HttpGet("Take/{usersName}")]
        public IActionResult getUser(string usersName)
        {
            try
            {
                var result = Search(usersName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("SortCol/{Column}")]
        public IActionResult getSort(string Column, string Sort = "ASC")
        {
            try
            {
                var result = Display(Column, Sort);
                return Ok(new { result, Success = true, });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult InsertPost(Usercs user)
        {
            try
            {
                InsertPost(user.Cccd, user.Name, user.Birthday, user.User, user.Password, Usercs.TypeOfAccount);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, });
            //return Ok(new { Success = true, });
        }
        [HttpPut]
        public IActionResult InsertPut(Usercs user)
        {
            try
            {
                InsertPut(user.Cccd, user.Name, user.Birthday, user.User, user.Password, Usercs.TypeOfAccount);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, });
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            try
            {
                DeleteHuman();
                //return Ok(new { message = "All data deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi Hệ thống: {ex.Message}");
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, message = "All data deleted successfully" });
        }
        [HttpDelete("Delete/{Users}")]
        public IActionResult DeleteUser(string Users)
        {
            try
            {
                DeleteUsers(Users);
                //return Ok(new { Success=true,});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi Hệ thống: {ex.Message}");
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, });
        }
        public static void createTable()
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {

                connection.Open();
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

        private void InsertPost(string? cccd, string? name, string? birth, string? usersName, string? pass, Boolean typeOfAcc)
        {
            Models_Human.InsertHman(cccd, name, birth);
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

        private void InsertPut(string? cccd, string? name, string? birth, string? usersName, string? pass, Boolean typeOfAcc)
        {
            Models_Human.InsertHman(cccd, name, birth);
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

        private EnumerableRowCollection<Dictionary<string, object>> Display(string columnName, string sortOrder)
        {

            var validColumns = new HashSet<string> { "CCCD", "Name", "Birth", "usersName", "typeOfAcc" };
            if (!validColumns.Contains(columnName))
                throw new ArgumentException($"Invalid column name: {columnName}");
            if (!string.Equals(sortOrder, "ASC", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sortOrder, "DESC", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid sort order: {sortOrder}");


            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();
                string query = $@"SELECT h.CCCD,h.Name, h.Birth, U.usersName, U.typeOfAcc
                            FROM Users U
                            LEFT JOIN Human h ON h.CCCD=U.CCCD
                            ORDER BY {columnName} {sortOrder}";
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
        private void DeleteHuman()
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                Models_Human.Connect = connect;
                Models_Human.DeleteHuman();
                connection.Open();
                string query = "DELETE FROM Users ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //command.Parameters.AddWithValue("@CCCD",cccd);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteUsers(string usersName)
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                Models_Human.Connect = connect;
                string humanquery = "SELECT CCCD FROM Users Where usersName=@usersName";
                connection.Open();
                using (SqlCommand command = new SqlCommand(humanquery, connection))
                {
                    command.Parameters.AddWithValue("@usersName", usersName);
                    var cccd = command.ExecuteScalar();
                    if (cccd != null)
                    {
                        Models_Human.DeleteCCCD(cccd.ToString());
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

        private EnumerableRowCollection<Dictionary<string, object>> Search(string usersName)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();

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

