﻿using System;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class ProfilePhotos: Base
	{
		public string image { get; set; } = "";
        public async Task GetProfilePhoto()
        {
            try
            {
                AzureStorageService azureStorageService = new AzureStorageService();
                var hashedUserId = CommonService.Instance.HashString(userId.ToString(), "ProfilePhotos");
                var images = await azureStorageService.GetPhotos("profile-photos", hashedUserId);
                if(images != null)
                {
                    image = (images.Count > 0) ? images[0] : "";
                }
                else
                {
                    error = CommonService.Instance.GetUnexpectedErrrorMsg();
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
        public async Task UpdateProfilePhoto()
        {
            try
            {
                AzureStorageService azureStorageService = new AzureStorageService();
                var hashedUserId = CommonService.Instance.HashString(userId.ToString(), "ProfilePhotos");
                var success = await azureStorageService.StoreImage(image, "profile-photos", $"{hashedUserId}.txt");
                if (!success)
                {
                    error = CommonService.Instance.GetUnexpectedErrrorMsg();
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
    }
}

