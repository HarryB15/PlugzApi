using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class BlockedAccounts: Base
	{
        public int blockedId { get; set; }
        public int blockedUserId { get; set; }
        public string userName { get; set; } = "";

        public async Task InsBlockedAccounts()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsBlockedAccounts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@blockedUserId", SqlDbType.Int).Value = blockedUserId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task DeleteBlockedAccount()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("DeleteBlockedAccount", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@blockedId", SqlDbType.Int).Value = blockedId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task<List<BlockedAccounts>> GetUsersBlockedAccounts()
        {
            List<BlockedAccounts> blockedAccounts = new List<BlockedAccounts>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersBlockedAccounts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    blockedAccounts.Add(new BlockedAccounts()
                    {
                        blockedId = (int)sdr["BlockedId"],
                        blockedUserId = (int)sdr["BlockedUserId"],
                        userName = (string)sdr["UserName"]
                    });
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return blockedAccounts;
        }
    }
}

