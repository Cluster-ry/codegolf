using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CodeGolf.DataBase.Entities;
using CodeGolf.Services;
using CodeGolf.DataBase;
using IronPython.Modules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<DataBaseContext>();
builder.Services.AddSingleton<GroupService>();
builder.Services.AddSingleton<ProblemService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Do initial loading

var problemService = app.Services.GetRequiredService<ProblemService>();
problemService.InitializeProblems();


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
