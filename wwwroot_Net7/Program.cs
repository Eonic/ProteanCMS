using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.Features;
using Protean;
using static Antlr4.Runtime.Atn.SemanticContext;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();




var app = builder.Build();

app.UseCallProtean();

//app.MapGet("/", () => "This is a string");

app.Run();
