using System;
using System.Data;
using System.Data.SqlClient;
using PlugzApi.Interfaces;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Users: Login
    {
		public string name { get; set; } = "";
        public bool verified { get; set; }
        public async Task GetUser()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUser", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    name = (string)sdr["name"];
                    email = (string)sdr["email"];
                    verified = (bool)sdr["verified"];
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
            catch(Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }

		public async Task CreateUser()
		{
            try
            {
                con = await CommonService.Instance.Open();
                var userExists = await CheckUserExists();
                if (userExists)
                {
                    error = new Error()
                    {
                        errorMsg = "An account already exists with these details",
                        errorCode = StatusCodes.Status400BadRequest
                    };
                }
                else
                {
                    string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                    var hashedPassword = HashPassword(salt);
                    cmd = new SqlCommand("InsertUsers", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
                    cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = email;
                    cmd.Parameters.Add("@salt", SqlDbType.VarChar).Value = salt;
                    cmd.Parameters.Add("@hashedPassword", SqlDbType.VarChar).Value = hashedPassword;
                    sdr = await cmd.ExecuteReaderAsync();
                    if (sdr.Read())
                    {
                        userId = (int)sdr["UserId"];
                    }
                }
            }
            catch(Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        private async Task<bool> CheckUserExists()
        {
            try
            {
                cmd = new SqlCommand("CheckUserExists", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = email;
                sdr = await cmd.ExecuteReaderAsync();
                var userExists = sdr.Read();
                sdr.Close();
                return userExists;
            }
            catch(Exception ex)
            {
                CommonService.Instance.Log(ex);
                return true;
            }
        }
    }
}

