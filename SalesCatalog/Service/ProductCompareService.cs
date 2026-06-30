using Microsoft.Extensions.Caching.Memory;
using SalesCatalog.Entity;

namespace SalesCatalog.Service
{
	public class ProductCompareService
	{
		private readonly ProductAggregatorService _productAggregatorService;
		public ProductCompareService(ProductAggregatorService productAggregatorService)
		{
			_productAggregatorService = productAggregatorService;
		}
		public async Task<PagedResult<Product>> GetCompareResultAsync(string? category, string? name, SortType sortingType, int page, int pageSize = 20)
		{
			List<Product> allProducts = await _productAggregatorService.GetAllProductsAsync();
			if (category != null)
				allProducts = allProducts.Where(x => x.Category?.Contains(category, StringComparison.OrdinalIgnoreCase) == true).ToList();
			if (name != null)
				allProducts = allProducts.Where(x => x.Name?.Contains(name, StringComparison.OrdinalIgnoreCase) == true).ToList();
			foreach (var product in allProducts)
				product.IsBestDeal = false;
			switch (sortingType)
			{
				case SortType.ByPrice:
					{
						allProducts = allProducts.OrderBy(x => x.Price).ToList();
						break;
					}
				case SortType.ByDiscount:
					{
						allProducts = allProducts.OrderByDescending(x =>
						int.TryParse(new string(x.DiscountPercent.Where(char.IsDigit).ToArray()), out int discount) ? discount : 0).ToList();
						break;
					}
				case SortType.ByDifference:
					{
						allProducts = allProducts.OrderByDescending(x => x.OldPrice - x.Price).ToList();
						break;
					}
			}
			foreach (var product in allProducts.Take(3))
				product.IsBestDeal = true;

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