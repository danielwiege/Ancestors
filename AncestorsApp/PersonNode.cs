using System.Collections.ObjectModel;

namespace AncestorsApp;

public class PersonNode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = "Neue";
    public string LastName { get; set; } = "Person";
    public DateTime? BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public string BirthPlace { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public ObservableCollection<PersonNode> Children { get; set; } = [];

    public string DisplayName => $"{FirstName} {LastName}".Trim();
}
