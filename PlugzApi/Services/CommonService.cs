﻿using System.Data;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using PlugzApi.Models;

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
            SentrySdk.CaptureException(ex);
        }

        public string? GenerateJwt(int userId)
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
    }
}
