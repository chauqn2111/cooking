using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectLibrary.ObjectBussiness;
using ProjectWebAPI.App.Code;
using ProjectWebAPI.Models;

namespace ProjectWebMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private string _userApiUrl = "";

        public AccountController()
        {
            _httpClient = new HttpClient();
            var contextType = new MediaTypeWithQualityHeaderValue("application/json");
            _userApiUrl = "https://localhost:7269/api/User";
        }
        public async Task<IActionResult> Index()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_userApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var strData = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                List<ProjectLibrary.ObjectBussiness.User> userList = JsonSerializer.Deserialize<List<ProjectLibrary.ObjectBussiness.User>>(strData, options);
                return View(userList);
            }
            else
            {
                return View(new List<ProjectLibrary.ObjectBussiness.User>());
            }
        }


        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Đăng nhập";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                string loginData = JsonSerializer.Serialize(model);
                var contentData = new StringContent(loginData, System.Text.Encoding.UTF8, "application/json");
                string loginApiUrl = $"{_userApiUrl}/login";
                HttpResponseMessage response = await _httpClient.PostAsync(loginApiUrl, contentData);

                if (response.IsSuccessStatusCode)
                {
                    var strData = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    ProjectLibrary.ObjectBussiness.User user = JsonSerializer.Deserialize<ProjectLibrary.ObjectBussiness.User>(strData, options);

                    if (!user.EmailConfirmed)
                    {
                        TempData["Message"] = "Email chưa được xác thực.";
                        return View("NotificationEmailComfirm");
                    }
                    ViewBag.User = user.UserId.ToString();

					var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Assuming Id is an integer
                        new Claim("UserId", user.UserId.ToString()),
                };

                    if (user.RoleId == 1)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));

                        var identity = new ClaimsIdentity(claims, "Admin");
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync("Admin", principal, new AuthenticationProperties
                        {
                            IsPersistent = model.RememberLogin
                        });

                        TempData["Message"] = "Đăng nhập thành công";

                        var routeValues = new RouteValueDictionary
                        {
                            {"area", "Admin"},
                            {"claimType", "UserClaim"},
                            {"claimValue", "true"}
                        };

                        LogUserActivity.LogUserLoginActivity(user.UserId);
                        return RedirectToAction("Index", "HomeAdmin", routeValues);
                    }
                    else if (user.RoleId == 2)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "User"));

                        var identity = new ClaimsIdentity(claims, "User");
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync("User", principal, new AuthenticationProperties
                        {
                            IsPersistent = model.RememberLogin
                        });

                        TempData["Message"] = "Đăng nhập thành công";
                        var routeValues = new RouteValueDictionary
                        {
                            {"area", "User"},
                            {"claimType", "UserClaim"},
                            {"claimValue", "true"}
                        };
                        // Redirect to a view for regular users
                        LogUserActivity.LogUserLoginActivity(user.UserId);
                        return RedirectToAction("Index", "HomeUser", routeValues);
                    }
                    else
                    {
                        // Handle other roles or scenarios
                        ModelState.AddModelError("", "Role not supported");
                        return View("Login", model);
                    }
                }
                else
                {
                    string errorData = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error: {errorData}");
                    return View("Login", model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View("Error");
            }
        }

        public async Task<IActionResult> Logout()
        {
            //var UserIdClaim = ((ClaimsIdentity)User.Identity).FindFirst("UserId");
            string userId = ViewBag.User;// UserIdClaim?.Value;

            // Log thông tin đăng xuất
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var userIdInt))
            {
                LogUserActivity.LogUserLogoutActivity(userIdInt);
            }

            // Đăng xuất người dùng
            await HttpContext.SignOutAsync();

            // Chuyển hướng đến trang đăng nhập hoặc trang chính
            return RedirectToAction("Index", "Home", new { area = "" });

        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                string userData = JsonSerializer.Serialize(model);
                var contentData = new StringContent(userData, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_userApiUrl}/register", contentData);
                if (response.IsSuccessStatusCode)
                {
                    // Đăng ký thành công, hiển thị thông báo xác nhận email
                    ViewData["Email"] = model.Email;
                    return View("EmailAuthentication");
                }
                else
                {
                    // Xử lý lỗi, đọc đối tượng JSON từ response
                    string errorData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(errorData))
                    {
                        return BadRequest("Object null");
                    }
                    /*var error = JsonSerializer.Deserialize<ErrorResponseModel>(errorData);*/

                    // Lưu thông báo lỗi vào TempData
                    ViewData["ErrorMessage"] = errorData;
                    TempData.Keep("ErrorMessage");

                    // Redirect về view NotificationEmailConfirm
                    return View("Register");
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ, lưu thông báo lỗi vào TempData
                TempData["ErrorMessage"] = ex.Message;

                // Redirect về view Register
                return RedirectToAction("Register");
            }
        }

        public IActionResult ConfirmEmail()
        {
            // Lấy giá trị từ đường dẫn query string
            string email = HttpContext.Request.Query["email"];
            string token = HttpContext.Request.Query["token"];

            // Tạo và gán giá trị cho ConfirmEmailModel
            ConfirmEmailModel model = new ConfirmEmailModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel model)
        {
            try
            {
                string confirmData = JsonSerializer.Serialize(model);
                var contentData = new StringContent(confirmData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync($"{_userApiUrl}/confirm-email", contentData);
                if (response.IsSuccessStatusCode)
                {

                    return RedirectToAction("Login");
                }
                TempData["ErrorMessage"] = "Invalid response from server.";
                return View("NotificationEmailConfirm");
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ, lưu thông báo lỗi vào TempData
                TempData["ErrorMessage"] = ex.Message;

                // Redirect về view NotificationEmailConfirm
                return View("NotificationEmailConfirm");
            }
        }
    }
}
