using Microsoft.AspNetCore.Mvc;
using SalesCatalog.Entity;
using SalesCatalog.Service;

namespace SalesCatalog.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsController : ControllerBase
	{
		private readonly ParserService _parserService;
		private readonly ElectrosilaParser _electrosilaParser;
		private readonly MtsParser _mtsParser;
		private readonly ProductCompareService _productCompareService;
		private readonly ProductAggregatorService _productAggregatorService;
		private readonly FeaturedProductsService _featuredProductsService;

		public ProductsController(ParserService parserService, ElectrosilaParser electrosilaParser, MtsParser mtsParser, ProductCompareService productCompareService, ProductAggregatorService productAggregatorService, FeaturedProductsService featuredProductsService)
		{
			_parserService = parserService;
			_electrosilaParser = electrosilaParser;
			_mtsParser = mtsParser;
			_productCompareService = productCompareService;
			_productAggregatorService = productAggregatorService;
			_featuredProductsService = featuredProductsService;
		}
		[HttpGet("five-element")]
		public async Task<ActionResult<List<Product>>> GetFiveElementProducts()
		{
			var products = await _parserService.GetProductsAsync();

			if (products == null || products.Count == 0)
				return NotFound("Товары не найдены или возникла ошибка при парсинге.");
			return Ok(products);
		}
		[HttpGet("electrosila")]
		public async Task<ActionResult<List<Product>>> GetEletrosilaProducts()
		{
			var products = await _electrosilaParser.GetProductsAsync();
			if (products == null || products.Count == 0)
				return NotFound("Товары не найдены или возникла ошибка при парсинге.");
			return Ok(products);
		}
		[HttpGet("Mts")]
		public async Task<ActionResult<List<Product>>> GetMtsProducts()
		{
			var products = await _mtsParser.ParseProductsAsync();
			if (products == null || products.Count == 0)
				return NotFound("Товары не найдены или возникла ошибка при парсинге.");
			return Ok(products);
		}
		[HttpGet("Compare")]
		public async Task<ActionResult<PagedResult<Product>>> GetCompareResult([FromQuery] string? category, [FromQuery] string? name, [FromQuery] SortType sortingType, [FromQuery] int page)
		{
			var compareResult = await _productCompareService.GetCompareResultAsync(category, name, sortingType, page);
			if (compareResult == null || compareResult.Items.Count == 0)
				return NotFound("Товары не найдены или возникла ошибка при парсинге.");
			return Ok(compareResult);
		}
		[HttpGet("Main")]
		public async Task<ActionResult<List<Product>>> GetFeaturedProducts()
		{
			var products = await _featuredProductsService.GetFeaturedProductsAsync();
			if (products == null || products.Count == 0)
				return NotFound("Товары не найдены или возникла ошибка при парсинге.");
			return Ok(products);
		}
		[HttpGet("Categories")]
		public async Task<ActionResult<List<string>>> GetCategories()
		{
			var categories = await _productAggregatorService.GetCategoriesAsync();
			return Ok(categories);
		}
	}
}
