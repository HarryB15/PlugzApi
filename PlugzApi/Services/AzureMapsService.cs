using System;
using Azure;
using Azure.Maps.Search.Models;
using PlugzApi.Models;

namespace PlugzApi.Services
{
	public class AzureMapsService
	{
		public List<Location>? SearchAddress(string address)
		{
			try
			{
				List<Location> locations = new List<Location>();
                var mapsClient = CommonService.Instance.GetMapsClient();
                Response<GeocodingResponse> result = mapsClient.GetGeocoding(address);
                for (int i = 0; i < result.Value.Features.Count; i++)
                {
					var features = result.Value.Features[i];
					locations.Add(new Location()
					{
						address = features.Properties.Address.FormattedAddress,
						lat = (decimal)features.Geometry.Coordinates[1],
                        lng = (decimal)features.Geometry.Coordinates[0]
                    });
                }
                return locations;
			}
			catch(Exception ex)
			{
				CommonService.Log(ex);
				return null;
			}
        }
	}
}

