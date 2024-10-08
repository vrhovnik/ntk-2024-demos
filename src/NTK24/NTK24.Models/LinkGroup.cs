﻿namespace NTK24.Models;

public class LinkGroup
{
    public Guid LinkGroupId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ShortName { get; set; }
    public SulUser User { get; set; }
    public int Clicked { get; set; }
    public Category Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Link> Links { get; set; } = new();
}