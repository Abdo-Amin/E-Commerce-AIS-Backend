namespace Cart.Api.Services;

public class ProductCatalogItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public List<ProductCatalogImage> Images { get; set; } = [];
}

public class ProductCatalogImage
{
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}
