using Microsoft.Extensions.Caching.Memory;
using SalesCatalog.Entity;

namespace SalesCatalog.Service
{
    public class ProductAggregatorService
    {
        private readonly ParserService _parserService;
        private readonly ElectrosilaParser _electrosilaParser;
        private readonly MtsParser _mtsParser;
        private readonly IMemoryCache _cache;
        public ProductAggregatorService(ParserService parserService, ElectrosilaParser electrosilaParser, MtsParser mtsParser, IMemoryCache cache)
        {
            _parserService = parserService;
            _electrosilaParser = electrosilaParser;
            _mtsParser = mtsParser;
            _cache = cache;
        }
        public async Task<List<Product>> GetAllProductsAsync()
        {
            if (!_cache.TryGetValue("allProducts", out List<Product> allProducts))
            {
                allProducts = (await _parserService.GetProductsAsync()).Concat(await _electrosilaParser.GetProductsAsync()).Concat(await _mtsParser.ParseProductsAsync()).GroupBy(x => x.Id).Select(x => x.First()).ToList();
                _cache.Set("allProducts", allProducts, TimeSpan.FromHours(1));
            }
            return allProducts;
        }
    }
}
