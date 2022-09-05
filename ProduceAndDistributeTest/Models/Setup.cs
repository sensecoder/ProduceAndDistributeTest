using System.ComponentModel.DataAnnotations;

namespace ProduceAndDistributeTest.Models
{
    public class Setup
    {
        private const int _minM = 100;
        private const int _minFactoriesCount = 3;
        private const int _minN = 50;
        private const int _minTrucksTypeCount = 2;
        private const int _maxTrucksTypeCount = 6;
        private Truck[] _trucksTypes = new Truck[]
        {
            new Truck { Capacity = 1000, Name = "MAN TGL" },
            new Truck { Capacity = 700, Name = "Volvo FL" },
            new Truck { Capacity = 500, Name = "Mercedes Atego" },
            new Truck { Capacity = 300, Name = "GAZ Bully" },
            new Truck { Capacity = 200, Name = "GAZelle Long" },
            new Truck { Capacity = 150, Name = "GAZelle" }
        };
        
        [Range(_minM, int.MaxValue, ErrorMessage = "Значение M должно быть больше или равно {1}!")]
        public int M { get; set; }
        [Range(_minFactoriesCount, int.MaxValue, ErrorMessage = "Количество фабрик должно быть больше или равно {1}!")]
        public int FactoriesCount { get; set; }
        [Range(_minN, int.MaxValue, ErrorMessage = "Значение n должно быть больше или равно {1}!")]
        public int n { get; set; }
        [Range(_minTrucksTypeCount,_maxTrucksTypeCount, ErrorMessage = "Количество типов грузовиков должно быть между {1} и {2}!")]
        public int TrucksTypesCount { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Количество циклов должно быть положительным числом!")]
        public int Duration { get; set; } = 100;

        public Truck GetTruck(int Index)
        {
            return _trucksTypes[Index];
        }

        public Setup()
        {
            M = _minM;
            FactoriesCount = _minFactoriesCount;
            n = _minN;
            TrucksTypesCount = _minTrucksTypeCount;
        }
    }
}
