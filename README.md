# Ancestors

Ancestors ist eine WPF-Anwendung zum Erstellen, Bearbeiten und Speichern eines Stammbaums.

## Funktionen

- Moderne Hauptansicht mit Baumansicht, Detailbereich und Statusleiste
- Grafische Stammbaum-Darstellung mit auswählbaren Personenkarten
- Hinzufügen und Löschen von Personen:
  - Kind hinzufügen
  - Geschwister hinzufügen
  - Person inklusive untergeordneter Personen löschen
- Bearbeitung von Personendaten:
  - Vorname, Nachname
  - Geburts- und Sterbedatum im Format `TT.MM.JJJJ`
  - Geburtsort
  - Notizen
- Rückfrage bei ungespeicherten Änderungen
- Speichern und Laden als JSON-Datei

## Projektstruktur

- `Ancestors.sln`: Visual-Studio-Solution
- `AncestorsApp/AncestorsApp.csproj`: WPF-Projekt für `.NET 8`
- `AncestorsApp/MainWindow.xaml`: Hauptoberfläche
- `AncestorsApp/MainWindow.xaml.cs`: Bedienlogik der Oberfläche
- `AncestorsApp/Services/FamilyTreeEditingService.cs`: Baumoperationen
- `AncestorsApp/Services/FamilyTreePersistenceService.cs`: JSON Laden/Speichern
- `AncestorsApp/Rendering/TreeLayoutService.cs`: Berechnung der Baumpositionen
- `AncestorsApp/Rendering/TreeRenderer.cs`: Zeichnen von Personen und Verbindungen

## Starten

Auf Windows mit installiertem .NET SDK:

```bash
dotnet run --project AncestorsApp/AncestorsApp.csproj
```

Oder über die Solution:

```bash
dotnet build Ancestors.sln
```
