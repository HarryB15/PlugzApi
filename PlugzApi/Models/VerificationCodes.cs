using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace PlugzApi.Models
{
	public class VerificationCodes: Login
	{
		public int codeId { get; set; }
		public int code { get; set; }
        public int submittedCode { get; set; }
		public async Task InsVerificationCode()
		{
            try
            {
                code = GenerateVerificationCode();
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsVerificationCode", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@code", SqlDbType.Int).Value = code;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        public async Task ValidateVerificationCode()
        {
            await GetVerificationCode();
            if(error == null)
            {
                if(submittedCode == code)
                {
                    await DelVerificationCode();
                    await UpdUserVerified();
                    jwt = CommonService.Instance.GenerateJwt(userId);
                    if (jwt == null)
                    {
                        error = CommonService.Instance.GetUnexpectedErrrorMsg();
                    }
                }
                else
                {
                    error = new Error()
                    {
                        errorCode = StatusCodes.Status406NotAcceptable,
                        errorMsg = "Code is not valid"
                    };
                }
            }
        }
        private async Task DelVerificationCode()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("DelVerificationCode", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@codeId", SqlDbType.Int).Value = codeId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        private async Task UpdUserVerified()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdUserVerified", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        private async Task GetVerificationCode()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetVerificationCode", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    code = (int)sdr["Code"];
                    codeId = (int)sdr["CodeID"];
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        private int GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 1000000);
        }
    }
}

