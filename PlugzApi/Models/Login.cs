using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Login: Base
    {
        public string emailUsername { get; set; } = "";
        public string password { get; set; } = "";
        public async Task<bool> ValidateUser()
        {
            var valid = false;
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@email_userName", SqlDbType.VarChar).Value = emailUsername;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    var salt = (string)sdr["Salt"];
                    var expHashedPassword =  (string)sdr["HashedPassword"];
                    var actualHashedPassword = HashPassword(salt);
                    valid = expHashedPassword == actualHashedPassword;
                    userId = (int)sdr["UserId"];
                    await sdr.CloseAsync();

                    var lockedDatetime = await CheckAccountLocked();
                    if(lockedDatetime == null)
                    {
                        if (valid)
                        {
                            await DeleteUserFailedSignIns();
                            jwt = CommonService.Instance.GenerateJwt(userId);
                            if (jwt == null)
                            {
                                error = CommonService.Instance.GetUnexpectedErrrorMsg();
                            }
                        }
                        else
                        {
                            lockedDatetime = await InsFailedSignInAttempts();
                            if(lockedDatetime != null)
                            {
                                GetLockedErrorMsg(lockedDatetime);
                            }
                        }
                    }
                    else
                    {
                        GetLockedErrorMsg(lockedDatetime);
                    }
                }
                if (!valid && error == null)
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
        private async Task<DateTime?> CheckAccountLocked()
        {
            DateTime? lockedDatetime = null;
            try
            {
                cmd = new SqlCommand("CheckAccountLocked", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lockedDatetime", SqlDbType.DateTime).Direction = ParameterDirection.Output;
                await cmd.ExecuteNonQueryAsync();
                lockedDatetime = (cmd.Parameters["@lockedDatetime"].Value != DBNull.Value) ?
                    (DateTime)cmd.Parameters["@lockedDatetime"].Value : null;
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            return lockedDatetime;
        }
        private async Task<DateTime?> InsFailedSignInAttempts()
        {
            DateTime? lockedDatetime = null;
            try
            {
                cmd = new SqlCommand("InsFailedSignInAttempts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lockedDatetime", SqlDbType.DateTime).Direction = ParameterDirection.Output;
                await cmd.ExecuteNonQueryAsync();
                lockedDatetime = (cmd.Parameters["@lockedDatetime"].Value != DBNull.Value) ?
                    (DateTime)cmd.Parameters["@lockedDatetime"].Value : null;
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            return lockedDatetime;
        }
        private async Task DeleteUserFailedSignIns()
        {
            try
            {
                cmd = new SqlCommand("DeleteUserFailedSignIns", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
        private void GetLockedErrorMsg(DateTime? lockedDatetime)
        {
            error = new Error()
            {
                errorMsg = "Your account is locked until " + lockedDatetime!.Value.ToString("HH:mm"),
                errorCode = StatusCodes.Status400BadRequest
            };
        }
        protected string HashPassword(string salt)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return hashedPassword.Replace(salt, "");
        }
        public async Task<Users> ResetPassword()
        {
            Users user = new Users();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUserByEmailUsername", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@email_userName", SqlDbType.VarChar).Value = emailUsername;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    userId = (int)sdr["UserId"];
                    user.userName = (string)sdr["UserName"];
                    user.email = (string)sdr["Email"];
                    await sdr.CloseAsync();
                    await InsResetPasswordAttempts();
                    if(error == null)
                    {
                        GenerateRandomPassword();
                        await UpdUsersPassword();
                    }
                }
                else
                {
                    error = new Error()
                    {
                        errorMsg = "No account exists with the deatils entered",
                        errorCode = StatusCodes.Status400BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            return user;
        }
        private async Task InsResetPasswordAttempts()
        {
            try
            {
                cmd = new SqlCommand("InsResetPasswordAttempts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@valid", SqlDbType.Bit).Direction = ParameterDirection.Output;
                await cmd.ExecuteNonQueryAsync();
                var valid = (bool)cmd.Parameters["@valid"].Value;
                if (!valid)
                {
                    error = new Error()
                    {
                        errorMsg = "Too many reset password attempts have been sent, please try again later",
                        errorCode = StatusCodes.Status400BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
        private void GenerateRandomPassword()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder result = new StringBuilder(12);
            for (int i = 0; i < 12; i++)
            {
                int index = random.Next(chars.Length);
                result.Append(chars[index]);
            }
            password = result.ToString();
        }
        private async Task UpdUsersPassword()
        {
            try
            {
                string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                var hashedPassword = HashPassword(salt);
                cmd = new SqlCommand("UpdUsersPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@salt", SqlDbType.VarChar).Value = salt;
                cmd.Parameters.Add("@hashedPassword", SqlDbType.VarChar).Value = hashedPassword;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
    }
}

