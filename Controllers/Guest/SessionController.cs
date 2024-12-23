using Microsoft.AspNetCore.Mvc;

namespace Guest
{
  public class SessionController : BaseController
  {
    [HttpGet]
    public IActionResult New()
    {
      var siteUrl = HttpContext.Request.Query["site_url"].ToString();

      if (!string.IsNullOrEmpty(siteUrl))
      {
        return RedirectToAction("ExternalLogin", "Session", new { area = "User", site_url = siteUrl });
      }
      else
      {
        return View("~/Views/GUest/Session/New.cshtml");
      }
    }
  }
}