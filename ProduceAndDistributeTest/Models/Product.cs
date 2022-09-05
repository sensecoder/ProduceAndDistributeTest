using System.Diagnostics.CodeAnalysis;

namespace ProduceAndDistributeTest.Models
{
    public class Product : IEqualityComparer<Product>
    {
        public string Factory { get; set; }
        public string Name { get; set; }
        public int Weight { get; set; }
        public int PackageType { get; set; }
        public Product(string factory, string name, int weight, int packageType)
        {
            Factory = factory;
            Name = name;
            Weight = weight;
            PackageType = packageType;
        }

        public bool Equals(Product? x, Product? y)
        {
            return x?.Name == y?.Name ? true : false;
        }

        public int GetHashCode([DisallowNull] Product obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
