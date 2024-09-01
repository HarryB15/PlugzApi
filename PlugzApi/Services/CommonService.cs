using System.Data;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using PlugzApi.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Text;
using Azure.Maps.Search;
using Azure;

namespace PlugzApi.Services
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
        public static async Task Close(SqlConnection? con, SqlDataReader? sdr = null)
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
        public static Error GetUnexpectedErrrorMsg()
        {
            return new Error()
            {
                errorMsg = "Unexpected error occured, please try later",
                errorCode = StatusCodes.Status500InternalServerError
            };
        }
        public static void Log(Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }

        public static string? GenerateJwt(int userId)
        {
            try
            {
                string privateKey = File.ReadAllText(Directory.GetCurrentDirectory() + "/Auth/jwtPrivateKey.pem");
                RSA privateRsa = RSA.Create();
                privateRsa.ImportFromPem(privateKey.ToCharArray());
                var privateKeySecurityKey = new RsaSecurityKey(privateRsa);
                var signingCredentials = new SigningCredentials(privateKeySecurityKey, SecurityAlgorithms.RsaSha256);
                var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "userId"),
                    new Claim("userId", userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, iat)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = signingCredentials
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Log(ex);
                return null;
            }
        }
        public BlobServiceClient GetBlobServiceClient()
        {
            return new BlobServiceClient(new Uri(_configuration["Azure:Storage"]!), new DefaultAzureCredential());
        }
        public MapsSearchClient GetMapsClient()
        {
            var credential = new AzureKeyCredential(_configuration["Azure:Maps"]!);
            return new MapsSearchClient(credential);
        }
        public static string HashString(string message, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public static DataTable AddListInt(List<int> listInts)
        {
            var dt = new DataTable();
            dt.Columns.Add("IntValue", typeof(int));
            foreach (var listInt in listInts)
            {
                dt.Rows.Add(listInt);
            }
            return dt;
        }
        public string GetConfig(string key)
        {
            return _configuration[key]!;
        }
    }
}

