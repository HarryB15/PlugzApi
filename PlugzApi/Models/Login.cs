using System;
using System.Data;
using System.Data.SqlClient;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Login: Base
    {
        public string email { get; set; } = "";
        public string password { get; set; } = "";
        public string? jwt { get; set; } = null;
        public async Task<bool> ValidateUser()
        {
            var valid = false;
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = email;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    var salt = (string)sdr["Salt"];
                    var expHashedPassword =  (string)sdr["HashedPassword"];
                    var actualHashedPassword = HashPassword(salt);
                    valid = expHashedPassword == actualHashedPassword;
                    if (valid)
                    {
                        userId = (int)sdr["UserId"];
                        jwt = CommonService.Instance.GenerateJwt(userId);
                        if(jwt == null)
                        {
                            error = CommonService.Instance.GetUnexpectedErrrorMsg();
                        }
                    }
                }
                if (!valid)
                {
                    error = new Error()
                    {
                        errorMsg = "The email or password entered is incorrect",
                        errorCode = StatusCodes.Status400BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
            return valid;
        }
        protected string HashPassword(string salt)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return hashedPassword.Replace(salt, "");
        }
    }
}

