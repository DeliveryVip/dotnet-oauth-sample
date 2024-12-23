using Microsoft.AspNetCore.Mvc;

namespace User
{
  public class DashboardController : BaseController
  {
    public IActionResult Show()
    {
      var model = GetUserViewModel();
      return View("~/Views/User/Dashboard/Show.cshtml", model);
    }
  }
}