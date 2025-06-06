using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using to_do_server.Services.Implementation;
using to_do_server.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration.GetValue<string>("jwtIssuer");
var jwtAudience = builder.Configuration.GetValue<string>("jwtAudience");
var jwtKey = builder.Configuration.GetValue<string>("jwtKey");
var supabaseUrl = builder.Configuration.GetValue<string>("supabaseUrl");
var supabaseKey = builder.Configuration.GetValue<string>("supabaseKey");

builder.Services.AddControllers();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddScoped(_ =>
    new Supabase.Client(
        supabaseUrl,
        supabaseKey,
        new Supabase.SupabaseOptions { AutoConnectRealtime = true }
    )
);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
    );
});
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

