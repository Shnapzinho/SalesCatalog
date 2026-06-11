namespace SalesCatalog.Entity
{
	public class Product
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public decimal Price { get; set; }
		public string Url { get; set; }
		public string ImageUrl { get; set; }
		public string DiscountPercent { get; set; }
		public decimal? OldPrice { get; set; }

	}
}
