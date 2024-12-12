namespace TinyDb.Library.Interfaces;

/// <summary>
/// Interface which defined an identifier contract for any entity wanting to use database functionality.
/// </summary>
public interface IWithId
{
    /// <summary>
    /// The database identifier of this entity.
    /// </summary>
    public int Id { get; set; }
}