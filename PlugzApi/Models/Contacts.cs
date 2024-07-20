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
        public bool isConnected { get; set; }
        public Users contactUser { get; set; } = new Users();
        public Messages? mostRecentMsg { get; set; }
        public async Task<List<Contacts>> GetUsersContacts()
        {
            List<Contacts> contacts = new List<Contacts>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersContacts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Contacts contact = new Contacts()
                    {
                        contactId = (int)sdr["ContactId"],
                        lastMessageDate = (sdr["LastMessageDate"] != DBNull.Value) ? (DateTime)sdr["LastMessageDate"] : null,
                        isConnected = (bool)sdr["IsConnected"],
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
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
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
                    isConnected = (bool)sdr["IsConnected"];
                    contactUser.userName = (string)sdr["UserName"];
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
    }
}

