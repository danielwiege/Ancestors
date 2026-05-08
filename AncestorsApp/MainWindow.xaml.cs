using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AncestorsApp.Rendering;
using AncestorsApp.Services;
using Microsoft.Win32;

namespace AncestorsApp;

public partial class MainWindow : Window
{
    private const double NodeWidth = 230;
    private const double NodeHeight = 108;
    private const double HorizontalSpacing = 42;
    private const double VerticalSpacing = 128;

    private readonly FamilyTreeEditingService _editingService = new();
    private readonly FamilyTreePersistenceService _persistenceService = new();
    private readonly TreeLayoutService _layoutService = new(NodeWidth, NodeHeight, HorizontalSpacing, VerticalSpacing);
    private readonly TreeRenderer _treeRenderer = new(NodeWidth, NodeHeight);

    private FamilyTreeDocument _document = new();
    private TreeLayoutResult? _currentLayout;
    private PersonNode? _selected;
    private string? _currentFilePath;
    private bool _hasUnsavedChanges;
    private bool _isRefreshingInputs;

    public MainWindow()
    {
        InitializeComponent();
        CreateNewDocument("Neuer Stammbaum angelegt.");
    }

    private void NewTree_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardUnsavedChanges())
        {
            return;
        }

        CreateNewDocument("Neuer Stammbaum angelegt.");
    }

    private void CreateNewDocument(string statusMessage)
    {
        _document = _editingService.CreateNewDocument();
        _currentFilePath = null;
        _hasUnsavedChanges = false;
        SelectNode(_document.Root);
        RenderTree();
        SetStatus(statusMessage);
    }

    private void SaveTree_Click(object sender, RoutedEventArgs e)
    {
        var targetPath = _currentFilePath ?? PromptForSavePath();
        if (targetPath is null)
        {
            return;
        }

        try
        {
            _persistenceService.SaveToFile(_document, targetPath);
            _currentFilePath = targetPath;
            _hasUnsavedChanges = false;
            UpdateUiState();
            SetStatus($"Gespeichert: {targetPath}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Der Stammbaum konnte nicht gespeichert werden.\n\n{ex.Message}", "Speichern fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string? PromptForSavePath()
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "Stammbaum (*.json)|*.json",
            FileName = "stammbaum.json"
        };

        return saveDialog.ShowDialog(this) == true ? saveDialog.FileName : null;
    }

    private void LoadTree_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardUnsavedChanges())
        {
            return;
        }

        var openDialog = new OpenFileDialog
        {
            Filter = "Stammbaum (*.json)|*.json"
        };

        if (openDialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var loaded = _persistenceService.LoadFromFile(openDialog.FileName);

            if (loaded?.Root is null)
            {
                MessageBox.Show("Die Datei enthält keinen gültigen Stammbaum.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _document = loaded;
            _currentFilePath = openDialog.FileName;
            _hasUnsavedChanges = false;
            SelectNode(_document.Root);
            RenderTree();
            SetStatus($"Geladen: {openDialog.FileName}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            MessageBox.Show($"Die Datei konnte nicht geladen werden.\n\n{ex.Message}", "Öffnen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddChild_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            return;
        }

        var child = _editingService.AddChild(_selected);
        SelectNode(child);
        MarkDocumentChanged();
        RenderTree();
        SetStatus("Kind hinzugefügt.");
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
            SetStatus("Die Wurzelperson hat keine Geschwister auf dieser Ebene.");
            return;
        }

        SelectNode(sibling);
        MarkDocumentChanged();
        RenderTree();
        SetStatus("Geschwisterperson hinzugefügt.");
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

        var result = MessageBox.Show(
            $"Person \"{GetDisplayName(_selected)}\" und alle darunterliegenden Personen löschen?",
            "Person löschen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var parent = _editingService.DeleteNode(_document, _selected);
        if (parent is null)
        {
            return;
        }

        SelectNode(parent);
        MarkDocumentChanged();
        RenderTree();
        SetStatus("Person gelöscht.");
    }

    private void Metadata_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshingInputs || _selected is null)
        {
            return;
        }

        var hasValidBirthDate = TryParseOptionalDate(BirthDateBox.Text, out var birthDate);
        var hasValidDeathDate = TryParseOptionalDate(DeathDateBox.Text, out var deathDate);

        _selected.FirstName = FirstNameBox.Text.Trim();
        _selected.LastName = LastNameBox.Text.Trim();
        _selected.BirthPlace = BirthPlaceBox.Text.Trim();
        _selected.Notes = NotesBox.Text.Trim();

        if (hasValidBirthDate)
        {
            _selected.BirthDate = birthDate;
        }

        if (hasValidDeathDate)
        {
            _selected.DeathDate = deathDate;
        }

        DateValidationText.Visibility = hasValidBirthDate && hasValidDeathDate ? Visibility.Collapsed : Visibility.Visible;
        MarkDocumentChanged();
        RenderTree();
    }

    private static bool TryParseOptionalDate(string input, out DateTime? date)
    {
        date = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return true;
        }

        if (!DateTime.TryParseExact(input.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return false;
        }

        date = parsed;
        return true;
    }

    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private void SelectNode(PersonNode? node)
    {
        _selected = node;
        RefreshEditor();
        UpdateUiState();
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
            DateValidationText.Visibility = Visibility.Collapsed;
        }
        else
        {
            FirstNameBox.Text = _selected.FirstName;
            LastNameBox.Text = _selected.LastName;
            BirthDateBox.Text = FormatDate(_selected.BirthDate);
            DeathDateBox.Text = FormatDate(_selected.DeathDate);
            BirthPlaceBox.Text = _selected.BirthPlace;
            NotesBox.Text = _selected.Notes;
            DateValidationText.Visibility = Visibility.Collapsed;
            SetStatus($"Ausgewählt: {GetDisplayName(_selected)}");
        }

        _isRefreshingInputs = false;
    }

    private void RenderTree()
    {
        if (_document.Root is null)
        {
            TreeCanvas.Children.Clear();
            _currentLayout = null;
            UpdateUiState();
            return;
        }

        var layout = _layoutService.Calculate(_document.Root);
        _currentLayout = layout;
        _treeRenderer.Render(TreeCanvas, layout, _selected, node =>
        {
            SelectNode(node);
            RenderTree();
        });

        UpdateUiState();
    }

    private void FocusSelection_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || _currentLayout?.Positions.TryGetValue(_selected, out var position) != true)
        {
            return;
        }

        var horizontalOffset = Math.Max(0, position.X - (TreeScrollViewer.ViewportWidth - NodeWidth) / 2);
        var verticalOffset = Math.Max(0, position.Y - (TreeScrollViewer.ViewportHeight - NodeHeight) / 2);

        TreeScrollViewer.ScrollToHorizontalOffset(horizontalOffset);
        TreeScrollViewer.ScrollToVerticalOffset(verticalOffset);
    }

    private void UpdateUiState()
    {
        var hasSelection = _selected is not null;
        AddChildButton.IsEnabled = hasSelection;
        AddSiblingButton.IsEnabled = hasSelection && _selected != _document.Root;
        DeletePersonButton.IsEnabled = hasSelection && _selected != _document.Root;
        FocusSelectionButton.IsEnabled = hasSelection;

        var personCount = CountPeople(_document.Root);
        var generationCount = CountGenerations(_document.Root);
        TreeSummaryText.Text = $"{personCount} {Pluralize(personCount, "Person", "Personen")} | {generationCount} {Pluralize(generationCount, "Generation", "Generationen")}";

        DirtyMarkerText.Text = _hasUnsavedChanges ? "Ungespeicherte Änderungen" : "Gespeichert";
        DirtyMarkerText.Foreground = _hasUnsavedChanges ? Brushes.DarkOrange : Brushes.SeaGreen;

        var fileName = _currentFilePath is null ? "Unbenannt" : Path.GetFileName(_currentFilePath);
        Title = $"{(_hasUnsavedChanges ? "* " : string.Empty)}{fileName} - Ancestors";

        UpdateSelectionSummary();
    }

    private void UpdateSelectionSummary()
    {
        if (_selected is null)
        {
            SelectedTitleText.Text = "Keine Auswahl";
            SelectedSubtitleText.Text = "Wählen Sie eine Person im Baum aus.";
            SelectedIdText.Text = string.Empty;
            return;
        }

        var childCount = _selected.Children.Count;
        var relationship = _selected == _document.Root
            ? "Wurzelperson"
            : _document.Root is null
                ? "Person"
                : $"Kind von {GetDisplayName(_editingService.FindParent(_document.Root, _selected))}";

        SelectedTitleText.Text = GetDisplayName(_selected);
        SelectedSubtitleText.Text = $"{relationship} | {childCount} {Pluralize(childCount, "Kind", "Kinder")}";
        SelectedIdText.Text = $"ID: {_selected.Id.ToString()[..8]}";
    }

    private void MarkDocumentChanged()
    {
        _hasUnsavedChanges = true;
        UpdateUiState();
    }

    private void SetStatus(string message)
    {
        StatusText.Text = message;
    }

    private bool ConfirmDiscardUnsavedChanges()
    {
        if (!_hasUnsavedChanges)
        {
            return true;
        }

        var result = MessageBox.Show(
            "Es gibt ungespeicherte Änderungen. Wirklich verwerfen?",
            "Ungespeicherte Änderungen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (!ConfirmDiscardUnsavedChanges())
        {
            e.Cancel = true;
        }
    }

    private static int CountPeople(PersonNode? node)
    {
        return node is null ? 0 : 1 + node.Children.Sum(CountPeople);
    }

    private static int CountGenerations(PersonNode? node)
    {
        return node is null ? 0 : 1 + node.Children.Select(CountGenerations).DefaultIfEmpty().Max();
    }

    private static string Pluralize(int count, string singular, string plural)
    {
        return count == 1 ? singular : plural;
    }

    private static string GetDisplayName(PersonNode? node)
    {
        return node is null || string.IsNullOrWhiteSpace(node.DisplayName)
            ? "Unbenannte Person"
            : node.DisplayName;
    }
}
