﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SharedModel.Model
{
    public class Card
    {
        [Key] public Guid Id { get; init; }

        public string Name { get; init; } = null!;
        public Layout Layout { get; init; }
        public ICollection<Face> Faces { get; set; } = null!;

        public ICollection<Record> Records { get; init; } = null!;

        // Underlying DB representation has no 
        public void SortFaces()
        {
            Faces = Faces.OrderBy(face => face.Sequence).ToList();
        }

        public override string ToString()
        {
            return
                $"{nameof(Card)} ({nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Layout)}: {Layout}, {nameof(Faces)}: {Faces})";
        }
    }
}