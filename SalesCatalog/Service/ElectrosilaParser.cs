using AngleSharp.Html.Parser;
using SalesCatalog.Entity;
using System.Globalization;
using System.Text;

namespace SalesCatalog.Service
{
	public class ElectrosilaParser
	{
		private HttpClient _httpClient;
		private string _baseUrl = "https://sila.by";

		public ElectrosilaParser()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
		}

		private async Task<string> GetHtmlAsync(string url)
		{
			var bytes = await _httpClient.GetByteArrayAsync(url);
			return Encoding.GetEncoding("windows-1251").GetString(bytes);
		}

		public async Task<List<Product>> GetProductsAsync()
		{
			List<Product> allProducts = new List<Product>();
			string url = $"{_baseUrl}/skidki";
			try
			{
				var html = await GetHtmlAsync(url);
				var parser = new HtmlParser();
				var document = await parser.ParseDocumentAsync(html);

				var categoryLinks = document.QuerySelectorAll(".promo .label_visible a").Select(a => new
				{
					Url = a.GetAttribute("href"),
					Name = a.TextContent.Trim()
				})
				.Where(x => !string.IsNullOrEmpty(x.Url)).ToList();

				foreach (var category in categoryLinks)
				{
					var categoryProducts = await ParseCategoryAsync(category.Url, category.Name);
					allProducts.AddRange(categoryProducts);
					await Task.Delay(300);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при работе с anglesharp: {ex.Message}");
			}
			return allProducts;
		}

		public async Task<List<Product>> ParseCategoryAsync (string url, string categoryName)
		{
			List<Product> products = new List<Product>();
			int page = 1;
			string firstIdOnPrevPage = null;

			while (true)
			{
				string pageUrl = page == 1 ? url : $"{url}/{page}";
				List<Product> pageProducts = await ParsePageAsync(pageUrl, categoryName);
				if (pageProducts.Count == 0)
					break;
				string firstIdOnCurPage = pageProducts[0].Id;
				if (firstIdOnCurPage == firstIdOnPrevPage)
					break;
				firstIdOnPrevPage = firstIdOnCurPage;
				products.AddRange(pageProducts);
				page++;
			}
			return products;
		}

		public async Task<List<Product>> ParsePageAsync(string url, string categoryName)
		{
			var products = new List<Product>();
			try
			{
				var html = await GetHtmlAsync(url);
				var parser = new HtmlParser();
				var document = await parser.ParseDocumentAsync(html);
				var items = document.QuerySelectorAll(".tov_prew");
				foreach (var item in items)
				{
					try
					{
						string discountElement = item.QuerySelector("p")?.TextContent.Trim() ?? "";
						if (string.IsNullOrEmpty(discountElement))
							continue;
						var oldPriceElements = item.QuerySelectorAll(".price s b").ToList();
						if (oldPriceElements.Count < 2)
							continue;
						var priceElements = item.QuerySelectorAll(".price div b").ToList();
						var imgElement = item.QuerySelector("img");
						string nameElement = imgElement?.GetAttribute("title") ?? imgElement?.GetAttribute("alt") ?? "Без названия";

						string digits = new string(discountElement.Where(char.IsDigit).ToArray());
						string discountPercent = $"-{digits}%";
						var product = new Product
						{
							Id = item.GetAttribute("data-idabc"),
							Name = nameElement.Trim(),
							Category = categoryName,
							Url = item.QuerySelector("a")?.GetAttribute("href"),
							ImageUrl = imgElement?.GetAttribute("src"),
							DiscountPercent = discountPercent,
							Shop = "Электросила"
						};
						if (priceElements.Count >= 2)
						{
							string rub = new string(priceElements[0].TextContent
								.Where(char.IsDigit).ToArray());
							string kop = new string(priceElements[1].TextContent
								.Where(char.IsDigit).ToArray()).PadLeft(2, '0');

							if (decimal.TryParse($"{rub}.{kop}",
								NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
								product.Price = price;
						}
						string oldRub = new string(oldPriceElements[0].TextContent
							.Where(char.IsDigit).ToArray());
						string oldKop = new string(oldPriceElements[1].TextContent
							.Where(char.IsDigit).ToArray()).PadLeft(2, '0');

						if (decimal.TryParse($"{oldRub}.{oldKop}",
							NumberStyles.Any, CultureInfo.InvariantCulture, out decimal oldPrice))
							product.OldPrice = oldPrice;
						products.Add(product);
						Console.WriteLine($"Добавлен продукт {product.Name}. Магазин {product.Shop}");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Ошибка в категории {categoryName}: {ex.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка страницы {url}: {ex.Message}");
			}
			return products;
		}
	}
}