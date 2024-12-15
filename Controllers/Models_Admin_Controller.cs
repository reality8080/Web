﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Data;
using Web.Models.Class;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class Models_Admin_Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;
        static string? connect;

        public Models_Admin_Controller(IConfiguration configuration)
        {
            _configuration = configuration;
            connect = _configuration.GetConnectionString("DefaultConnection") ?? "";
            Models_Human.Connect = connect;
            Models_Human.createTable();
            createTable();
        }

        [HttpGet]
        public IActionResult GET()
        {
            try
            {
                var result=Display("Name","ASC");
                return Ok(new { result,Success=true,});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("SortColumn/{Column}")]
        public IActionResult GET(string Column, string  Sort="ASC")
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

        [HttpGet("Take/{adminName}")]
        public IActionResult GetHumanWithAdmin(string adminName)
        {
            try
            {
                var result = Search(adminName);
                return Ok(new { result,Success=true});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult Add(Admin admin)
        {
            try
            {
                InsertPost(admin.Cccd, admin.Name, admin.Birthday, admin.ADmin, admin.Password, Admin.TypeOfAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, });
            //return Ok(new { Success = true, });
        }

        [HttpPut]
        public IActionResult Put([FromBody] Admin admin)
        {
            try
            {
                InsertPut(admin.Cccd, admin.Name, admin.Birthday, admin.ADmin, admin.Password, Admin.TypeOfAccount);
                //return Ok(new { message = "Data inserted/update successfully" });
            }
            catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, message = "Data inserted/update successfully" });

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
                return BadRequest(new { message = ex.Message });
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, message = "All data deleted successfully" });
            //return Ok(new { Success = true, });
        }
        [HttpDelete("Delete/{adminName}")]
        public IActionResult Delete(string adminName)
        {
            try
            {
                DeleteAdmin(adminName);
                //return Ok(new { message = "All data deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            var result = Display("Name", "ASC");
            return Ok(new { result, Success = true, message = "All data deleted successfully" });
            //return Ok(new { Success = true, });
        }

        private void createTable()
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {

                connection.Open();
                string query = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Admin')
                    BEGIN
                        CREATE TABLE Admin(
                            CCCD NVARCHAR(50) PRIMARY KEY,
                            adminName VARCHAR(50),
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
        private void InsertPost(string? cccd, string? name, string? birth, string? adminName, string? pass, Boolean typeOfAcc)
        {
            Models_Human.InsertHman(cccd, name, birth);
            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //string query = @"
                        //MERGE INTO Admin AS target
                        //USING (VALUES(@CCCD,@adminName,@pass,@typeOfAcc)) AS Source(CCCD, adminName, pass, typeOfAcc)
                        //ON target.CCCD=source.CCCD
                        //WHEN MATCHED THEN
                        //    UPDATE SET CCCD=source.CCCD,
                        //    adminName=source.adminName, 
                        //    pass=source.pass, 
                        //    typeOfAcc=source.typeOfAcc
                        //WHEN NOT MATCHED THEN
                        //    INSERT (CCCD,adminName, pass, typeOfAcc)
                        //    VALUES (source.CCCD, source.adminName, source.pass, source.typeOfAcc)
                        //;";
                        string query = @"
                                IF NOT EXISTS (SELECT * FROM Admin WHERE CCCD = @CCCD)
                                BEGIN
                                    INSERT INTO Admin (CCCD, adminName, pass, typeOfAcc)
                                    VALUES (@CCCD, @adminName, @pass, @typeOfAcc);
                                END
                                ";
                        // Chỉ kiểm tra sự tồn tại của CCCD, tác dụng chính là tránh lỗi PRIMARY KEY
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CCCD", cccd);
                            //command.Parameters.AddWithValue("@name", name);
                            //command.Parameters.AddWithValue("");
                            command.Parameters.AddWithValue("@adminName", adminName);
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

        private void InsertPut(string? cccd, string? name, string? birth, string? adminName, string? pass, Boolean typeOfAcc)
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
                            MERGE INTO Admin AS target
                            USING (VALUES (@CCCD, @adminName, @pass, @typeOfAcc)) AS Source(CCCD, adminName, pass, typeOfAcc)
                            ON target.CCCD = Source.CCCD
                            WHEN MATCHED AND (target.adminName <> Source.adminName OR target.pass <> Source.pass OR target.typeOfAcc <> Source.typeOfAcc) THEN
                                UPDATE SET 
                                    adminName = Source.adminName, 
                                    pass = Source.pass, 
                                    typeOfAcc = Source.typeOfAcc
                            WHEN NOT MATCHED THEN
                                INSERT (CCCD, adminName, pass, typeOfAcc)
                                VALUES (Source.CCCD, Source.adminName, Source.pass, Source.typeOfAcc);
";
                        // Kiểm tra sự giống nhau của bản ghi cũ và mới
                        // Nếu giống thì bỏ qua
                        // Nếu không giống hoàn toàn thì cập nhật.
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CCCD", cccd);
                            //command.Parameters.AddWithValue("@name", name);
                            //command.Parameters.AddWithValue("");
                            command.Parameters.AddWithValue("@adminName", adminName);
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
        private void DeleteHuman()
        {
            using(SqlConnection connection = new SqlConnection(connect))
            {
                Models_Human.Connect = connect;
                Models_Human.DeleteHuman();
                connection.Open();
                string query = "DELETE FROM Admin ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //command.Parameters.AddWithValue("@CCCD",cccd);
                    command.ExecuteNonQuery ();
                }
            }
        }
        private void DeleteAdmin(string adminName)
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                Models_Human.Connect = connect;
                string humanquery = "SELECT CCCD FROM Admin Where adminName=@adminName";           
                connection.Open();
                using (SqlCommand command = new SqlCommand(humanquery, connection))
                {
                    command.Parameters.AddWithValue("@adminName", adminName);
                    var cccd=command.ExecuteScalar();
                    Models_Human.DeleteCCCD(cccd.ToString());
                }
                //Models_Human.DeleteCCCD(cccd);
                string query = "DELETE FROM Admin WHERE adminName=@adminName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@adminName", adminName);
                    command.ExecuteNonQuery();
                }
            }
        }
        private EnumerableRowCollection<Dictionary<string,object>> Display(string columnName, string sortOrder)
        {

            var validColumns = new HashSet<string> { "CCCD", "Name", "Birth", "adminName", "typeOfAcc" };
            if (!validColumns.Contains(columnName))
                throw new ArgumentException($"Invalid column name: {columnName}");
            if (!string.Equals(sortOrder, "ASC", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sortOrder, "DESC", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid sort order: {sortOrder}");


            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();
                string query = $@"SELECT h.CCCD,h.Name, h.Birth, A.adminName, A.typeOfAcc
                    FROM Admin a
                    LEFT JOIN Human h ON h.CCCD=A.CCCD
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
        private EnumerableRowCollection<Dictionary<string, object>> Search(string adminName)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();

                string query = @"
     SELECT h.CCCD,h.Name, h.Birth, A.adminName, A.typeOfAcc
     FROM Human h
     LEFT JOIN Admin A ON h.CCCD = A.CCCD
     WHERE A.adminName = @adminName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@adminName", adminName);

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
//using (SqlConnection connection = new SqlConnection(connect))
//{
//    connection.Open();
//    string query = $@"SELECT h.CCCD,h.Name, h.Birth, A.adminName, A.typeOfAcc
//                    FROM Human h
//                    LEFT JOIN Admin a ON h.CCCD=A.CCCD
//                    ORDER BY {columnName} {sortOrder}
//                    ";
//    using (SqlCommand command = new SqlCommand(query, connection))
//    {
//        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
//        {
//            DataTable dataTable = new DataTable();
//            adapter.Fill(dataTable);
//            var result = dataTable.AsEnumerable()
//                .Select(row => dataTable.Columns.Cast<DataColumn>()
//                .ToDictionary(col => col.ColumnName, col => row[col]));
//            return Ok(result);
//        }
//    }
//}