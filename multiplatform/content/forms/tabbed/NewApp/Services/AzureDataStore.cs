using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
#if (IncludeXamarinEssentials)
using Xamarin.Essentials;
#endif
using NewApp.Models;

namespace NewApp.Services
{
    public class AzureDataStore : IDataStore<Item>
    {
        HttpClient client;
        IEnumerable<Item> items;

        public AzureDataStore()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri($"{App.AzureBackendUrl}/");

            items = new List<Item>();
        }

#if (IncludeXamarinEssentials)
		bool IsConnected => Connectivity.NetworkAccess != NetworkAccess.Internet;
#endif
		public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
		{
#if (IncludeXamarinEssentials)
			if (forceRefresh && IsConnected)
#else
			if (forceRefresh)
#endif
			{
				var json = await client.GetStringAsync($"api/item");
				items = await Task.Run(() => JsonConvert.DeserializeObject<IEnumerable<Item>>(json));
			}

            return items;
        }

		public async Task<Item> GetItemAsync(string id)
		{
#if (IncludeXamarinEssentials)
			if (id != null && IsConnected)
#else
			if (id != null)
#endif
			{
				var json = await client.GetStringAsync($"api/item/{id}");
				return await Task.Run(() => JsonConvert.DeserializeObject<Item>(json));
			}

            return null;
        }

		public async Task<bool> AddItemAsync(Item item)
		{
#if (IncludeXamarinEssentials)
			if (item == null || !IsConnected)
#else
			if (item == null)
#endif
				return false;

            var serializedItem = JsonConvert.SerializeObject(item);

            var response = await client.PostAsync($"api/item", new StringContent(serializedItem, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

		public async Task<bool> UpdateItemAsync(Item item)
		{
#if (IncludeXamarinEssentials)
			if (item == null || item.Id == null || !IsConnected)
#else
			if (item == null || item.Id == null)
#endif
				return false;

            var serializedItem = JsonConvert.SerializeObject(item);
            var buffer = Encoding.UTF8.GetBytes(serializedItem);
            var byteContent = new ByteArrayContent(buffer);

            var response = await client.PutAsync(new Uri($"api/item/{item.Id}"), byteContent);

            return response.IsSuccessStatusCode;
        }

		public async Task<bool> DeleteItemAsync(string id)
		{
#if (IncludeXamarinEssentials)
			if (string.IsNullOrEmpty(id) && !IsConnected)
#else
			if (string.IsNullOrEmpty (id))
#endif
				return false;

            var response = await client.DeleteAsync($"api/item/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}