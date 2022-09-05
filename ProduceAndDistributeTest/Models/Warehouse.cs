namespace ProduceAndDistributeTest.Models
{
    
    public class Warehouse
    {
        static object IncomingLocker = new();
        static object ShipperLocker = new();
        static object QuantityChangeLocker = new();
        public int Capacity { get; set; }
        public DeliveryService? Delivery { get; set; }
        public Dictionary<Product, int> QuantityOfProducts { get; private set; }
        public List<StockRecord> IncomingRecords { get; private set; }
        public void AddToWarehouse(object Production)
        {
            StockRecord production = (StockRecord) Production;
           
            lock (IncomingLocker)
            {
                // Защита от переполнения склада
                while ((QuantityOfProducts.Values.Sum() + production.Quantity) > Capacity)
                {
                    Thread.Sleep(1);
                }
                production.Pos = IncomingRecords.Count + 1;
                IncomingRecords.Add(production);
                IncreaseStock(production);
                CheckWarehouseFullness();
            }
        }
        public bool ShipFromWarehouse(StockRecord Production)
        {
            bool result = false;
            lock (ShipperLocker)
            {
                if(QuantityOfProducts.TryGetValue(Production.ProductData, out var quantity))
                {
                    if (quantity > Production.Quantity)
                    {
                        DecreaseStock(Production);
                        result = true;
                    }
                }                
            }
            return result;
        }
        private void IncreaseStock(StockRecord production)
        {
            lock (QuantityChangeLocker)
            {
                int quantity = 0;
                if (!QuantityOfProducts.TryGetValue(production.ProductData, out quantity))
                {
                    QuantityOfProducts.Add(production.ProductData, production.Quantity);
                }
                QuantityOfProducts[production.ProductData] = quantity + production.Quantity;
            }
        }
        private void DecreaseStock(StockRecord production)
        {
            lock (QuantityChangeLocker)
            {
                int quantity = 0;
                if (QuantityOfProducts.TryGetValue(production.ProductData, out quantity))
                {
                    QuantityOfProducts[production.ProductData] = quantity - production.Quantity;
                }
            }   
        }
        private void CheckWarehouseFullness()
        {
            int fullness = QuantityOfProducts.Values.Sum();
            if (fullness > (0.95 * Capacity) && Delivery != null)
            {
                Delivery.PendingDistribute();
            }          
        }
        public Warehouse()
        {
            IncomingRecords = new List<StockRecord>();
            QuantityOfProducts = new Dictionary<Product, int>();
        }
    }

    public struct StockRecord
    {
        public int Pos { get; set; }
        public Product ProductData { get; set; }
        public int Quantity { get; set; }
    }
}
