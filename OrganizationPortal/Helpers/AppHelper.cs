using OrganizationPortal.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Filters;
using OrganizationPortal.Data;
using OrganizationPortal.Controllers;
using System.Linq;
using System.Net;
using RestSharp;

namespace OrganizationPortal.Helpers
{
	public static class AppHelper
	{
		public static DataResult SaveFile(IFormFile image, string path, string fileName)
		{
			try
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				var filePath = Path.Combine(path, fileName);

				using (var stream = File.Create(filePath))
				{
					image.CopyTo(stream);
				}
			}
			catch (Exception ex)
			{
				return new DataResult { Type = DataResultTypes.Error, Message = ex.Message };
			}

			return new DataResult { Type = DataResultTypes.Success };
		}
		public static DataResult SaveFile(string base64, string path, string fileName)
		{
			if (base64 != null)
			{
				if (!Directory.Exists(path))			
					Directory.CreateDirectory(path);
				
				var filePath = Path.Combine(path, fileName);
				var base64Content = base64.Substring(base64.LastIndexOf(',') + 1); // base64.Replace(base64.Substring(base64.LastIndexOf(',')), "");
				try
				{
					var base64array = Convert.FromBase64String(base64Content);
					System.IO.File.WriteAllBytes(filePath, base64array);
				}
				catch (Exception ex)
				{
					return new DataResult { Type = DataResultTypes.Error , Message = ex.Message };
				}
			}

			return new DataResult { Type = DataResultTypes.Success };
		}
		public static void DeleteFile(string path, string fileName)
		{
			var filePath = Path.Combine(path, fileName);
			if (File.Exists(filePath))			
				File.Delete(filePath);
		}

		public static IpInfo GetUserLocationDetailsyByIp(string ip)
		{
			var client = new RestClient($"https://apility-io-ip-geolocation-v1.p.rapidapi.com/{ip}");
			var request = new RestRequest(Method.GET);
			request.AddHeader("x-rapidapi-host", "apility-io-ip-geolocation-v1.p.rapidapi.com");
			request.AddHeader("x-rapidapi-key", "5629a1449bmsh9e1f9bfdc82b15ep1a9c03jsn30a87f676c20");
			request.AddHeader("accept", "application/json");
			request.AddHeader("fields", "geo");
			
			IRestResponse response = client.Execute(request);

			IpInfo result = new IpInfo();
			if (response.IsSuccessful)
				result = JsonConvert.DeserializeObject<RestAPIResponseModel>(response.Content)?.Ip;
			
			return result;
		}
	}
	public class IpInfo
	{
		[JsonProperty("address")]
		public string Ip { get; set; }
		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("country")]
		public string Country{ get; set; }
		[JsonProperty("longitude")]
		public string Longitude { get; set; }
		[JsonProperty("latitude")]
		public string Latitude { get; set; }
	}

	public class RestAPIResponseModel 
	{
		[JsonProperty("ip")]
		public IpInfo Ip { get; set; }
	}
}
