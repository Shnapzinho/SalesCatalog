using Microsoft.AspNetCore.Mvc;
using SalesCatalog.Entity; 

namespace SalesCatalog.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsController : ControllerBase
	{
		private readonly ParserService _parserService;
		private readonly ElectrosilaParser _electrosilaParser;
		private readonly MtsParser _mtsParser;

		public ProductsController(ParserService parserService, ElectrosilaParser electrosilaParser, MtsParser mtsParser)
		{
			_parserService = parserService;
			_electrosilaParser = electrosilaParser;
			_mtsParser = mtsParser;
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
	}
}
