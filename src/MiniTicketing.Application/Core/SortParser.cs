using System.Globalization;
using MiniTicketing.Application.Abstractions.Services;

namespace MiniTicketing.Application.Core;
public static class SortParser
{
  // Engedélyezett mezők (dugó a “reflection injection” ellen)
  private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdAt", "priority", "status", "title"
    };

  // Aliasok: támogatjuk a "-createdAt" szintaxist is
  public static IReadOnlyList<SortBy> Parse(string? input)
  {
    if (string.IsNullOrWhiteSpace(input))
      return new[] { new SortBy("createdAt", Desc: true) }; // default: createdAt desc

    var result = new List<SortBy>();
    var parts = input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    foreach (var raw in parts)
    {
      var token = raw.Trim();

      bool desc;
      string field;

      // 1) "-field" forma
      if (token.StartsWith("-", StringComparison.Ordinal))
      {
        desc = true;
        field = token.Substring(1).Trim();
      }
      else
      {
        // 2) "field desc/asc" forma
        var chunks = token.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        field = chunks[0];
        desc = chunks.Length > 1 && chunks[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
      }

      // validálás
      if (!Allowed.Contains(field))
        continue; // ismeretlen mezőt kihagyjuk (nem dobunk hibát)

      result.Add(new SortBy(field, desc));
    }

    // ha minden érvénytelen volt, essen vissza a default-ra
    return result.Count > 0 ? result : new[] { new SortBy("createdAt", Desc: true) };
  }
}

