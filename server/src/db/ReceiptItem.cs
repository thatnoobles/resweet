using System;
using System.Collections.Generic;
using Resweet.Database;

namespace Resweet.Entities;

public class ReceiptItem : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public double Price { get; set; }

    public void BuildFromFields(object[] fields)
    {
        Id = (Guid)fields[0];
        Name = (string)fields[1];
        Price = (double)fields[2];
    }

    public string ToJson() => $"{{\"id\":\"{Id}\",\"name\":\"{Name}\",\"price\":{Price}}}";
}