using Microsoft.AspNetCore.Mvc;
using ProduceAndDistributeTest.Models;

namespace ProduceAndDistributeTest.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ProcessController : Controller
    {
        private ProduceAndDistribute? _PnD;

        public ProcessController()
        {
            _PnD = ProduceAndDistribute.Instance();
        }

        [HttpPost("StartProcess")]
        async public Task<IActionResult> StartProcess([FromForm]Setup setup)
        {
            if (setup != null)
            {
                //_setup = setup;
                _PnD = ProduceAndDistribute.Instance(setup);
                GC.Collect();
                await _PnD.StartProcess(setup.Duration);
                return Ok();
            }
            return Conflict();
        }

        [HttpGet("StockJournal")]
        public ActionResult<IEnumerable<StockRecord>> StockJournal(int page = 0, int itemsOnPage = 100)
        {
            return _PnD != null ? _PnD.GetWarehouseIncome(page, itemsOnPage) : NotFound();
        }

        [HttpGet("DeliveryStatistic")]
        public ActionResult<IEnumerable<Truck>> DeliveryStatistic()
        {
            return _PnD != null ? _PnD.GetDeliveryStatistic() : NotFound();
        }
    }
}
