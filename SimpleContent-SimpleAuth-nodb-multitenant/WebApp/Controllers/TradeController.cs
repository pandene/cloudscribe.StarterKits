using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eo4Coding.Trade.AutoTrader.Runners;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{

    public class TradeController : Controller
    {
        private readonly CollectionRunner runner;

        public TradeController(CollectionRunner runner)
        {
            this.runner = runner;
        }
        public IActionResult Index()
        {

            return View(runner);
        }
    }
}