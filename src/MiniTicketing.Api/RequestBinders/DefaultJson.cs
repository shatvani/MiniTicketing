using System.Text.Json;

namespace MiniTicketing.Api.RequestBinders;

public static class DefaultJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}