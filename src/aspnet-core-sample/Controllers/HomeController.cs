using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using websample.Models;
using DataTablesParser;

namespace websample.Controllers
{
    public class HomeController : Controller
    {
        private readonly PersonContext _context;

        public HomeController(PersonContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Data()
        {
            var parser = new Parser<Person>(Request.Form, _context.People);

            return Json(parser.Parse());
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
