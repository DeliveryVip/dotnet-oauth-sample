using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(30);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

// Configure authentication services
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = "DeliveryVip"; // Nome do provedor OAuth
})
.AddCookie()
.AddOAuth("DeliveryVip", options =>
{
  options.ClientId = "wC_eoif7Nx4eS-KazEd5DIjdIv27dUPLCnA_ZW1Oth0";
  options.ClientSecret = "bpFUAP2ZPzTLgXPjdQMqyeXRC5boubhGUesUm5cLDuE";
  options.CallbackPath = new PathString("/auth/delivery_vip/callback");
  options.AuthorizationEndpoint = $"https://app.deliveryvip.com.br/panel/company_{0}/oauth/authorize";
  options.TokenEndpoint = "https://api.deliveryvip.com.br/authentication/v1/oauth/token";
  options.UserInformationEndpoint = "https://api.deliveryvip.com.br/merchant/v3/me";
  options.SaveTokens = true;

  options.Scope.Add("od.all");

  options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
  options.ClaimActions.MapJsonSubKey(ClaimTypes.Name, "basicInfo", "name");

  options.Events = new OAuthEvents
  {
    OnCreatingTicket = async context =>
    {
      // Solicitar informações do usuário
      var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
      request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
      request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

      var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
      response.EnsureSuccessStatusCode();

      var json = await response.Content.ReadAsStringAsync();
      using var document = JsonDocument.Parse(json);
      var user = document.RootElement;

      Console.WriteLine(user);

      context.RunClaimActions(user);
    },
    OnRedirectToAuthorizationEndpoint = context =>
    {
      // Obtém o novo AuthorizationEndpoint a partir do `site_url` nos AuthenticationProperties
      var siteUrl = context.Properties.Items.ContainsKey("site_url")
          ? $"{context.Properties.Items["site_url"]}/oauth/authorize"
          : null;

      if (!string.IsNullOrEmpty(siteUrl))
      {
        // Extrai os query params da RedirectUri
        var redirectUri = new Uri(context.RedirectUri);
        var queryParams = redirectUri.Query; // Obtém os parâmetros existentes

        // Cria a nova URL com o siteUrl e os query params
        var newAuthorizationUri = $"{siteUrl}{queryParams}";

        // Redireciona para o novo AuthorizationEndpoint
        context.Response.Redirect(newAuthorizationUri);
      }
      else
      {
        // Continua com o comportamento padrão
        context.Response.Redirect(context.RedirectUri);
      }

      return Task.CompletedTask;
    }
  };
});

// Configure the application cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/Guest/Session/New";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configurar `UseForwardedHeaders` para reconhecer cabeçalhos de proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "guest_session",
    pattern: "guest/session/new",
    defaults: new { controller = "Session", action = "New" });

app.MapControllerRoute(
    name: "user_session",
    pattern: "user/session/ExternalLogin",
    defaults: new { controller = "Session", action = "ExternalLogin" });

app.MapControllerRoute(
    name: "user_dashboard",
    pattern: "user/dashboard",
    defaults: new { controller = "Dashboard", action = "Show" });

app.Run();
