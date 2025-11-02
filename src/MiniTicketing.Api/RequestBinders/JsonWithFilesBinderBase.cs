
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MiniTicketing.Api.RequestBinders;
public abstract class JsonWithFilesBinderBase<TDto> : IModelBinder
{
  private readonly JsonSerializerOptions _options;

  protected JsonWithFilesBinderBase(JsonSerializerOptions options)
  {
    _options = options;
  }

  protected virtual string TicketFieldName => "ticket";

  public Task BindModelAsync(ModelBindingContext ctx)
  {
    var form = ctx.HttpContext.Request.Form;

    var json = form[TicketFieldName];
    if (string.IsNullOrWhiteSpace(json))
    {
      ctx.ModelState.AddModelError(TicketFieldName, "JSON part is required.");
      ctx.Result = ModelBindingResult.Failed();
      return Task.CompletedTask;
    }

    TDto? dto;
    try
    {
      dto = JsonSerializer.Deserialize<TDto>(json!, _options);
    }
    catch (JsonException)
    {
      ctx.ModelState.AddModelError(TicketFieldName, "Invalid JSON.");
      ctx.Result = ModelBindingResult.Failed();
      return Task.CompletedTask;
    }

    var files = form.Files?.ToList() ?? new List<IFormFile>();

    ctx.Result = ModelBindingResult.Success(new JsonWithFiles<TDto>(dto!, files));
    return Task.CompletedTask;
  }
}

public sealed record JsonWithFiles<TDto>(TDto Payload, List<IFormFile> Files);
