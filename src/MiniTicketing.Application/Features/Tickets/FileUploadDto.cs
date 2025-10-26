namespace MiniTicketing.Application.Features.Tickets;

public class FileUploadDto
{
  public required string FileName { get; set; }
  public required byte[] Content { get; set; }
  public required string ContentType { get; set; }
}