// Infrastructure/Persistence/Converters/UtcDateTimeConverter.cs
using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MiniTicketing.Infrastructure.Persistence.Converters;

public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
  public UtcDateTimeConverter() : base(
      toDb => toDb.Kind == DateTimeKind.Utc ? toDb : DateTime.SpecifyKind(toDb, DateTimeKind.Utc),
      fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc))
  { }
}

public sealed class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter() : base(
        toDb   => toDb.HasValue ? DateTime.SpecifyKind(toDb.Value, DateTimeKind.Utc) : (DateTime?)null,
        fromDb => fromDb.HasValue ? DateTime.SpecifyKind(fromDb.Value, DateTimeKind.Utc) : (DateTime?)null)
    { }
}
