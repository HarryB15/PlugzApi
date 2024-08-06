using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class Posts: Base
	{
        public int postId { get; set; }
        public string postText { get; set; } = "";
        public decimal price { get; set; }
        public byte minUserRatings { get; set; }
        public short? minSales { get; set; }
        public bool isPublic { get; set; }
        public DateTime createdDatetime { get; set; }
        public DateTime? expiryDatetime { get; set; }
        public int expiryHours { get; set; }
        public string userName { get; set; } = "";

        public async Task InsPosts(bool shareWithContacts)
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsPosts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@postText", SqlDbType.NVarChar).Value = postText;
                cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                cmd.Parameters.Add("@minUserRatings", SqlDbType.TinyInt).Value = minUserRatings;
                cmd.Parameters.Add("@minSales", SqlDbType.SmallInt).Value = minSales;
                cmd.Parameters.Add("@isPublic", SqlDbType.Bit).Value = isPublic;
                cmd.Parameters.Add("@expiryHours", SqlDbType.Int).Value = expiryHours;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    postId = (int)sdr["PostId"];
                    await sdr.CloseAsync();
                    if (shareWithContacts)
                    {
                        var contactObj = new Contacts();
                        contactObj.userId = userId;
                        var contacts = await contactObj.GetUsersContactsBasic();

                        var messages = new Messages();
                        messages.messageTypeId = 2;
                        messages.senderUserId = userId;
                        messages.extId = postId;
                        foreach(var contact in contacts)
                        {
                            messages.receiverUserId = contact.contactUser.userId;
                            await messages.InsMessage()
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task DeletePost()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("DeletePost", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
        }
        public async Task<List<Posts>> GetUsersPosts(bool incExpired)
        {
            List<Posts> posts = new List<Posts>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersPosts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@incExpired", SqlDbType.Bit).Value = incExpired;
                cmd.Parameters.Add("@existingPostIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Posts post = new Posts()
                    {
                        postId = (int)sdr["PostId"],
                        userId = userId,
                        postText = (string)sdr["PostText"],
                        price = (decimal)sdr["Price"],
                        minUserRatings = (byte)sdr["MinUserRatings"],
                        minSales = (short)sdr["MinSales"],
                        isPublic = (bool)sdr["IsPublic"],
                        createdDatetime = (DateTime)sdr["CreatedDatetime"],
                        expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                    };
                    posts.Add(post);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return posts;
        }
        public async Task UpdPost()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdPost", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId;
                cmd.Parameters.Add("@postText", SqlDbType.NVarChar).Value = postText;
                cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                cmd.Parameters.Add("@minUserRatings", SqlDbType.TinyInt).Value = minUserRatings;
                cmd.Parameters.Add("@minSales", SqlDbType.SmallInt).Value = minSales;
                cmd.Parameters.Add("@isPublic", SqlDbType.Bit).Value = isPublic;
                cmd.Parameters.Add("@expiryHours", SqlDbType.Int).Value = expiryHours;
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

