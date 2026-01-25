using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using WebApp.Shared.Model;

namespace WebApp.Client.Services;

public class ClientAuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtAuthStateProvider _authStateProvider;

    private const string TokenKey = "authToken";

    public ClientAuthService(
        HttpClient http,
        ILocalStorageService localStorage,
        JwtAuthStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<ServiceResponse<string>> Login(string email, string password)
    {
        var request = new { Email = email, Password = password };

        var response = await _http.PostAsJsonAsync("api/auth/login", request);

        // Read raw body once
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ServiceResponse<string>? error = null;
            try
            {
                error = JsonSerializer.Deserialize<ServiceResponse<string>>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
            }

            return error ?? new ServiceResponse<string>
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(raw)
                    ? "Login failed."
                    : raw
            };
        }

        // Success path – log raw JSON first if something goes wrong
        ServiceResponse<string>? result = null;
        try
        {
            result = JsonSerializer.Deserialize<ServiceResponse<string>>(raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = $"Could not parse JSON: {raw}"
            };
        }

        if (result == null)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = $"Null result. Raw: {raw}"
            };
        }

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            // Show what came back if Data is empty
            return new ServiceResponse<string>
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(result.Message)
                    ? $"No token in response. Raw: {raw}"
                    : result.Message
            };
        }

        var token = result.Data;

        await _localStorage.SetItemAsync(TokenKey, token);
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        await _authStateProvider.MarkUserAsAuthenticated(token);

        return result;
    }

    public async Task<string?> GetTokenAsync() =>
        await _localStorage.GetItemAsync<string>(TokenKey);

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        _http.DefaultRequestHeaders.Authorization = null;
        _authStateProvider.MarkUserAsLoggedOut();
    }
}
