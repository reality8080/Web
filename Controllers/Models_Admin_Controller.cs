using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using Web.Models.Class;

namespace Web.Models.Controller
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
        }
        [HttpGet]
        public IActionResult GET()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connect))
                {
                    connection.Open();
                    string query = @"SELECT h.CCCD,h.Name, h.Birth, A.adminName, A.typeOfAcc
                    FROM Human h
                    LEFT JOIN Admin a ON h.CCCD=A.CCCD
                    ";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            var result = dataTable.AsEnumerable()
                                .Select(row => dataTable.Columns.Cast<DataColumn>()
                                .ToDictionary(col => col.ColumnName, col => row[col]));
                            return Ok(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetHumanWithAdmin(string id)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();

                string query = @"
                SELECT h.CCCD,h.Name, h.Birth, A.adminName, A.typeOfAcc
                FROM Human h
                LEFT JOIN Admin A ON h.CCCD = A.CCCD
                WHERE h.CCCD = @CCCD";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CCCD", id);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                        var result = dataTable.AsEnumerable()
                            .Select(row => dataTable.Columns.Cast<DataColumn>()
                                .ToDictionary(col => col.ColumnName, col => row[col]));

                        return Ok(result);

                    }
                }
            }
        }
    
        [HttpPost]
        public IActionResult Add(Admin admin)
        {
            try
            {
                
                createTable();
                Insert(admin.Cccd, admin.Name, admin.Birthday, admin.ADmin, admin.Password, Admin.TypeOfAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
            return Ok(new { Success = true, });
        }

        private void createTable()
        {
            using (SqlConnection connection = new SqlConnection(connect))
            {
                Models_Human.Connect = connect;
                Models_Human.createTable();
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
                    END";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        private void Insert(string? cccd, string? name,string? birth, string? adminName,  string? pass, Boolean typeOfAcc)
        {
            Models_Human.InsertHman(cccd,name,birth);
            using (SqlConnection connection = new SqlConnection(connect))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                        MERGE INTO Admin AS target
                        USING (VALUES(@CCCD,@adminName,@pass,@typeOfAcc)) AS Source(CCCD, adminName, pass, typeOfAcc)
                        ON target.CCCD=source.CCCD
                        WHEN MATCHED THEN
                            UPDATE SET CCCD=source.CCCD,
                            adminName=source.adminName, 
                            pass=source.pass, 
                            typeOfAcc=source.typeOfAcc
                        WHEN NOT MATCHED THEN
                            INSERT (CCCD,adminName, pass, typeOfAcc)
                            VALUES (source.CCCD, source.adminName, source.pass, source.typeOfAcc)
                        ;";
                        using(SqlCommand command = new SqlCommand(query,connection, transaction))
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
    }
}
