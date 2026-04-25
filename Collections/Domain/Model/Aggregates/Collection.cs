namespace Frock_backend.Collections.Domain.Model.Aggregates;

public class Collection
{
    public int Id { get; }
    public string Name { get; set; } = string.Empty;
    public int FkIdUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CollectionItem> Items { get; set; } = new();

    protected Collection() { CreatedAt = DateTime.UtcNow; }

    public Collection(string name, int fkIdUser)
    {
        Name = name;
        FkIdUser = fkIdUser;
        CreatedAt = DateTime.UtcNow;
    }
}

public class CollectionItem
{
    public int Id { get; set; }
    public int FkIdCollection { get; set; }
    public int FkIdRoute { get; set; }
    public DateTime AddedAt { get; set; }

    protected CollectionItem() { AddedAt = DateTime.UtcNow; }

    public CollectionItem(int fkIdCollection, int fkIdRoute)
    {
        FkIdCollection = fkIdCollection;
        FkIdRoute = fkIdRoute;
        AddedAt = DateTime.UtcNow;
    }
}
