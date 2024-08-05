using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class PrivacyOptions: Base
	{
		public int privacyOptionId { get; set; }
		public string privacyOption { get; set; } = "";
		public bool selected { get; set; }

        public async Task<List<PrivacyOptions>> GetUsersPrivacyOptions()
        {
            List<PrivacyOptions> privacyOptions = new List<PrivacyOptions>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersPrivacyOptions", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    privacyOptions.Add(new PrivacyOptions()
                    {
                        userId = userId,
                        privacyOptionId = (int)sdr["PrivacyOptionId"],
                        privacyOption = (string)sdr["PrivacyOption"],
                        selected = (bool)sdr["Selected"],
                    });
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return privacyOptions;
        }
        public async Task UpdInsUsersPrivacyOptions()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdInsUsersPrivacyOptions", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@privacyOptionId", SqlDbType.Int).Value = privacyOptionId;
                cmd.Parameters.Add("@selected", SqlDbType.Int).Value = selected;
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

