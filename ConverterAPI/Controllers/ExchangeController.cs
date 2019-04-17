using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace ConverterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private IMemoryCache _cache;
        private static readonly HttpClient HttpClient;

        static ExchangeController()
        {
            HttpClient = new HttpClient();
        }

        public ExchangeController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        // GET api/Exchange
        [HttpGet]
        [Route("{from}/{to}/{value}")]
        public async Task<IActionResult> Exchange(string from, string to, int value)
        {
            double cacheResult;
            //string fromUpper = from.ToUpper();
            //string toUpper = to.ToUpper();
            HttpResponseMessage responseMessage = await HttpClient.GetAsync($"https://api.exchangeratesapi.io/latest?base={from}&symbols={to}");
            responseMessage.EnsureSuccessStatusCode();
            string responseBody = await responseMessage.Content.ReadAsStringAsync();

            if(!_cache.TryGetValue(_cache, out cacheResult))
            {
                var options = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _cache.Set(_cache, cacheResult, options);
            }

            JObject jObject = JObject.Parse(responseBody);
            var rate = jObject["rates"][to].ToString();
            var result = Math.Truncate((Double.Parse(rate) * value) * 1000) / 1000;

            return Ok(result);
        }
    }
}
