using DataTablesParser.WebSample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataTablesParser;

namespace DataTablesParser.WebSample.Controllers
{
    public class PersonController : Controller
    {
        private PersonContext context = new PersonContext();
        //
        // GET: /People/

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult All()
        {
            var parser = new DataTablesParser<Person>(Request, context.People);

            return Json(parser.Parse());
        }

    }
}
