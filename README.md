# Ancestors

Diese Repository enthält eine WPF-Anwendung zum Erstellen und Speichern eines Stammbaums.

## Enthaltene Funktionen

- Ribbon-Menüleiste für Datei- und Bearbeitungsaktionen
- Organigramm-ähnliche, grafische Darstellung des Stammbaums
- Hinzufügen und Löschen von Personen (Kind/Geschwister)
- Bearbeitung von Metadaten pro Person:
  - Vorname, Nachname
  - Geburts- und Sterbedatum
  - Geburtsort
  - Notizen
- Speichern/Laden als JSON-Datei

## Projekt

- `AncestorsApp/AncestorsApp.csproj` (WPF, `net8.0-windows`)
- `AncestorsApp/Services/`
  - `FamilyTreeEditingService.cs`: Baumoperationen (Neu, hinzufügen, löschen)
  - `FamilyTreePersistenceService.cs`: JSON Laden/Speichern
- `AncestorsApp/Rendering/`
  - `TreeLayoutService.cs`: Berechnung der Organigramm-Positionen
  - `TreeRenderer.cs`: Zeichnen von Knoten und Verbindungen auf dem Canvas

## Starten (auf Windows mit installiertem .NET SDK)

```bash
dotnet run --project AncestorsApp/AncestorsApp.csproj
```
