namespace ObjectInitializer
{
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public static class Store
    {
        public static Product CreateProduct()
        {
            return new Product
            {
                Name = "Widget",
                Price = 9.99m,
                Quantity = 100
            };
        }

        public static decimal GetTotal(Product p)
        {
            return p.Price * p.Quantity;
        }
    }
}
