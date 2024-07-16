using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class CommonService
	{
        private readonly IConfiguration _configuration;
        private static CommonService? _instance;

        public CommonService(IConfiguration configuration)
        {
            _configuration = configuration;
            
        }
        public static void Initialize(IConfiguration configuration)
        {
            if (_instance == null)
            {
                _instance = new CommonService(configuration);
            }
        }

        public static CommonService Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("CommonService has not been initialized.");
                }
                return _instance;
            }
        }
        public async Task<SqlConnection> Open(string db = "Plugz")
        {
            string connectionString = _configuration.GetConnectionString(db)!;
            SqlConnection con = new SqlConnection(connectionString);
            await con.OpenAsync();
            return con;
        }
        public async Task Close(SqlConnection? con, SqlDataReader? sdr = null)
        {
            try
            {
                if (sdr != null)
                {
                    if (!sdr.IsClosed)
                    {
                        await sdr.CloseAsync();
                    }
                }
                if (con != null)
                {
                    if (con.State == ConnectionState.Open)
                    {
                        await con.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }
        public Error GetUnexpectedErrrorMsg()
        {
            return new Error()
            {
                errorMsg = "Unexpected error occured, please try later",
                errorCode = StatusCodes.Status500InternalServerError
            };
        }
        public void Log(Exception ex)
        {
            
        }
    }
}

