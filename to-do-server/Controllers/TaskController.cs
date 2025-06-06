using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using static Supabase.Postgrest.Constants;
using to_do_server.Models;
using to_do_server.Requests;
using to_do_server.Services.Interface;
using to_do_server.DTO;
using System.Globalization;
using System;
using System.Threading.Tasks;

namespace to_do_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        [HttpPost("getTodayTasks")]
        public async Task<IActionResult> GetTodayTasks([FromBody] string timezone, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var userLists = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Get();
            var listIds = userLists.Models.Select(l => l.Id).ToList();

            var userTasks = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.In, listIds)
                .Get();

            TimeZoneInfo clientTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            DateTimeOffset today = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, clientTimeZone);
            var todayUserTasks = userTasks.Models
                .Where(task => {
                    DateTimeOffset clientTaskDue = TimeZoneInfo.ConvertTime(task.Due, clientTimeZone);
                    return clientTaskDue.Date == today.Date;
                })
                .ToList();

            var listMap = userLists.Models.ToDictionary(l => l.Id);

            var result = todayUserTasks
                .Select(task =>
                {
                    var list = listMap.GetValueOrDefault(task.ListId);
                    return new TaskDTO(task, list);
                })
                .ToList();

            return Ok(result);
        }

        [HttpPost("getDateTasks")]
        public async Task<IActionResult> GetDateTasks(GetDateTasksRequest request, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            DateTimeOffset targetDate = DateTimeOffset.ParseExact(request.Date, "MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);

            var userLists = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Get();
            var listIds = userLists.Models.Select(l => l.Id).ToList();

            var userTasks = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.In, listIds)
                .Get();

            TimeZoneInfo clientTimeZone = TimeZoneInfo.FindSystemTimeZoneById(request.Timezone);
            var dateUserTasks = userTasks.Models
                 .Where(task => {
                     DateTimeOffset clientTaskDue = TimeZoneInfo.ConvertTime(task.Due, clientTimeZone);
                     return clientTaskDue.Date == targetDate.Date;
                 })
                 .ToList();

            var listMap = userLists.Models.ToDictionary(l => l.Id);

            var result = dateUserTasks
                .Select(task =>
                {
                    var list = listMap.GetValueOrDefault(task.ListId);
                    return new TaskDTO(task, list);
                })
                .ToList();

            return Ok(result);
        }

        [HttpPost("getTaskCounts")]
        public async Task<IActionResult> GetTaskCounts(GetTaskCountsRequest request, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            DateTimeOffset startDate = DateTimeOffset.ParseExact(request.StartDate, "MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);
            DateTimeOffset endDate = DateTimeOffset.ParseExact(request.EndDate, "MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture).AddDays(1);
            TimeZoneInfo clientTimeZone = TimeZoneInfo.FindSystemTimeZoneById(request.Timezone);
            // Utc format for filters
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startDate.DateTime, clientTimeZone).ToString("o");
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endDate.DateTime, clientTimeZone).ToString("o");

            var userLists = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Get();
            var listIds = userLists.Models.Select(l => l.Id).ToList();

            var dateRangeUserTasks = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.In, listIds)
                .Filter("due", Operator.GreaterThanOrEqual, startUtc)
                .Filter("due", Operator.LessThan, endUtc)
                .Get();

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientTimeZone);
            var taskCounts = dateRangeUserTasks.Models
                .GroupBy(t => TimeZoneInfo.ConvertTime(t.Due, clientTimeZone).ToString("yyyy-MM-dd"))
                .ToDictionary(
                    g => g.Key,
                    g => new Dictionary<string, int>()
                    {
                            { "Overdue", g.Count(task => !task.Completed && TimeZoneInfo.ConvertTime(task.Due, clientTimeZone) <= now) },
                            { "Pending", g.Count(task => !task.Completed && TimeZoneInfo.ConvertTime(task.Due, clientTimeZone) > now) },
                            { "Completed", g.Count(task => task.Completed) },
                    }
                );

            return Ok(taskCounts);
        }

        [HttpPost("saveTask")]
        public async Task<IActionResult> SaveTask(SaveTaskRequest request, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            DateTimeOffset due = DateTimeOffset.ParseExact(request.Due, "MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);
            DateTimeOffset created = DateTimeOffset.ParseExact(request.Due, "MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);

            var userList = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Filter("id", Operator.Equals, request.ListId)
                .Single();
            if (userList == null)
                return BadRequest();

            var newTask = new Models.Task
            {
                Name = request.Name,
                Description = request.Description,
                Created = created,
                Completed = false,
                Due = due,
                ListId = userList.Id,
            };

            var response = await client
                .From<Models.Task>()
                .Insert(newTask);
            if (response.Model == null)
                return BadRequest();

            return Ok(new TaskDTO(response.Model, userList));
        }

        [HttpDelete("deleteTask")]
        public async Task<IActionResult> DeleteTask([FromBody] int taskId, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var userLists = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Get();
            var listIds = userLists.Models.Select(l => l.Id).ToList();

            var targetTask = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.In, listIds)
                .Filter("id", Operator.Equals, taskId)
                .Single();
            if (targetTask == null)
                return BadRequest();

            await client
                .From<Models.Task>()
                .Delete(targetTask);

            return Ok();
        }

        [HttpPut("updateTask")]
        public async Task<IActionResult> UpdateTask(UpdateTaskRequest request, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            DateTimeOffset due = DateTimeOffset.ParseExact(request.Due, "MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);

            var userLists = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Get();
            var listIds = userLists.Models.Select(l => l.Id).ToList();

            var targetTask = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.In, listIds)
                .Filter("id", Operator.Equals, request.Id)
                .Single();
            if (targetTask == null)
                return BadRequest();

            targetTask.Name = request.Name;
            targetTask.Description = request.Description;
            targetTask.Due = due;

            var targetList = userLists.Models.Find(list => list.Id == targetTask.ListId);
            if (targetList == null)
                return BadRequest();

            var response = await client
                .From<Models.Task>()
                .Update(targetTask);
            if (response.Model == null)
                return BadRequest();

            return Ok(new TaskDTO(response.Model, targetList));
        }

        [HttpPut("toggleTask")]
        public async Task<IActionResult> ToggleTask([FromBody] int taskId, [FromServices] IAuthService authService, Supabase.Client client)
        {
            var token = Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (token == null || !authService.ValidateToken(token))
                return Unauthorized();
            var userId = authService.GetUserId(token);

            var userLists = await client
                .From<List>()
                .Filter("user_id", Operator.Equals, userId)
                .Get();
            var listIds = userLists.Models.Select(l => l.Id).ToList();

            var targetTask = await client
                .From<Models.Task>()
                .Filter("list_id", Operator.In, listIds)
                .Filter("id", Operator.Equals, taskId)
                .Single();
            if (targetTask == null)
                return BadRequest();

            var targetList = userLists.Models.Find(list => list.Id == targetTask.ListId);
            if (targetList == null)
                return BadRequest();

            targetTask.Completed = !targetTask.Completed;

            var response = await client
                .From<Models.Task>()
                .Update(targetTask);
            if (response.Model == null)
                return BadRequest();

            return Ok(new TaskDTO(response.Model, targetList));
        }
    }
}