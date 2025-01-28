using System.Text.Json;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;

namespace BudgetPlanner.Client.Services;

public class CookieStorageAccessor
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    private readonly IJSRuntime _jsRuntime;

    public CookieStorageAccessor(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }


    public async Task<CredentialDTO> GetValueAsync(string key)
    {
        await WaitForReference();
        var jsonResult = await _accessorJsRef.Value.InvokeAsync<string>("get", key);

        if (jsonResult.StartsWith($"{key}="))
        {
            // Remove the key and "=" to isolate the JSON value
            jsonResult = jsonResult.Substring(key.Length + 1);
        }

        var result = JsonSerializer.Deserialize<CredentialDTO>(jsonResult);
        
        return result;
    }

    public async Task SetValueAsync<T>(string key, CredentialDTO value)
    {
        await WaitForReference();
        string jsonValue = JsonSerializer.Serialize(value);
        await _accessorJsRef.Value.InvokeVoidAsync("set", key, jsonValue);
    }


    //TODO: Måste fixas
    public async Task DeleteValueAsync(string key)
    {
        await WaitForReference();
        try
        {
            await _accessorJsRef.Value.InvokeVoidAsync("remove", key);
        }
        catch (Exception e)
        {
            Console.WriteLine($"EEEEEEEEEERRRRRRROOOOOOOOOOOOOOORRRRRR:  {e}");
            throw;
        }
        
    }

    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/CookieStorageAccessor.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }



}