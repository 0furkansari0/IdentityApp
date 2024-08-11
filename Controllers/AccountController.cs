using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null) 
                {
                    await _signInManager.SignOutAsync();

                    if(!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Lütfen hesabınızı onaylayınız.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        return RedirectToAction("Index","Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Hesabınız kitlendi. Lütfen {timeLeft.Minutes + 1} dakika sonra deneyiniz.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hatalı parola");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Hatalı email");
                }
            }

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var url = Url.Action("ConfirmEmail", "Account", new {user.Id, token});

                    //Email
                    await _emailSender.SendEmainAsync(user.Email, "Hesap Onayı", $"Lütfen hesabınızı onaylamak için <a href='https://localhost:7116{url}'>linke tıklayınız<a>");

             
                    TempData["message"] = "Hesabınızı onaylamak için, mailinize gönderilen linke tıklayınız.";
                    return RedirectToAction("Login","Account");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }



            }

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if(id == null || token == null)
            {
                TempData["message"] = "Geçersiz User veya Token Bilgisi.";
                return View();
            }

            var user = await _userManager.FindByIdAsync(id);

            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if(result.Succeeded)
                {
                    TempData["message"] = "Hesabınız Onaylandı.";
                    return RedirectToAction("Login", "Account");
                }
            }

            TempData["message"] = "Kullanıcı Bulunamadı";
            return View();

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            
           return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["message"] = "Mail boş olamaz.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                TempData["message"] = "Lütfen mailinizi doğru yazdığınızdan emin olun.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url = Url.Action("ResetPassword", "Account", new { user.Id, token });

            await _emailSender.SendEmainAsync(email, "Parola Sıfırlama", $"Parolanızı sıfırlamak için <a href='https://localhost:7116{url}'>linke tıklayınız<a>");

            TempData["message"] = "Mailinize gelen linkten şifre sıfırlama işlemini yapabirsiniz.";

            return View();
        }

        public async Task<IActionResult> ResetPassword(string id, string token)
        {
            if (id == null || token == null)
            {
                TempData["message"] = "Geçersiz User veya Token Bilgisi.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordModel { Token = token };
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user == null)
                {
                    TempData["message"] = "Geçersiz User Bilgisi.";
                    return RedirectToAction("Login");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    TempData["message"] = "Şifre Değiştirildi.";
                    return RedirectToAction("Login");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);

        }
    }
}
