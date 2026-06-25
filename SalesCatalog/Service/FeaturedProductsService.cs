using SalesCatalog.Entity;
namespace SalesCatalog.Service
{
    public class FeaturedProductsService
    {
        private readonly ProductAggregatorService _productAggregatorService;
        public FeaturedProductsService (ProductAggregatorService productAggregatorService)
        {
            _productAggregatorService = productAggregatorService;
        }

        public async Task<List<Product>> GetFeaturedProductsAsync()
        {
           return Random.Shared.GetItems((await _productAggregatorService.GetAllProductsAsync()).ToArray(), 16).ToList();
        }
    }
}
