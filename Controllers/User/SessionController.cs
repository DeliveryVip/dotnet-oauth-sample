using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace User
{
  [AllowAnonymous]
  public class SessionController : BaseController
  {
    [HttpGet]
    public IActionResult ExternalLogin(string site_url = null)
    {
      if (string.IsNullOrEmpty(site_url))
      {
        // Adicionar mensagem de erro ao ModelState e redirecionar para Guest/Session/New
        ModelState.AddModelError(string.Empty, "The site_url parameter is required.");
        return RedirectToAction("New", "Session", "Guest");
      }

      var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Session");
      var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

      try
      {
        // Decodificar o site_url de Base64
        var base64EncodedBytes = Convert.FromBase64String(site_url);
        var decodedSiteUrl = Encoding.UTF8.GetString(base64EncodedBytes);

        properties.Items["site_url"] = decodedSiteUrl;
      }
      catch (FormatException)
      {
        // Lidar com a situação onde a string não está em Base64 correta
        ModelState.AddModelError(string.Empty, "Invalid site_url format.");
        return RedirectToAction("New", "Session", "Guest");
      }

      return Challenge(properties, "DeliveryVip");
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string remoteError = null)
    {
      if (remoteError != null)
      {
        ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
        return RedirectToAction("New", "Session", "Guest");
      }

      var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
      {
        return RedirectToAction("New", "Session", "Guest");
      }

      // Use SignInAsync para autenticar o usuário
      await HttpContext.SignInAsync(
          CookieAuthenticationDefaults.AuthenticationScheme,
          new ClaimsPrincipal(authenticateResult.Principal.Identity),
          new AuthenticationProperties { IsPersistent = true }
      );

      return RedirectToAction("Show", "Dashboard", "User");
    }
  }
}