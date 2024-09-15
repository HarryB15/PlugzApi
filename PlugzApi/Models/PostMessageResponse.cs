using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class PostMessageResponse: Base
	{
		public int responseId { get; set; }
        public int postMessageId { get; set; }

        public async Task UpsertPostMessageResponses()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpsertPostMessageResponses", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@postMessageId", SqlDbType.Int).Value = postMessageId;
                cmd.Parameters.Add("@listingIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
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

