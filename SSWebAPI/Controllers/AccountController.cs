using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SSBOL;
using SSWebAPI.ViewModels;

namespace SSWebAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        UserManager<SSUser> userManager;
        SignInManager<SSUser> signInManager;

        public AccountController(SignInManager<SSUser> _signInManager, UserManager<SSUser> _userManager)
        {
            signInManager = _signInManager;
            userManager = _userManager;
        }

        [HttpGet("signInWithGoogle")]
        public IActionResult SignInWithGoogle()
        {
            var authenticationProperties = signInManager.ConfigureExternalAuthenticationProperties("Google", Url.Action(nameof(HandleExternalLogin)));
            return Challenge(authenticationProperties, "Google");
        }

        public async Task<IActionResult> HandleExternalLogin()
        {
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (!result.Succeeded) //user does not exist yet
            {
                var gUser = new SSUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(gUser);
                var roleResult = await userManager.AddToRoleAsync(gUser, "User");

                await userManager.AddLoginAsync(gUser, info);
                var newUserClaims = info.Principal.Claims.Append(new Claim("userId", gUser.Id));
                await userManager.AddClaimsAsync(gUser, newUserClaims);
                await signInManager.SignInAsync(gUser, isPersistent: true);
            }

            //Old process for generating JWT 
            var user = await userManager.FindByNameAsync(email);
            var roles = await userManager.GetRolesAsync(user);
            //Step - 1 Creating Claims
            IdentityOptions identityOptions = new IdentityOptions();
            var claims = new Claim[]
            {
                        new Claim(identityOptions.ClaimsIdentity.UserIdClaimType,user.Id),
                        new Claim(identityOptions.ClaimsIdentity.UserNameClaimType,user.UserName),
                        new Claim(identityOptions.ClaimsIdentity.RoleClaimType, roles[0])
            };

            //Step - 2: Create signingKey from Secretkey
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-secret-key-for-angular8-app"));

            //Step -3: Create signingCredentials from signingKey with HMAC algorithm
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            //Step - 4: Create JWT with signingCredentials, IdentityClaims & expire duration.
            var jwt = new JwtSecurityToken(signingCredentials: signingCredentials,
                                            expires: DateTime.Now.AddSeconds(30), claims: claims);

            //Step - 5: Finally write the token as response with OK().
            return Ok(new { tokenJwt = new JwtSecurityTokenHandler().WriteToken(jwt), userName = user.UserName, role = roles[0] });

            //var tokenJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            //return Redirect("http://Localhost:4200/google?tokenJwt=" + tokenJwt + "&id=" + user.Id + "&userName=" + user.UserName + "&role=" + roles[0]);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await userManager.FindByNameAsync(model.UserName);
                    var signInResult = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                    if (signInResult.Succeeded)
                    {
                        var roles = await userManager.GetRolesAsync(user);

                        //Step - 1 Creating Claims
                        IdentityOptions identityOptions = new IdentityOptions();
                        var claims = new Claim[]
                        {
                        new Claim(identityOptions.ClaimsIdentity.UserIdClaimType,user.Id),
                        new Claim(identityOptions.ClaimsIdentity.UserNameClaimType,user.UserName),
                        new Claim(identityOptions.ClaimsIdentity.RoleClaimType, roles[0])
                        };

                        //Step - 2: Create signingKey from Secretkey
                        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-secret-key-for-angular8-app"));

                        //Step -3: Create signingCredentials from signingKey with HMAC algorithm
                        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                        //Step - 4: Create JWT with signingCredentials, IdentityClaims & expire duration.
                        var jwt = new JwtSecurityToken(signingCredentials: signingCredentials,
                                                        expires: DateTime.Now.AddMinutes(30), claims: claims);

                        //Step - 5: Finally write the token as response with OK().
                        return Ok(new { tokenJwt = new JwtSecurityTokenHandler().WriteToken(jwt), id = user.Id, userName = user.UserName, role = roles[0] });

                        //return Ok(new { userName = user.UserName, role = roles[0] });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid UserName or Password");
                    }
                }
                return BadRequest(ModelState.Values);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error! Please Contact Admin!");
            }
        }


        [HttpPost("register"), DisableRequestSizeLimit]
        public async Task<IActionResult> Register()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<RegisterViewModel>(Request.Form["myModel"].ToString());
                if (ModelState.IsValid)
                {
                    var user = new SSUser()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        DOB = model.DOB
                    };

                    var userResult = await userManager.CreateAsync(user, model.Password);
                    if (userResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(user, "User");
                        if (roleResult.Succeeded)
                        {
                            if (Request.Form.Files.Count > 0)
                            {
                                var filePath = Path.GetFullPath("~/ProfilePics/" + user.Id + ".jpeg").Replace("~\\", "");

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    Request.Form.Files[0].CopyTo(stream);
                                }
                            }
                            return Ok(user);
                        }
                    }
                    else
                    {
                        foreach (var item in userResult.Errors)
                        {
                            ModelState.AddModelError(item.Code, item.Description);
                        }
                    }
                }
                return BadRequest(ModelState.Values);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error! Please Contact Admin!");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("getProfilePic")]
        public IActionResult GetProfilePic()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var filePath = Path.GetFullPath("~/ProfilePics/" + id + ".jpeg").Replace("~\\", "");
            if (!System.IO.File.Exists(filePath))
            {
                filePath = Path.GetFullPath("~/ProfilePics/universal.jpeg").Replace("~\\", "");
            }
            var profilePic = System.IO.File.OpenRead(filePath);
            return File(profilePic, "image/jpeg");
        }
    }
}