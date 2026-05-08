namespace AncestorsApp.Services;

public class FamilyTreeEditingService
{
    public FamilyTreeDocument CreateNewDocument()
    {
        return new FamilyTreeDocument
        {
            Root = new PersonNode { FirstName = "Stammvater", LastName = "Familie" }
        };
    }

    public PersonNode AddChild(PersonNode parent)
    {
        var child = new PersonNode
        {
            FirstName = "Neues",
            LastName = "Kind"
        };

        parent.Children.Add(child);
        return child;
    }

    public PersonNode? AddSibling(FamilyTreeDocument document, PersonNode selected)
    {
        if (document.Root is null)
        {
            return null;
        }

        var parent = FindParent(document.Root, selected);
        if (parent is null)
        {
            return null;
        }

        var sibling = new PersonNode
        {
            FirstName = "Neues",
            LastName = "Geschwister"
        };

        parent.Children.Add(sibling);
        return sibling;
    }

    public PersonNode? DeleteNode(FamilyTreeDocument document, PersonNode selected)
    {
        if (document.Root is null || document.Root == selected)
        {
            return null;
        }

        var parent = FindParent(document.Root, selected);
        if (parent is null)
        {
            return null;
        }

        parent.Children.Remove(selected);
        return parent;
    }

    public PersonNode? FindParent(PersonNode current, PersonNode target)
    {
        foreach (var child in current.Children)
        {
            if (child == target)
            {
                return current;
            }

            var nestedParent = FindParent(child, target);
            if (nestedParent is not null)
            {
                return nestedParent;
            }
        }

        return null;
    }
}
