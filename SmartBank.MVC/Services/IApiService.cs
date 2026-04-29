namespace SmartBank.MVC.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string? token = null);
        Task<T?> PostAsync<T>(string endpoint, object body, string? token = null);
        Task<T?> PutAsync<T>(string endpoint, object body, string? token = null);
    }
}