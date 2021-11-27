using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedModel.Model
{
    public class Record
    {
        [Key] public Guid Id { get; init; }
        public DateTime When { get; init; }
        public ICollection<Card> Cards { get; init; } = null!;
        public List<string> UnrecognizedCards { get; init; } = null!;
    }
}