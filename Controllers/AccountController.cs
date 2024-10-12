﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Sastri_Library_Backend.Data;
using Sastri_Library_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Sastri_Library_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly LibraryAppContext _context;
        private readonly UserManager<Student> _userManager;
        private readonly JwtHelper _jwtHelper;
        private readonly SignInManager<Student> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AccountController(LibraryAppContext context, UserManager<Student> userManager, JwtHelper jwtHelper, SignInManager<Student> signInManager, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _jwtHelper = jwtHelper;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new Student
            {
                UserName = model.Email,
                Email = model.Email,
                StudentIdNumber = model.StudentIdNumber,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            await _signInManager.SignInAsync(user, isPersistent: false);

            if (result.Succeeded)
            {
                var token = _jwtHelper.GenerateJwtToken(user.Id, user.Email);
                // Set the token in a cookie
                SetTokenCookie(token);
                return Ok(new { Message = "User created successfully", Token = token });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var token = _jwtHelper.GenerateJwtToken(user.Id, user.Email); // Implement this in your JWT helper


                // Set the token in a cookie
                SetTokenCookie(token);

                return Ok(new { Token = token });
            }

            return Unauthorized();
        }

        [HttpGet("profile")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Get the user ID from JWT token claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                    return Unauthorized("Invalid user token.");

                // Fetch user profile from the database
                var userProfile = await _context.Users.FindAsync(userId);

                if (userProfile == null)
                    return NotFound("User profile not found.");

                var userProfileData = new
                {
                    userProfile.Email,
                    userProfile.FirstName,
                    userProfile.LastName
                };

                return Ok(userProfileData);
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Ensures the cookie is accessible only by the server
                SameSite = SameSiteMode.None, // Allows cross-site requests
                Secure = true, // Ensures the cookie is sent over HTTPS only
                Expires = DateTime.Now.AddMinutes(30).ToUniversalTime() // Set expiration time for the cookie
            };

            Response.Cookies.Append("Bearer", token, cookieOptions);
        }

        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class SignUpDto
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string StudentIdNumber { set; get; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}