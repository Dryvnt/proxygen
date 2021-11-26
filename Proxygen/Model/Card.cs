using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Proxygen.Model
{
    public class Card
    {
        [Key] public Guid Id { get; init; }

        public string Name { get; set; }
        public Layout Layout { get; set; }
        public List<Face> Faces { get; set; }
        
        // Underlying DB representation has no 
        public void SortFaces()
        {
            Faces.Sort((a, b) => a.Sequence - b.Sequence);
        }

        public override string ToString()
        {
            return
                $"{nameof(Card)} ({nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Layout)}: {Layout}, {nameof(Faces)}: {Faces})";
        }
    }
}