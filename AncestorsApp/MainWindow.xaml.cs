using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using AncestorsApp.Rendering;
using AncestorsApp.Services;
using Microsoft.Win32;

namespace AncestorsApp;

public partial class MainWindow : Window
{
    private const double NodeWidth = 200;
    private const double NodeHeight = 90;
    private const double HorizontalSpacing = 35;
    private const double VerticalSpacing = 120;

    private readonly FamilyTreeEditingService _editingService = new();
    private readonly FamilyTreePersistenceService _persistenceService = new();
    private readonly TreeLayoutService _layoutService = new(NodeWidth, NodeHeight, HorizontalSpacing, VerticalSpacing);
    private readonly TreeRenderer _treeRenderer = new(NodeWidth, NodeHeight);

    private FamilyTreeDocument _document = new();
    private PersonNode? _selected;
    private bool _isRefreshingInputs;

    public MainWindow()
    {
        InitializeComponent();
        CreateNewDocument();
    }

    private void NewTree_Click(object sender, RoutedEventArgs e) => CreateNewDocument();

    private void CreateNewDocument()
    {
        _document = _editingService.CreateNewDocument();
        SelectNode(_document.Root);
        RenderTree();
    }

    private void SaveTree_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "Stammbaum (*.json)|*.json",
            FileName = "stammbaum.json"
        };

        if (saveDialog.ShowDialog() != true)
        {
            return;
        }

        _persistenceService.SaveToFile(_document, saveDialog.FileName);
        StatusText.Text = $"Gespeichert: {saveDialog.FileName}";
    }

    private void LoadTree_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "Stammbaum (*.json)|*.json"
        };

        if (openDialog.ShowDialog() != true)
        {
            return;
        }

        var loaded = _persistenceService.LoadFromFile(openDialog.FileName);

        if (loaded?.Root is null)
        {
            MessageBox.Show("Die Datei enthält keinen gültigen Stammbaum.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _document = loaded;
        SelectNode(_document.Root);
        RenderTree();
        StatusText.Text = $"Geladen: {openDialog.FileName}";
    }

    private void AddChild_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            return;
        }

        var child = _editingService.AddChild(_selected);
        SelectNode(child);
        RenderTree();
    }

    private void AddSibling_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            return;
        }

        var sibling = _editingService.AddSibling(_document, _selected);
        if (sibling is null)
        {
            StatusText.Text = "Die Wurzelperson hat keine Geschwister auf dieser Ebene.";
            return;
        }

        SelectNode(sibling);
        RenderTree();
    }

    private void DeletePerson_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            return;
        }

        if (_selected == _document.Root)
        {
            MessageBox.Show("Die Wurzelperson kann nicht gelöscht werden.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var parent = _editingService.DeleteNode(_document, _selected);
        if (parent is null)
        {
            return;
        }

        SelectNode(parent);
        RenderTree();
    }

    private void Metadata_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshingInputs || _selected is null)
        {
            return;
        }

        _selected.FirstName = FirstNameBox.Text.Trim();
        _selected.LastName = LastNameBox.Text.Trim();
        _selected.BirthPlace = BirthPlaceBox.Text.Trim();
        _selected.Notes = NotesBox.Text.Trim();
        _selected.BirthDate = ParseDate(BirthDateBox.Text);
        _selected.DeathDate = ParseDate(DeathDateBox.Text);

        RenderTree();
    }

    private static DateTime? ParseDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return DateTime.TryParseExact(input.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private void SelectNode(PersonNode? node)
    {
        _selected = node;
        RefreshEditor();
    }

    private void RefreshEditor()
    {
        _isRefreshingInputs = true;

        if (_selected is null)
        {
            FirstNameBox.Text = string.Empty;
            LastNameBox.Text = string.Empty;
            BirthDateBox.Text = string.Empty;
            DeathDateBox.Text = string.Empty;
            BirthPlaceBox.Text = string.Empty;
            NotesBox.Text = string.Empty;
            StatusText.Text = "Keine Person ausgewählt.";
        }
        else
        {
            FirstNameBox.Text = _selected.FirstName;
            LastNameBox.Text = _selected.LastName;
            BirthDateBox.Text = FormatDate(_selected.BirthDate);
            DeathDateBox.Text = FormatDate(_selected.DeathDate);
            BirthPlaceBox.Text = _selected.BirthPlace;
            NotesBox.Text = _selected.Notes;
            StatusText.Text = $"Ausgewählt: {_selected.DisplayName} ({_selected.Id.ToString()[..8]})";
        }

        _isRefreshingInputs = false;
    }

    private void RenderTree()
    {
        if (_document.Root is null)
        {
            TreeCanvas.Children.Clear();
            return;
        }

        var layout = _layoutService.Calculate(_document.Root);
        _treeRenderer.Render(TreeCanvas, layout, _selected, node =>
        {
            SelectNode(node);
            RenderTree();
        });
    }
}
