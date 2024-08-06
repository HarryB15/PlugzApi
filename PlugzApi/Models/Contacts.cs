using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models.Security;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Contacts: Base
	{
        public int contactId { get; set; }
        public DateTime? lastMessageDate { get; set; }
        public int connectionStatus { get; set; }
        public bool isConnected { get; set; }
        public Users contactUser { get; set; } = new Users();
        public Messages? mostRecentMsg { get; set; }
        public ProfilePhotos profilePhoto { get; set; } = new ProfilePhotos();
        public async Task<List<Contacts>> GetUsersContacts()
        {
            List<Contacts> contacts = new List<Contacts>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersContacts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@isConnected", SqlDbType.Bit).Value = isConnected;
                cmd.Parameters.Add("@existingContactIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Contacts contact = new Contacts()
                    {
                        contactId = (int)sdr["ContactId"],
                        lastMessageDate = (sdr["LastMessageDate"] != DBNull.Value) ? (DateTime)sdr["LastMessageDate"] : null,
                        contactUser = new Users()
                        {
                            userId = (int)sdr["ContactUserId"],
                            userName = (string)sdr["UserName"]
                        }
                    };
                    if (sdr["MessageId"] != DBNull.Value)
                    {
                        contact.mostRecentMsg = new Messages()
                        {
                            messageId = (int)sdr["MessageId"],
                            messageText = (string)sdr["MessageText"],
                            userIsSender = (bool)sdr["UserIsSender"],
                            sentDatetime = (DateTime)sdr["SentDatetime"],
                            messageRead = (bool)sdr["MessageRead"]
                        };
                    }
                    contacts.Add(contact);
                }
                foreach(var contact in contacts)
                {
                    contact.profilePhoto.userId = contact.contactUser.userId;
                    await contact.profilePhoto.GetProfilePhoto();
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return contacts;
        }
        public async Task GetContact()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetContact", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@contactUserId", SqlDbType.Int).Value = contactUser.userId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    contactId = (int)sdr["ContactId"];
                    connectionStatus = (int)sdr["ConnectionStatus"];
                    contactUser.userName = (string)sdr["UserName"];
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task UpdUserContactIsConnected()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdUserContactIsConnected", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@contactId", SqlDbType.Int).Value = contactId;
                cmd.Parameters.Add("@isConnected", SqlDbType.Bit).Value = isConnected;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task<List<Contacts>> GetUsersContactsBasic()
        {
            List<Contacts> contacts = new List<Contacts>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersContactsBasic", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    contacts.Add(new Contacts()
                    {
                        contactId = (int)sdr["ContactId"],
                        contactUser = new Users()
                        {
                            userId = (int)sdr["ContactUserId"],
                            userName = (string)sdr["UserName"]
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return contacts;
        }
    }
}

