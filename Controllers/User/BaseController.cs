using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace User
{
  [Authorize]
  public class BaseController : Controller
  {
    protected UserViewModel GetUserViewModel()
    {
      var userViewModel = new UserViewModel
      {
        UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        UserName = User.FindFirst(ClaimTypes.Name)?.Value
      };

      return userViewModel;
    }
  }
}