using AngleSharp.Html.Parser;
using SalesCatalog.Entity;
using System.Globalization;

namespace SalesCatalog.Service
{
	public class MtsParser
	{
		private HttpClient _httpClient;
		private string _baseUrl = "https://shop.mts.by";

		public MtsParser()
		{
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
		}

		//public async Task<List<(string url, string category)>> GetCategoriesAsync()
		//{
		//	var html = await _httpClient.GetStringAsync(_baseUrl);
		//	var parser = new HtmlParser();
		//	var document = await parser.ParseDocumentAsync(html);
		//	return document.QuerySelectorAll(".menu-list > .menu-list__item > a.menu-link")
		//	.Select(a => (Url: a.GetAttribute("href"), Category: a.QuerySelector(".menu-link__text")?.TextContent.Trim()))
		//	.Where(x => !string.IsNullOrEmpty(x.Url) && !string.IsNullOrEmpty(x.Category) &&
		//	x.Url != "/news-actions/" &&
		//	x.Url != "/certificate/" &&
		//	x.Url != "/tariffs/" &&
		//	x.Url != "/top-10/" &&
		//	x.Url != "/vacancy/" &&
		//	x.Url != "/terminals/"
		//	)
		//	.ToList();
		//}

		private readonly List<(string Url, string Category)> _categories = new()
		{
			("/phones/",                "Смартфоны"),
			("/tabletpc/",              "Планшеты и ноутбуки"),
			("/for-home/",              "Товары для дома"),
			("/tv/tv2/",                "Телевизоры"),
			("/gadgets/",               "Гаджеты"),
			("/tv/",                    "Видеоигры"),
			("/accessories/",           "Аксессуары"),
		};

		public async Task<List<Product>> ParseProductsAsync()
		{
			List<Product> allProducts = new List<Product>();
			////var categories = await GetCategoriesAsync();
			//foreach (var (url,category) in categories) 
			//{
			//	List <Product> products= await ParseCategoryAsync(url, category);
			//	allProducts.AddRange(products);
			//	await Task.Delay(500);
			//}
			foreach (var (url, category) in _categories)
			{
				List<Product> products = await ParseCategoryAsync(url, category);
				allProducts.AddRange(products);
				await Task.Delay(300);
			}
			return allProducts;
		}

		public async Task<List<Product>> ParseCategoryAsync(string categoryUrl, string category)
		{
			List<Product> products = new List<Product>();
			int page = 1;
			string firstIdOnPrevPage = null;

			while (true)
			{
				string url = $"{_baseUrl}{categoryUrl}?page={page}";
				List<Product> pageProducts = await ParsePageAsync (url, category);
				if (pageProducts.Count == 0)
					break;
				string firstIdOnCurPage = pageProducts[0].Id;
				if (firstIdOnCurPage == firstIdOnPrevPage)
					break;
				firstIdOnPrevPage = firstIdOnCurPage;
				products.AddRange(pageProducts);
				Console.WriteLine($"Обработана категория {category}");
				page++;
			}
			return products;
		}

		public async Task<List<Product>> ParsePageAsync(string url, string category)
		{
			List<Product > products = new List<Product>();
			try
			{
				var html = await _httpClient.GetStringAsync(url);
				var parser = new HtmlParser();
				var document = await parser.ParseDocumentAsync(html);
				var items = document.QuerySelectorAll("article.card-product");
				foreach (var item in items)
				{
					try
					{
						var oldPriceElement = item.QuerySelector(".card-product__price--old .num");
						if (oldPriceElement == null)
							continue;
						var priceElement = item.QuerySelector(".card-product__price:not(.card-product__price--old):not(.card-product__price--red) .num");
						string priceText = priceElement?.TextContent.Trim() ?? "0";
						string oldPriceText = oldPriceElement.TextContent.Trim();
						decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);
						decimal.TryParse(oldPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal oldPrice);
						string discountPercent = oldPrice > 0
							? $"-{(int)Math.Round((1 - price / oldPrice) * 100)}%"
							: "";
						if (discountPercent == "")
							continue;
						var imgElement = item.QuerySelector(".card-product__slider img");
						var linkElement = item.QuerySelector(".card-product__link");
						var product = new Product
						{
							Id = item.GetAttribute("data-ga-item-id"),
							Name = item.QuerySelector(".card-product__title")?.TextContent.Trim(),
							Category = item.QuerySelector(".card-product__category")?.TextContent.Trim() ?? category,
							Price = price,
							OldPrice = oldPrice,
							DiscountPercent = discountPercent,
							Url = _baseUrl + linkElement?.GetAttribute("href"),
							ImageUrl = _baseUrl + imgElement?.GetAttribute("src"),
							Shop = "Мтс"
						};
						products.Add(product);
						Console.WriteLine($"Добавлен продукт {product.Name}");
					}
					catch(Exception ex) 
					{
						Console.WriteLine($"Ошибка в категории {category}: {ex.Message}");
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
