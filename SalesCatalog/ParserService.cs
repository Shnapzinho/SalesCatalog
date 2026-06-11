using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SalesCatalog.Entity;
using System.Globalization;
using System.Net;

namespace SalesCatalog
{
	public class ParserService
	{
		private HttpClient _httpClient;
		private string _baseUrl = "https://5element.by";

		public ParserService()
		{
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
		}

		public async Task<List<Product>> GetProductsAsync()
		{
			List<Product> products = new List<Product>();
			int page = 1;
			string firstIdOnPrevPage = null;
			while (true)
			{
				string url = $"{_baseUrl}/action/17-skidki-i-promokody?page={page}";
				List<Product> pageProducts = await GetPageProductsAsync(url);
				if (pageProducts.Count == 0)
					break;
				string firstIdOnCurPage = pageProducts[0].Id;
				if (firstIdOnCurPage == firstIdOnPrevPage)
					break;
				firstIdOnPrevPage = firstIdOnCurPage;
				products.AddRange(pageProducts);
				page++;
				await Task.Delay(300);

		}
			return products;
		}

		public async Task<List<Product>> GetPageProductsAsync(string url)
		{
			List<Product> products = new List<Product>();
			try
			{
				var html = await _httpClient.GetStringAsync(url);
				var parser = new HtmlParser();
				var document = await parser.ParseDocumentAsync(html);

				var items = document.QuerySelectorAll(".c-list__item");

				foreach (var item in items)
				{
					var discountElement = item.QuerySelector(".p-price__discount");
					var oldPriceElement = item.QuerySelector(".p-price__old");

					if (discountElement == null && oldPriceElement == null)
					{
						continue;
					}

					var rawJson = item.GetAttribute("data-product");
					if (string.IsNullOrEmpty(rawJson)) 
						continue;

					var decodedJson = WebUtility.HtmlDecode(rawJson);
					var jsonData = JsonConvert.DeserializeObject<dynamic>(decodedJson);

					var linkElement = item.QuerySelector("a[href*='/products/']");
					var imgElement = item.QuerySelector("img.swiper-lazy, .c-list__img img");

					var product = new Product
					{
						Id = jsonData.id,
						Name = jsonData.name,
						Category = jsonData.category_name,
						Price = jsonData.price,
						Url = _baseUrl + linkElement?.GetAttribute("href"),
						ImageUrl = imgElement?.GetAttribute("src") ?? imgElement?.GetAttribute("data-src"),
						DiscountPercent = discountElement?.TextContent.Trim()
					};

					if (oldPriceElement != null)
					{
						string rawPrice = oldPriceElement.TextContent.Trim();
						string cleanPrice = new string(rawPrice.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray()).Replace(",", ".");
						if (decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal op))
							product.OldPrice = op;
					}

					products.Add(product);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при работе с anglesharp: {ex.Message}");
			}

			return products;
		}
	}
}
