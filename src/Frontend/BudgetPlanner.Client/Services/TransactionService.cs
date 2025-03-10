using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services
{
    public class TransactionService(IHttpClientFactory factory) : IRepository<TransactionDTO>
    {
        private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

        public async Task<string> AddAsync(TransactionDTO item, string uId)
        {
            var response = await _httpClient.PostAsJsonAsync($"/transaction/{uId}", item);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("failed to post");
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<TransactionDTO>> GetAllAsync(string uId)
        {
            var response = await _httpClient.GetAsync($"transaction/{uId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<List<TransactionDTO>>();
        }

        public async Task<TransactionDTO> GetByIdAsync(string docId, string uId)
        {
            var response = await _httpClient.GetAsync($"transaction/{docId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
            }
            return await response.Content.ReadFromJsonAsync<TransactionDTO>();
        }

        public async Task RemoveAsync(string docId, string uId)
        {
            var response = await _httpClient.DeleteAsync($"/transaction/{docId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("failed to delete");
            }
        }

        public async Task UpdateAsync(TransactionDTO item, string docId, string uId)
        {
            var response = await _httpClient.PutAsJsonAsync($"/transaction/{docId}", item);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("failed to post");
            }
        }
    }
