﻿using CatStealer.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CatStealer.Infrastructure.Services
{
    public interface ICatApiClient
    {
        Task<List<CatApiResponse>> FetchCatsAsync();
        Task<byte[]> FetchImageAsync(string url);
    }

    public class CatApiClient : ICatApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatApiClient> _logger;
        private readonly CatsApiSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public CatApiClient(
            IHttpClientFactory httpClientFactory,
            ILogger<CatApiClient> logger,
            IOptions<CatsApiSettings> settings,
            IOptions<JsonSerializerOptions> jsonOptions)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _settings = settings.Value;
            _jsonOptions = jsonOptions.Value;

            _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        }

        public async Task<List<CatApiResponse>> FetchCatsAsync()
        {
            var cats = new List<CatApiResponse>();

            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/images/search?has_breeds=1&limit={_settings.FetchCount}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                cats = JsonSerializer.Deserialize<List<CatApiResponse>>(content, _jsonOptions);
            }
            else
            {
                _logger.LogWarning($"Failed to fetch cat data. Status code: {response.StatusCode}");
            }

            return cats;
        }

        public async Task<byte[]> FetchImageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                _logger.LogWarning($"Failed to fetch image. Status code: {response.StatusCode}");
                return null;
            }
        }
    }

    public class CatApiResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("breeds")]
        public List<Breed> Breeds { get; set; }
    }

    public class Breed
    {
        [JsonPropertyName("temperament")]
        public string Temperament { get; set; }
    }
}