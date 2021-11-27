using System;
using System.ComponentModel.DataAnnotations;

namespace SharedModel.Model
{
    public enum UpdateStatus
    {
        Begun = 0,
        Success = 1,
        Failure = 2,
    }

    public class Update
    {
        [Key] public Guid Id { get; init; }
        public DateTime When { get; init; }
        public UpdateStatus Status { get; set; }
    }
}