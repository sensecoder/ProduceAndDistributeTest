namespace ProduceAndDistributeTest.Models
{
    public class DeliveryService
    {
        static object PendingLocker = new();
        static object DeliveryJournalLocker = new();
        public List<Truck> CargoTrucks { get; set; }
        public List<Truck> DeliveryJournal { get; set; }

        private Warehouse _warehouse;
        private bool _inProcess;
        private int _pending;
        private List<Thread> _deliveryThreads;
        private Queue<Truck> _onLoadQueue = new Queue<Truck>();

        public DeliveryService(Warehouse warehouse)
        {
            CargoTrucks = new List<Truck>();
            DeliveryJournal = new List<Truck>();
            _deliveryThreads = new List<Thread>();
            _warehouse = warehouse;
            _inProcess = false;
            _pending = 0;
        }
        public void PendingDistribute()
        {
            lock (PendingLocker)
            {
                _pending++;
                if (!_inProcess)
                {
                    _inProcess = true;
                    DistributeStart();
                }
            }           
        }
        public bool IsBusy()
        {
            if (_pending > 0) return true;
            if (_inProcess) return true;
            JoinDeliveryThreads();
            CheckDeliveryThreads();
            return false;
        }

        private void DistributeStart()
        {
            // Снимок состояния склада на текущий момент:
            Dictionary<Product, int> stockImprint =
                new Dictionary<Product, int>(_warehouse.QuantityOfProducts);
            while (_pending > 0)
            {
                lock (PendingLocker)
                {
                    _pending--;
                }                
                if (OptimizeTrucksLoad(stockImprint))
                {
                    while (_onLoadQueue.Count > 0)
                    {
                        var truck = _onLoadQueue.Dequeue();
                        var faultList = new List<Product>();
                        Thread delivery = new Thread(() => { Delivery(truck, faultList); });                        
                        delivery.Start();
                        _deliveryThreads.Add(delivery);
                    }
                }
            }
            CheckDeliveryThreads();
            _inProcess = false;
        }
        private void Delivery(Truck truck, List<Product> faultShipList)
        {          
            foreach (var cargoPlace in truck.Cargo)
            {
                if (!_warehouse.ShipFromWarehouse(cargoPlace))
                {
                    faultShipList.Add(cargoPlace.ProductData);
                }
            }
            if (faultShipList.Count > 0)
                // Ошибка при отгрузке товара со склада:
                // Выгрузить загруженное обратно на склад и 
                // отменить рейс (не включать его в DeliveryJournal)
            {
                foreach (var cargoPlace in truck.Cargo)
                {
                    if (!faultShipList.Contains(cargoPlace.ProductData))
                    {
                        _warehouse.AddToWarehouse(cargoPlace);
                    }
                }
                return ;
            }
            lock (DeliveryJournalLocker)
            {
                DeliveryJournal.Add(truck);
            }            
        }
       
        private struct ProductShare
        {
            public Product ProductData;
            public double Value;
        }
        // Оптимальная загрузка грузовиков доставки из понимания эффективного складирования,
        // как складирования каждого вида продукции в одинаковых пропорциях.
        private bool OptimizeTrucksLoad(Dictionary<Product, int> StockImprint)
        {
            for (int j=0; j < CargoTrucks.Count; j++)
            {
                var truck = CargoTrucks[j].GetCopy();
                if (truck.Cargo != null)
                {
                    truck.Cargo.Clear();
                }
                else
                {
                    return false;
                }
                if (truck.Capacity < StockImprint.Values.Sum())
                {
                    // Расчёт загрузки грузовика из состояния на складе:
                    int minQuantity = StockImprint.Values.Min();
                    int[] reduceQuantities = new int[StockImprint.Count - 1];
                    ProductShare[] productShares = new ProductShare[StockImprint.Count - 1];
                    int indx = 0;
                    foreach (var productQuantityRec in StockImprint)
                    {
                        if (productQuantityRec.Value > minQuantity)
                        {
                            reduceQuantities[indx] = productQuantityRec.Value - minQuantity;
                            productShares[indx].ProductData = productQuantityRec.Key;
                            indx++;
                        }
                    }
                    int summarReduce = reduceQuantities.Sum();
                    for (int i = 0; i < productShares.Length; i++)
                    {
                        productShares[i].Value = (double)reduceQuantities[i] / summarReduce;
                    }

                    // Полная загрузка грузовика излишками продукции в соответствующих пропорциях
                    if (summarReduce > truck.Capacity)
                    {
                        for (int i = 0; i < productShares.Length; i++)
                        {
                            if (productShares[i].ProductData != null)
                            {
                                int loadQuantity = (int)Math.Round(truck.Capacity * productShares[i].Value);
                                truck.Cargo.Add(new StockRecord
                                {
                                    ProductData = productShares[i].ProductData,
                                    Quantity = loadQuantity
                                });
                                StockImprint[productShares[i].ProductData] =
                                    StockImprint[productShares[i].ProductData] - loadQuantity;
                            }
                        }
                        _onLoadQueue.Enqueue(truck);
                        summarReduce -= truck.Capacity;
                    }
                    // Все оставшиеся излишки загружаются в грузовик, остальное место заполняется
                    // всеми видами продукции в равных пропорциях до полного заполнения грузовика
                    else
                    {
                        int summarTruckCapacity = truck.Capacity;
                        // Загрузка излишков
                        if (summarReduce > 0)
                        {
                            for (int i = 0; i < productShares.Length; i++)
                            {
                                int quantity = (int)Math.Round(summarReduce * productShares[i].Value);
                                if (quantity > 0)
                                {
                                    truck.Cargo.Add(new StockRecord
                                    {
                                        ProductData = productShares[i].ProductData,
                                        Quantity = quantity
                                    });
                                    StockImprint[productShares[i].ProductData] =
                                    StockImprint[productShares[i].ProductData] - quantity;
                                }
                                summarTruckCapacity -= quantity;
                            }
                            summarReduce = 0;
                        }
                        // Загрузка всех видов продукции в равных долях
                        // на оставшееся место в грузовике.
                        // Здесь расчётное количество продукции каждого типа
                        // принимается за минимальное количество вычисленное ранее (minQuantity)
                        double share = 1.0 / StockImprint.Count;
                        int loadOnTruck = (int)(summarTruckCapacity * share); // Округляется в мин
                        foreach (var product in StockImprint)
                        {
                            if (minQuantity > loadOnTruck)
                            {
                                truck.Cargo.Add(new StockRecord
                                {
                                    ProductData = product.Key,
                                    Quantity = loadOnTruck
                                });
                                StockImprint[product.Key] =
                                    StockImprint[product.Key] - loadOnTruck;
                            }
                            // Недогруз грузовика означает отмену доставки
                            else
                            {
                                truck.Cargo.Clear();
                                break;
                            }
                            minQuantity = StockImprint.Values.Min();
                        }
                        // Подсчёт актуальной загрузки грузовика
                        int actualLoad = 0;
                        foreach (var place in truck.Cargo)
                        {
                            actualLoad += place.Quantity;
                        }
                        if (actualLoad > 0 && actualLoad < truck.Capacity)
                        {
                            int difference = truck.Capacity - actualLoad;
                            // Раскидать разницу в загрузке и вместимости по всем категориям продукции
                            // там должны быть единицы
                            foreach (var product in StockImprint)
                            {
                                if (difference > 0)
                                {
                                    truck.Cargo.Add(new StockRecord
                                    {
                                        ProductData = product.Key,
                                        Quantity = 1
                                    });
                                    StockImprint[product.Key] =
                                        StockImprint[product.Key] - 1;
                                }
                                difference--;
                            }
                        }
                        if (truck.Cargo.Count > 0)
                        {
                            minQuantity -= loadOnTruck;
                        }
                    }
                    if (truck.Cargo.Count > 0)
                    {
                        truck.Cargo = truck.Cargo.GroupBy(cargo => cargo.ProductData, cargo => cargo.Quantity,
                        (product, quantities) => new StockRecord
                        {
                            ProductData = product,
                            Quantity = quantities.Sum()
                        }).ToList();
                        _onLoadQueue.Enqueue(truck);
                    }
                }               
            }
            if (_onLoadQueue.Count == 0)
            {
                return false;
            }
            return true;
        }

        private void CheckDeliveryThreads()
        {
            int i = 0;
            while (i < _deliveryThreads.Count)
            {
                if (_deliveryThreads[i].ThreadState == ThreadState.Stopped)
                {
                    _deliveryThreads.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
        private void JoinDeliveryThreads()
        {
            int i = 0;
            while (i < _deliveryThreads.Count)
            {
                if (_deliveryThreads[i].ThreadState != ThreadState.Stopped)
                {
                    _deliveryThreads[i].Join();
                }
                else
                {
                    i++;
                }
            }
        }
    }
    public struct Truck
    {
        public string ?Name { get; set; }
        public int Capacity { get; set; } // Вместимость
        public List<StockRecord> ?Cargo { get; set; }
        
        public Truck GetCopy()
        {
            return new Truck
            {
                Name = this.Name,
                Capacity = this.Capacity,
                Cargo = new List<StockRecord>()
            };
        }
    }
}
