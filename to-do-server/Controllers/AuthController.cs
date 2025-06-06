using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using to_do_server.Models;
using to_do_server.Requests;
using to_do_server.Services.Interface;
using static Supabase.Postgrest.Constants;

namespace to_do_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController() : ControllerBase
    {
        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn(SignInRequest signInRequest, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var user = await client
                .From<User>()
                .Filter("email", Operator.Equals, signInRequest.Email)
                .Single();
            if (user == null || user.Password != signInRequest.Password)
                return BadRequest();

            var newToken = authService.GenerateToken(user.Id);

            return Ok(new { Token = newToken });
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp(SignUpRequest signUpRequest, Supabase.Client client)
        {
            var isValid = ValidateCredentials(signUpRequest.Email, signUpRequest.Password);
            if (!isValid)
                return BadRequest();

            var user = await client
                .From<User>()
                .Filter("email", Operator.Equals, signUpRequest.Email)
                .Single();
            if (user != null)
                return BadRequest();

            var newUser = new User
            {
                Email = signUpRequest.Email,
                Password = signUpRequest.Password,
            };

            await client
                .From<User>()
                .Insert(newUser);

            return Ok();
        }

        [HttpPost("deleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var user = await client
                .From<User>()
                .Filter("id", Operator.Equals, userId)
                .Single();
            if (user == null)
                return BadRequest();

            await client
                .From<User>()
                .Delete(user);

            return Ok();
        }
        private static bool ValidateCredentials(string email, string password)
        {
            if (email == null || password == null)
                return false;

            if (!email.Contains('@'))
                return false;
            if (password.Length < 8)
                return false;

            return true;
        }
    }
}
