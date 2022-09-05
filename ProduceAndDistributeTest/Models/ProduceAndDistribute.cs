using System.Linq;

namespace ProduceAndDistributeTest.Models
{
    public class ProduceAndDistribute
    {
        private static ProduceAndDistribute? _instance;
        public static ProduceAndDistribute? Instance(Setup? setup = null)
        {
            if (setup != null)
            {
                _instance = new ProduceAndDistribute(setup);
                //Console.WriteLine("New instance of PnD created!");
            }
            GC.Collect();
            return _instance;
        }
        private ProduceAndDistribute(Setup setup)
        {
            _setup = setup;
            _factories = new List<Factory>();
            CreateEntities();
        }
        public Task StartProcess(int Duration)
        {         
            for (int i = 0; i < Duration; i++)
            {
                foreach (var factory in _factories)
                {                    
                    StockRecord record = new StockRecord
                    {
                        ProductData = factory.Product,
                        Quantity = factory.ProducedByHour
                    };
                    ThreadPool.QueueUserWorkItem(_warehouse.AddToWarehouse, record);
                }
            }
            while (ThreadPool.PendingWorkItemCount != 0)
            {
                Thread.Sleep(0);
            }
            Console.WriteLine("Все товары произведены и отправлены на склад!");
            DateTime now = DateTime.Now;
            DateTime now2;
            while (_deliveryService.IsBusy())
            {
                Thread.Sleep(0);
            }
            if (!_deliveryService.IsBusy())
            {
                now2 = DateTime.Now;
                var ticks = (now2 - now).Ticks;
                Console.WriteLine($"Доставка закончилась через: {ticks} тиков");
            }
            return Task.CompletedTask;
        }
        public List<StockRecord>GetWarehouseIncome(int page = 0, int itemsOnPage = 100)
        {
            return _warehouse.IncomingRecords
                .Skip(itemsOnPage * page)
                .Take(itemsOnPage).ToList<StockRecord>();
        }
        public List<Truck> GetDeliveryStatistic()
        {
            var list = new List<Truck>();
            if (_deliveryService != null)
            {
                list = _deliveryService.DeliveryJournal.GroupBy(name => name.Name, param => (param.Cargo, param.Capacity),
                            (truckName, trucksEnum) => new Truck
                            {
                                Name = truckName,
                                Capacity = trucksEnum.Select(x => x.Capacity).First(),
                                Cargo = trucksEnum.SelectMany(Cargos => Cargos.Cargo)
                                        .GroupBy(cargo => cargo.ProductData, cargo => cargo.Quantity,
                                            (product, quantities) => new StockRecord
                                            {
                                                ProductData = product,
                                                Quantity = (int)quantities.Average()
                                            })
                                        .OrderBy(order => order.ProductData.Name).ToList()
                            }).OrderByDescending(order => order.Capacity).ToList();
            }
            return list;
        }

        private Setup _setup;
        private List<Factory> _factories;
        private Warehouse _warehouse;
        private DeliveryService _deliveryService;
        
        private void CreateEntities()
        {
            _factories.Clear();
            int summarFactoryProduce = 0;
            for (int i = 0; i < _setup.FactoriesCount; i++)
            {
                string factoryName = GetFactoryName(i);
                int factoryProduce = (int)((1 + 0.1 * i) * _setup.n);
                _factories.Add(
                    new Factory(factoryName, GetProduct(factoryName), factoryProduce)
                );
                summarFactoryProduce += factoryProduce;
            }
            _warehouse = new Warehouse();
            _deliveryService = new DeliveryService(_warehouse);
            _warehouse.Delivery = _deliveryService;
            _warehouse.Capacity = _setup.M * summarFactoryProduce;
            for (int i = 0; i < _setup.TrucksTypesCount; i++)
            {
                _deliveryService.CargoTrucks.Add(_setup.GetTruck(i));
            }
        }

        private Product GetProduct(string factoryName)
        {
            string factory = factoryName;
            string productName = factoryName.ToLower();
            Random random = new Random();
            int productWeight = random.Next(10, 100);
            int PackageType = random.Next(1, 10);
            return new Product(factory, productName, productWeight, PackageType);
        }

        private string GetFactoryName(int i)
        {
            char[] abc = ("ABCDEFGHIJKLMNOPQRSTUVWXYZ").ToCharArray();
            int abcIndx = i;
            int magnifier = 0;
            if (i >= abc.Length)
            {
                magnifier = (i - (i % (abc.Length - 1))) / (abc.Length - 1);
                abcIndx = i % (abc.Length - 1);
            }
            string factoryName = "";
            for (int j = 0; j <= magnifier; j++)
            {
                factoryName = factoryName + abc[abcIndx].ToString();
            }
            return factoryName;
        }
    }
}
