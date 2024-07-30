using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Users: Login
    {
		public string userName { get; set; } = "";
        public bool verified { get; set; }
        private bool tokenExpired = false;
        public string email { get; set; } = "";
        public async Task GetUser()
        {
            try
            {
                var tokenValid = ValidateJwtToken();
                if (tokenExpired)
                {
                    tokenValid = true;
                    tokenExpired = false;
                    jwt = CommonService.GenerateJwt(userId);
                    if (jwt == null)
                    {
                        error = CommonService.GetUnexpectedErrrorMsg();
                    }
                }
                if (tokenValid)
                {
                    con = await CommonService.Instance.Open();
                    cmd = new SqlCommand("GetUser", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    sdr = await cmd.ExecuteReaderAsync();
                    if (sdr.Read())
                    {
                        userName = (string)sdr["UserName"];
                        email = (string)sdr["Email"];
                        verified = (bool)sdr["Verified"];
                        mustResetPass = (bool)sdr["MustResetPass"];
                        lat = (decimal)sdr["Lat"];
                        lng = (decimal)sdr["Lng"];
                    }
                    else
                    {
                        error = new Error()
                        {
                            errorCode = StatusCodes.Status404NotFound,
                            errorMsg = "Unable to find a user with the details entered"
                        };
                    }
                }
            }
            catch(Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }

		public async Task CreateUser()
		{
            try
            {
                con = await CommonService.Instance.Open();
                error = await CheckUserExists();
                if (error == null)
                {
                    string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                    var hashedPassword = HashPassword(salt);
                    cmd = new SqlCommand("InsertUsers", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@userName", SqlDbType.VarChar).Value = userName;
                    cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = email;
                    cmd.Parameters.Add("@salt", SqlDbType.VarChar).Value = salt;
                    cmd.Parameters.Add("@hashedPassword", SqlDbType.VarChar).Value = hashedPassword;
                    cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = lat;
                    cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = lng;
                    sdr = await cmd.ExecuteReaderAsync();
                    if (sdr.Read())
                    {
                        userId = (int)sdr["UserId"];
                        jwt = CommonService.GenerateJwt(userId);
                        if (jwt == null)
                        {
                            error = CommonService.GetUnexpectedErrrorMsg();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        private async Task<Error?> CheckUserExists()
        {
            try
            {
                cmd = new SqlCommand("CheckUserExists", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = email;
                cmd.Parameters.Add("@userName", SqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@errorMsg", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                await cmd.ExecuteNonQueryAsync();
                string? errorMsg = (cmd.Parameters["@errorMsg"].Value != DBNull.Value) ? (string)cmd.Parameters["@errorMsg"].Value : null;
                if(errorMsg == null)
                {
                    return null;
                }
                else
                {
                    return new Error()
                    {
                        errorMsg = errorMsg,
                        errorCode = StatusCodes.Status400BadRequest
                    };
                }
            }
            catch(Exception ex)
            {
                CommonService.Log(ex);
                return CommonService.GetUnexpectedErrrorMsg();
            }
        }
        private bool ValidateJwtToken()
        {
            try
            {
                string publicKey = File.ReadAllText(Directory.GetCurrentDirectory() + "/Auth/jwtPublicKey.pem");
                RSA publicRsa = RSA.Create();
                publicRsa.ImportFromPem(publicKey.ToCharArray());
                var publicKeySecurityKey = new RsaSecurityKey(publicRsa);
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = publicKeySecurityKey,
                    ClockSkew = TimeSpan.Zero
                };
                var claims = tokenHandler.ValidateToken(jwt, validationParameters, out SecurityToken validatedToken);
                userId = int.Parse(claims.FindFirst("userId")!.Value);
                validationParameters.ValidateLifetime = true; //need to get user id before checking if token is expired
                tokenHandler.ValidateToken(jwt, validationParameters, out SecurityToken validatedToken2);
                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                tokenExpired = true;
                return false;
            }
            catch (Exception)
            {
                error = CommonService.GetUnexpectedErrrorMsg();
                return false;
            }
        }
        public async Task UpdateUserLocation()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdateUserLocation", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = lng;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
    }
}

