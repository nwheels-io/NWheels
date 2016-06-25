using System;
using System.Web.Mvc;

namespace BigTablePoc.Controllers
{
    public class JasmineController : Controller
    {
        public ViewResult Run()
        {
            return View("SpecRunner");
        }
    }
}
