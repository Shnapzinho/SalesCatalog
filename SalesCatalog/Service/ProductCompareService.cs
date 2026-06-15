using Microsoft.Extensions.Caching.Memory;
using SalesCatalog.Entity;

namespace SalesCatalog.Service
{
	public class ProductCompareService
	{
		private readonly ParserService _parserService;
		private readonly ElectrosilaParser _electrosilaParser;
		private readonly MtsParser _mtsParser;
		private readonly IMemoryCache _cache;
		public ProductCompareService(ParserService parserService, ElectrosilaParser electrosilaParser, MtsParser mtsParser, IMemoryCache cache) 
		{
			_parserService = parserService;
			_electrosilaParser = electrosilaParser;
			_mtsParser = mtsParser;
			_cache = cache;
		}
		public async Task<PagedResult<Product>> GetCompareResultAsync (string? category, string? name, SortType? sortingType, int page, int pageSize = 20)
		{
			if (!_cache.TryGetValue("allProducts", out List<Product> allProducts))
			{ 
				allProducts = (await _parserService.GetProductsAsync()).Concat(await _electrosilaParser.GetProductsAsync()).Concat(await _mtsParser.ParseProductsAsync()).ToList();
				_cache.Set("allProducts", allProducts, TimeSpan.FromHours(1));
			}
			if (category != null)
				allProducts = allProducts.Where(x => x.Category?.Contains(category, StringComparison.OrdinalIgnoreCase) == true).ToList();
			if (name != null)
				allProducts = allProducts.Where(x => x.Name?.Contains(name, StringComparison.OrdinalIgnoreCase) == true).ToList();
			switch (sortingType)
			{
				case SortType.ByPrice:
					{
						allProducts = allProducts.OrderBy(x => x.Price).ToList();
						foreach (var product in allProducts.Take(3))
							product.IsBestDeal = true;
						break;
					}
				case SortType.ByDiscount:
					{
						allProducts = allProducts.OrderByDescending(x =>
						int.TryParse(new string (x.DiscountPercent.Where(char.IsDigit).ToArray()), out int discount) ? discount : 0).ToList();
						foreach (var product in allProducts.Take(3))
							product.IsBestDeal = true;
						break;
					}
			}

			return new PagedResult<Product>
			{
				Items = allProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
				Page = page,
				PageSize = pageSize,
				TotalItems = allProducts.Count
			};
		}
	}
}