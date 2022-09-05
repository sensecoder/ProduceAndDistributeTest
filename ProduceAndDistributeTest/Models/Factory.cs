namespace ProduceAndDistributeTest.Models
{
    public class Factory
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public Product Product { get; set; }
        public int ProducedByHour { get; set; }
        public Factory(string name, Product product, int producedByHour)
        {
            Name = name;
            Product = product;
            ProducedByHour = producedByHour;
        }
    }
}
