using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using Web.Models.Class;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class Human_Controller : ControllerBase// Tạo phương thức thếm sửa xóa của đối tượng
    {
        private readonly IConfiguration _configuration;
        private static string? connect;

        public static string? Connect { get => connect; set => connect = value; }

        public Human_Controller(IConfiguration configuration)
        {
            _configuration = configuration;
            connect = _configuration.GetConnectionString("DefaultConnection") ?? "";
            Task.Run(async ()=> await createTable()).Wait();
        }
        //[HttpGet]
        //public IActionResult GetHuman()
        //{
        //    //return Ok();
        //    //string connect = _configuration.GetConnectionString("DefaultConnection");
        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connect))
        //        {
        //            connection.Open();
        //            string query = @"SELECT * FROM Human";
        //            using (SqlDataAdapter adapter = new SqlDataAdapter(query, connect))
        //            {
        //                DataTable dataTable = new DataTable();
        //                adapter.Fill(dataTable);
        //                // tuần tự hóa, chuyển từ dataTable về JSON
        //                var result= dataTable.AsEnumerable()
        //                    .Select(row=>dataTable.Columns.Cast<DataColumn>()
        //                    .ToDictionary(col=>col.ColumnName, col => row[col]));
        //                return Ok(result);
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"{ex.Message}");
        //    }
        //}
        //[HttpPost]
        //public IActionResult Add(Human human)
        //{
        //    try
        //    {   
        //        createTable();
        //        InsertHman(human.Cccd, human.Name, human.Birthday);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
        //    }
        //    return Ok(new
        //    {
        //        Success = true,
        //    });
        //}

        public static async Task createTable()
        {

            using (SqlConnection connection = new SqlConnection(connect))
            {
                await connection.OpenAsync();
                string query = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Human')
                BEGIN
                    CREATE TABLE HUMAN(
                        CCCD NVARCHAR(50) PRIMARY KEY,
                        Name NVARCHAR(100),
                        Birth VARCHAR(20)
                    )
                END
";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }


        }
        public static async Task InsertHuman(string? cccd, string? name, string? birthday)
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    string queryHuman = @"
                        MERGE INTO Human AS target
                        USING(VALUES (@CCCD,@Name,@Birth)) AS Source (CCCD,Name,Birth)
                        ON target.CCCD = source.CCCD
                        WHEN MATCHED THEN
                            UPDATE SET CCCD=source.CCCD, Name = source.Name, Birth = source.Birth
                        WHEN NOT MATCHED THEN
                            INSERT (CCCD, Name, Birth) 
                            VALUES (source.CCCD, source.Name, source.Birth);
";
                    using (SqlCommand command = new SqlCommand(queryHuman, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@CCCD", cccd);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Birth", birthday);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback(); // Rollback nếu có lỗi
                    throw;
                }
            }
        }
        public static async Task DeleteHuman()
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Human ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //command.Parameters.AddWithValue("@CCCD",cccd);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static async Task DeleteCCCD(string? cccd)
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Human WHERE CCCD=@CCCD";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CCCD", cccd);
                    if (cccd != null)
                    {
                        command.ExecuteNonQuery();
                        return;
                    }
                    throw new Exception("Không tìm thấy người dùng với tên: " + cccd);
                }
            }
        }
    }
}
