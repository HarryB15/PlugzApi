using System;
using System.Data;
using System.Data.SqlClient;
using PlugzApi.Services;

namespace PlugzApi.Models
{
    public class PostMessages: Base
	{
        public int postMessageId { get; set; }
        public int postId { get; set; }
        public int senderUserId { get; set; }
        public int receiverUserId { get; set; }
        public DateTime sentDatetime { get; set; }
        public bool messageRead { get; set; }
        public DateTime? messageReadDatetime { get; set; }

        public async Task InsPostMessages()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsPostMessages", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@senderUserId", SqlDbType.Int).Value = senderUserId;
                cmd.Parameters.Add("@receiverUserIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId;
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

