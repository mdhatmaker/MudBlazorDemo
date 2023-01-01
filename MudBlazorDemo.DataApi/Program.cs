using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using MudBlazorDemo.DataAccess;
using MudBlazorDemo.Models;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Reflection.Metadata.BlobBuilder;
using System.ComponentModel.DataAnnotations;
using MudBlazorDemo.Models.Auth.JWT;
using MudBlazorDemo.Models.Auth.JWT.Mock;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Diagnostics.Metrics;

// Reference:
// https://medium.com/geekculture/minimal-apis-in-net-6-a-complete-guide-beginners-advanced-fd64f4da07f5


var builder = WebApplication.CreateBuilder(args);

// configure using configuration
builder.Services.AddOptions();  // Configure<ApplicationOptions>( (Configuration.GetSection("applicationSettings"));
//builder.Services.Configure<ApplicationOptions>(IConfiguration config);  // Configuration.GetSection("applicationSettings"));
//builder.Configuration.AddConfiguration()
//// then apply a configuration function
//builder.Services.Configure<ApplicationOptions>(options =>
//{
//    // overwrite previous values
//    options.Foo = "bar";
//});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Inject the swagger services in your WebApplication builder.
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token * *_only_ * *",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });
});

builder.Services.AddDbContext<BloggingContext>();

builder.Services.AddSingleton<TokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepositoryService>(new UserRepositoryService());

builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader());
});


//builder.Services.AddAuthorization();
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
//{
//    opt.TokenValidationParameters = new()
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Use Swagger in your application by adding the middleware to render the Swagger UI.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.UseCors("Open");


// Get list of all blogs
app.MapGet("/blogs", async (BloggingContext ctx) =>
    await ctx.Blogs.OrderBy(b => b.BlogId).ToListAsync()
//{
    //using var db = new BloggingContext();

    //// Note: This sample requires the database to be created before running.
    ////Console.WriteLine($"Database path: {db.DbPath}.");

    //// Read
    //Console.WriteLine("Querying for a blog");
    //var blog = db.Blogs
    //    .OrderBy(b => b.BlogId)
    //    .First();
    //return blog;
//}
)
.Produces<List<Blog>>(StatusCodes.Status200OK)
.WithName("GetAllBlogs")
.WithTags("Getters");
//.ExcludeFromDescription(); --> exclude any method from the swagger description



// Fetch a single record using ID
app.MapGet("/blogs/{id}", async (BloggingContext ctx, int id) =>
await ctx.Blogs.SingleOrDefaultAsync(s => s.BlogId == id) is Blog myblog ? Results.Ok(myblog) : Results.NotFound()
)
.Produces<Blog>(StatusCodes.Status200OK)
.WithName("GetBlogbyID")
.WithTags("Getters");



// Perform a search for a given keyword
app.MapGet("/blogs/search/{query}",
(string query, BloggingContext ctx) =>
{
    var _selectedBlogs = ctx.Blogs.Where(x => x.Url.ToLower().Contains(query.ToLower())).ToList();
    return _selectedBlogs.Count > 0 ? Results.Ok(_selectedBlogs) : Results.NotFound(Array.Empty<Blog>());
})
.Produces<List<Blog>>(StatusCodes.Status200OK)
.WithName("Search")
.WithTags("Getters");



// Get Paginated Result set
app.MapGet("/blogs_by_page", async (int pageNumber, int pageSize, BloggingContext ctx) =>
await ctx.Blogs
.Skip((pageNumber - 1) * pageSize)
.Take(pageSize)
.ToListAsync()
)
.Produces<List<Blog>>(StatusCodes.Status200OK)
.WithName("GetBlogsByPage")
.WithTags("Getters");



// Add a new blog
app.MapPost("/blogs",
async ([FromBody] Blog addBlog, [FromServices] BloggingContext ctx, HttpResponse response) =>
//() =>
{
    Console.WriteLine("Inserting a new blog");

    ctx.Blogs.Add(addBlog);
    await ctx.SaveChangesAsync();
    response.StatusCode = 200;
    response.Headers.Location = $"blogs/{addBlog.BlogId}";

    //using var db = new BloggingContext();
    // Create
    //Console.WriteLine("Inserting a new blog");
    //db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
    //db.SaveChanges();
})
.Accepts<Blog>("application/json")
.Produces<Blog>(StatusCodes.Status201Created)
.WithName("AddNewBlog")
.WithTags("Setters");



// Update an existing blog
app.MapPut("/blogs",
[AllowAnonymous] async (int blogId, string blogUrl, [FromServices] BloggingContext ctx, HttpResponse response) =>
{
    var myblog = ctx.Blogs.SingleOrDefault(s => s.BlogId == blogId);
    if (myblog == null) return Results.NotFound();
    myblog.Url = blogUrl;
    await ctx.SaveChangesAsync();
    return Results.Created("/blogs", myblog);
})
.Produces<Blog>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status404NotFound)
.WithName("UpdateBlog")
.WithTags("Setters");



app.MapPost("/login", [AllowAnonymous] async ([FromBodyAttribute] UserModel userModel, TokenService tokenService, IUserRepositoryService userRepositoryService, HttpResponse response) =>
{
    var userDto = userRepositoryService.GetUser(userModel);
    if (userDto == null)
    {
        response.StatusCode = 401;
        return;
    }
    var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"], userDto);
    await response.WriteAsJsonAsync(new { token = token });
    return;
})
.Produces(StatusCodes.Status200OK)
.WithName("Login")
.WithTags("Accounts");



// Protected resource to make sure our security is working
app.MapGet("/AuthorizedResource", (Func<string>) (
[Authorize] () => "Action Succeeded")
)
.Produces(StatusCodes.Status200OK)
.WithName("Authorized")
.WithTags("Accounts")
.RequireAuthorization();




//app.Urls.Add("https://localhost:3000");
//app.Urls.Add("https://localhost:4000");     // can run API on multiple ports

app.Run();




// Adding Authentication and Authorization using JWT

// Make sure you have the following packages installed:
// Install-Package Microsoft.AspNetCore.Authentication.JwtBearer 
// Install-Package Microsoft.IdentityModel.Tokens

// Now create the JWT.Mock classes:










#region ################################ CODE GRAVEYARD ################################
/*var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();*/

//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
#endregion #############################################################################