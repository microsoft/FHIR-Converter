using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Tool;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var templateDirectory = builder.Configuration["TemplateDirectory"];
builder.Services.AddSingleton<ITemplateProvider>(new TemplateProvider($"{templateDirectory}/Ccda", DataType.Ccda));
builder.Services.AddSingleton<IFhirConverter>(new CcdaProcessor(new ProcessorSettings(), ConsoleLoggerFactory.CreateLogger<CcdaProcessor>()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/convert", async (HttpContext context, ITemplateProvider templateProvider, IFhirConverter dataProcessor) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var input = await reader.ReadToEndAsync();
        var result = dataProcessor.Convert(input, "CCD", templateProvider);
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    })
    .Accepts<string>("application/xml")
    .WithName("FhirConverter");

app.Run();
