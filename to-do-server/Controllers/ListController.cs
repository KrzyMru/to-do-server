using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using to_do_server.DTO;
using to_do_server.Models;
using to_do_server.Requests;
using to_do_server.Services.Interface;
using static Supabase.Postgrest.Constants;

namespace to_do_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListController : ControllerBase
    {
        [HttpPost("getList")]
        public async Task<IActionResult> GetList([FromBody] int listId, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var userList = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Filter("id", Operator.Equals, listId)
                .Single();
            if (userList == null)
                return BadRequest();

            var listTasks = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.Equals, userList.Id)
                .Get();

            return Ok(new ListDTO(userList, listTasks.Models));
        }

        [HttpGet("getListHeaders")]
        public async Task<IActionResult> GetListHeaders([FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var userLists = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Get();

            var listHeaders = userLists.Models.Select(list => new ListHeaderDTO(list));

            return Ok(listHeaders);
        }

        [HttpPost("saveList")]
        public async Task<IActionResult> SaveList(SaveListRequest request, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);
            if (!int.TryParse(userId, out var userIdInt))
                return BadRequest();

            var newList = new List
            {
                Name = request.Name,
                BackgroundColor = request.BackgroundColor,
                TextColor = request.TextColor,
                IconColor = request.IconColor,
                IconType = request.IconType,
                UserId = userIdInt,
            };

            var response = await client
                .From<List>()
                .Insert(newList);
            if (response.Model == null)
                return BadRequest();

            return Ok(new ListHeaderDTO(response.Model));
        }

        [HttpPut("updateList")]
        public async Task<IActionResult> UpdateList(UpdateListRequest request, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var userList = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Filter("id", Operator.Equals, request.Id)
                .Single();
            if (userList == null)
                return BadRequest();

            var listTasks = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.Equals, userList.Id)
                .Get();

            userList.Name = request.Name;
            userList.BackgroundColor = request.BackgroundColor;
            userList.TextColor = request.TextColor;
            userList.IconColor = request.IconColor;
            userList.IconType = request.IconType;

            var response = await client
                .From<List>()
                .Update(userList);
            if (response.Model == null)
                return BadRequest();

            return Ok(new ListDTO(response.Model, listTasks.Models));
        }

        [HttpDelete("deleteList")]
        public async Task<IActionResult> DeleteList([FromBody] int listId, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var userList = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Filter("id", Operator.Equals, listId)
                .Single();
            if (userList == null)
                return BadRequest();

            await client
                .From<List>()
                .Delete(userList);

            return Ok();
        }
    }
}