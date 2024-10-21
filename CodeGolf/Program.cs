using CodeGolf.Python;
using CodeGolf.Services;
using CodeGolf.DataBase;
using CodeGolf.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddPooledDbContextFactory<DataBaseContext>(options => options.UseInMemoryDatabase("Inmemory"));
builder.Services.AddSingleton<DataBaseContext>();
builder.Services.AddSingleton<PythonRunner>();
builder.Services.AddTransient<GroupService>();
builder.Services.AddTransient<ProblemService>();
builder.Services.AddTransient<EvaluateService>();

// Add client side code
builder.Services.AddTransient<ProtectedLocalStorage>();
builder.Services.AddTransient<GroupHandler>();
builder.Services.AddTransient<ProblemHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Do initial loading

_ = app.Services.GetRequiredService<PythonRunner>();
var problemService = app.Services.GetRequiredService<ProblemService>();
problemService.InitializeProblems();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
