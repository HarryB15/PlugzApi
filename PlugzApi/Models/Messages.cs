using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Messages: Base
	{
        public int messageId { get; set; }
        public byte messageTypeId { get; set; }
        public string messageText { get; set; } = "";
        public int senderUserId { get; set; }
        public int receiverUserId { get; set; }
        public bool userIsSender { get; set; }
        public DateTime sentDatetime { get; set; }
        public bool messageRead { get; set; }
        public async Task InsMessage()
        {

            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsMessage", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@messageTypeId", SqlDbType.TinyInt).Value = messageTypeId;
                cmd.Parameters.Add("@messageText", SqlDbType.NVarChar).Value = messageText;
                cmd.Parameters.Add("@senderUserId", SqlDbType.Int).Value = senderUserId;
                cmd.Parameters.Add("@receiverUserId", SqlDbType.Int).Value = receiverUserId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        public async Task<List<Messages>> GetMessages(int contactUserId)
        {
            List<Messages> messages = new List<Messages>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetMessages", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@contactUserId", SqlDbType.Int).Value = contactUserId;
                cmd.Parameters.Add("@existingMessageIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Messages message = new Messages()
                    {
                        messageId = (int)sdr["MessageId"],
                        messageTypeId = (byte)sdr["MessageTypeId"],
                        messageText = (string)sdr["MessageText"],
                        senderUserId = (int)sdr["SenderUserId"],
                        receiverUserId = (int)sdr["ReceiverUserId"],
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                    };
                    message.userIsSender = (message.senderUserId == userId);
                    messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
            return messages;
        }
    }
}