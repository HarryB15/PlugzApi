using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models;
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
        public string userName { get; set; } = "";
        public bool userIsSender { get; set; }
        public Posts post { get; set; } = new Posts();
        public ProfilePhotos profilePhoto { get; set; } = new ProfilePhotos();

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
        public async Task<List<PostMessages>> GetUsersPostMessages()
        {
            var messages = new List<PostMessages>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersPostMessages", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@existingPostMessageIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                messages = ReadPostMessages(sdr);
                foreach(var message in messages)
                {
                    message.profilePhoto.userId = message.userId;
                    await message.profilePhoto.GetProfilePhoto();
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return messages;
        }
        public async Task<List<PostMessages>> SearchPostMessages(string searchValue)
        {
            var messages = new List<PostMessages>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("SearchPostMessages", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@searchValue", SqlDbType.VarChar).Value = searchValue;
                cmd.Parameters.Add("@existingPostMessageIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                messages = ReadPostMessages(sdr);
                foreach (var message in messages)
                {
                    message.profilePhoto.userId = message.userId;
                    await message.profilePhoto.GetProfilePhoto();
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return messages;
        }

        private List<PostMessages> ReadPostMessages(SqlDataReader sdr)
        {
            var postMessages = new List<PostMessages>();
            while (sdr.Read())
            {
                postMessages.Add(new PostMessages()
                {
                    postMessageId = (int)sdr["PostMessageId"],
                    postId = (int)sdr["PostId"],
                    senderUserId = (int)sdr["SenderUserId"],
                    receiverUserId = (int)sdr["ReceiverUserId"],
                    sentDatetime = (DateTime)sdr["SentDatetime"],
                    messageRead = (bool)sdr["MessageRead"],
                    post = new Posts()
                    {
                        postId = (int)sdr["PostId"],
                        postText = (string)sdr["PostText"],
                        userId = (int)sdr["PostUserId"],
                        price = (decimal)sdr["Price"],
                        createdDatetime = (DateTime)sdr["CreatedDatetime"],
                        expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                        userName = (string)sdr["PostUserName"],
                        location = new Location()
                        {
                            lat = (decimal)sdr["Lat"],
                            lng = (decimal)sdr["Lng"],
                        }
                    },
                    userId = (int)sdr["UserId"],
                    userName = (string)sdr["UserName"],
                    userIsSender = userId == (int)sdr["SenderUserId"]
                });
            }
            return postMessages;
        }
        public async Task<List<int>> GetPostMessageOffers()
        {
            var listingIds = new List<int>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetPostMessageOffers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@postMessageId", SqlDbType.Int).Value = postMessageId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    listingIds.Add((int)sdr["ListingId"]);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return listingIds;
        }
    }
}

