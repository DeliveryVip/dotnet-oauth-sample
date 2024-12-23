using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Guest
{
  public class BaseController : Controller
  {
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Verifica se o usuário está autenticado
        if (User.Identity.IsAuthenticated)
        {
            // Redireciona para a Dashboard do usuário
            context.Result = new RedirectToActionResult("Show", "Dashboard", new { area = "User" });
            return;
        }

        // Continua normalmente caso o usuário não esteja autenticado
        base.OnActionExecuting(context);
    }
  }
}