using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace SharedModel.Model;

public enum UpdateStatusState
{
    Begun = 0,
    Success = 1,
    Failure = 2,
}

public class UpdateStatus
{
    [Key]
    public int Id { get; init; }

    public required Instant Created { get; init; }
    public UpdateStatusState StatusState { get; set; }
}
