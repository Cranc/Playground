# Plan: Konfigurierbare WorkflowEngine (JSON-Konfiguration)

## Ziel

Eine komplette Workflow-Definition — wie sie heute in `DriverTourWorkflow.cs` per
Fluent-Builder gebaut wird — soll aus einer externen JSON-Datei geladen werden können.
Die Umsetzung ist **generisch** (funktioniert mit jedem zukünftigen Workflow), DriverTour
dient nur als Demo/Referenz.

**Entscheidungen (abgestimmt):**
- Typ-Auflösung über eine **Registry mit Aliasen** (kein Reflection-Load beliebiger Typen).
- **JSON zuerst**, aber format-neutrales DTO-Modell, damit XML später ohne Umbau ergänzbar ist.
- Erster Schritt bildet ab, was Builder-Workflows heute deklarativ nutzen: Steps mit
  `Execute`-Typ, `Component`-Typ, `Retry`, `OnException → Step`.

## Wichtige Randbedingung: dynamische Transitions

Die Verzweigung/Reihenfolge (`NextStep`) liegt heute **im Code**:
- in `IWorkflowStep`-Klassen (`return StepResult.Ok("StopArrival")`)
- in Blazor-Seiten (`AdvanceAsync("StopComplete")`)

Die Config bildet deshalb zunächst die **Struktur** ab (welche Steps existieren, welcher
Typ/welche Component dahinter steht, Startstep, Retry, Exception-Sprung). Die
Ablauflogik bleibt in den referenzierten Typen. Für DriverTour ist das vollständig
ausreichend, da dort alle Transitions in den Steps/Pages liegen.

Der spätere Ausbau zu **voll deklarativen, user-konfigurierbaren Transitions**
("wann welcher Step abgeschlossen wird") ist als Phase 3 vorgesehen und im Modell
vorbereitet, ohne die Phase-1-Struktur zu brechen.

## Bestehende Regeln, die eingehalten werden

- `net10.0`, `Nullable enable`, `ImplicitUsings enable`
- File-scoped Namespaces, 2-Space-Einrückung
- Deutsche XML-Doc-Kommentare und deutsche Exception-Meldungen
- `sealed` Klassen, `StringComparer.Ordinal` bei Step-Dictionaries
- Validierung zentral in `WorkflowBuilder.Build()` — wird wiederverwendet, nicht dupliziert
- Keine neuen NuGet-Pakete (System.Text.Json ist Teil des Frameworks)

---

## Architektur

Neue Bausteine liegen im Kernprojekt `WorkflowEngine` unter `Configuration/`.
Die Config wird **nicht** direkt zu einer `WorkflowDefinition` — sie durchläuft den
bestehenden `WorkflowBuilder`, sodass die gesamte vorhandene Validierung greift.

```
JSON-Datei
   │  (System.Text.Json)
   ▼
IWorkflowConfigurationReader  ──►  WorkflowConfigurationModel   (format-neutrales DTO)
   ▲                                        │
JsonWorkflowConfigurationReader             │  (Aliase → Type via Registry)
(XmlReader später, gleiches DTO)            ▼
                              WorkflowDefinitionFactory
                                            │  (nutzt WorkflowBuilder + Registry)
                                            ▼
                                   WorkflowDefinition   (unverändert, wie heute)
```

### 1. Format-neutrales DTO-Modell

`Configuration/WorkflowConfigurationModel.cs`

```csharp
namespace WorkflowEngine.Configuration;

/// <summary>Serialisierbare, format-neutrale Beschreibung eines Workflows.</summary>
public sealed class WorkflowConfigurationModel
{
  public string Name { get; set; } = string.Empty;
  public string StartStep { get; set; } = string.Empty;
  public List<WorkflowStepConfigurationModel> Steps { get; set; } = new();
}

/// <summary>Beschreibung eines einzelnen Steps. Typen werden als Alias referenziert.</summary>
public sealed class WorkflowStepConfigurationModel
{
  public string Name { get; set; } = string.Empty;
  public string? Execute { get; set; }              // Alias eines IWorkflowStep-Typs
  public string? Component { get; set; }             // Alias eines Component-Typs
  public int Retry { get; set; }
  public string? OnExceptionNextStep { get; set; }   // deklarativer Exception-Sprung
  // Phase 3 (vorbereitet, jetzt ungenutzt): Next / Transitions
}
```

- Suffix `...Model`, um eine Kollision mit der bereits privaten `WorkflowStepConfiguration`
  in `WorkflowBuilder` zu vermeiden.
- Reines POCO ohne Framework-Attribute → jeder Reader (JSON/XML) kann es befüllen.

### 2. Typ-Registry (Alias → Type)

`Configuration/IWorkflowTypeRegistry.cs` + `WorkflowTypeRegistry.cs`

```csharp
public interface IWorkflowTypeRegistry
{
  bool TryResolveStep(string alias, out Type stepType);
  bool TryResolveComponent(string alias, out Type componentType);
}
```

`WorkflowTypeRegistry` (konkret, `sealed`):
- Zwei `Dictionary<string, Type>` (Steps / Components), `StringComparer.Ordinal`.
- `RegisterStep<TStep>(string? alias = null)` — Alias default = `typeof(TStep).Name`.
  Validiert, dass `TStep : IWorkflowStep`.
- `RegisterComponent(Type componentType, string? alias = null)` — Alias default = `Name`.
- Deutsche Exceptions bei Doppelregistrierung / unbekanntem Alias.

Vorteil: Config referenziert nur kurze, stabile Namen (z.B. `"InitializeTourStep"`),
kein assembly-qualifizierter Typname, kein unsicherer Reflection-Load. Der Default-Alias
= Klassenname macht die DriverTour-Demo 1:1 zur bestehenden `.cs`.

### 3. Kleine Ergänzung am Builder

`WorkflowStepBuilder` besitzt heute nur das generische `Execute<TStep>()`. Für die
Config-getriebene Erzeugung wird eine nicht-generische Überladung benötigt:

```csharp
public WorkflowStepBuilder Execute(Type stepType)   // validiert IWorkflowStep, setzt StepType
```

`Component(Type)` und `Retry(int)` existieren bereits und werden wiederverwendet.
Kein Bruch bestehender API.

### 4. Factory: Model → WorkflowDefinition

`Configuration/WorkflowDefinitionFactory.cs` (`sealed`)

```csharp
public WorkflowDefinition Create(
  WorkflowConfigurationModel model,
  IWorkflowTypeRegistry registry)
```

Ablauf:
1. `new WorkflowBuilder(model.Name).StartWith(model.StartStep)`
2. Pro Step: `builder.Step(name)`
   - `Execute`-Alias gesetzt → `registry.TryResolveStep` → `.Execute(type)`
   - `Component`-Alias gesetzt → `registry.TryResolveComponent` → `.Component(type)`
   - `Retry > 0` → `.Retry(retry)`
   - `OnExceptionNextStep` gesetzt → `.OnException((ctx, ex) => next)`
     (deklarativer Wrapper um den vorhandenen Delegate-Mechanismus)
3. `builder.Build()` → sämtliche bestehende Validierung (Startstep existiert,
   keine Duplikate, mindestens Execute/Component pro Step) greift automatisch.
4. Unbekannte Aliase → klare deutsche `InvalidOperationException` mit Stepname + Alias.

### 5. Reader (JSON zuerst)

`Configuration/IWorkflowConfigurationReader.cs`

```csharp
public interface IWorkflowConfigurationReader
{
  WorkflowConfigurationModel Read(Stream content);
  WorkflowConfigurationModel Read(string content);
}
```

`Configuration/JsonWorkflowConfigurationReader.cs`:
- `System.Text.Json` mit `PropertyNameCaseInsensitive = true` (camelCase-tolerant).
- Nur Deserialisierung in das DTO — keine Fachlogik.
- XML später: `XmlWorkflowConfigurationReader` erzeugt dasselbe DTO → Factory unverändert.

### 6. Komfort-Fassade

`Configuration/WorkflowConfigurationLoader.cs` (`sealed`) bündelt Reader + Factory:

```csharp
public WorkflowDefinition LoadFromJson(string json, IWorkflowTypeRegistry registry);
public WorkflowDefinition LoadFromFile(string path, IWorkflowTypeRegistry registry);
```

Damit ist der End-to-End-Aufruf ein Einzeiler.

---

## Demo: DriverTour aus JSON

`Work/Workflows/DriverTour/DriverTourWorkflow.json`

```json
{
  "name": "DriverTour",
  "startStep": "TourStart",
  "steps": [
    { "name": "TourStart",             "execute": "InitializeTourStep" },
    { "name": "StopArrival",           "component": "StopArrivalPage" },
    { "name": "ShipmentSelection",     "component": "ShipmentSelectionPage" },
    { "name": "ShipmentStatus",        "component": "ShipmentStatusPage" },
    { "name": "Signature",             "component": "SignaturePage" },
    { "name": "CompleteShipment",      "execute": "CompleteShipmentStep" },
    { "name": "BulkShipmentStatus",    "component": "BulkShipmentStatusPage" },
    { "name": "CompleteBulkShipments", "execute": "CompleteBulkShipmentsStep" },
    { "name": "StopComplete",          "execute": "CompleteStopStep" },
    { "name": "LoadCarrierBooking",    "component": "LoadCarrierBookingPage", "execute": "AdvanceStopStep" },
    { "name": "TourEnd",               "component": "TourEndPage" }
  ]
}
```

Registrierung (einmalig, z.B. in `Program.cs` oder einem `DriverTourRegistration`-Helper):

```csharp
var registry = new WorkflowTypeRegistry();
registry.RegisterStep<InitializeTourStep>();
registry.RegisterStep<CompleteShipmentStep>();
registry.RegisterStep<CompleteBulkShipmentsStep>();
registry.RegisterStep<CompleteStopStep>();
registry.RegisterStep<AdvanceStopStep>();
registry.RegisterComponent(typeof(StopArrivalPage));
// ... übrige Pages
```

`DriverTourWorkflow.Build()` bleibt erhalten (Code-Variante). Ergänzend eine
`BuildFromConfig(...)`-Variante, sodass beide Wege 1:1 vergleichbar sind.

---

## Umsetzungsschritte

1. `WorkflowConfigurationModel` + `WorkflowStepConfigurationModel` anlegen.
2. `IWorkflowTypeRegistry` + `WorkflowTypeRegistry` inkl. `Register...`-Methoden.
3. `WorkflowStepBuilder.Execute(Type)` ergänzen (nicht-generische Überladung).
4. `WorkflowDefinitionFactory` (Model + Registry → `WorkflowDefinition` via Builder).
5. `IWorkflowConfigurationReader` + `JsonWorkflowConfigurationReader`.
6. `WorkflowConfigurationLoader` (Fassade Reader + Factory).
7. Demo: `DriverTourWorkflow.json` + Registrierungs-Helper + `BuildFromConfig`.
8. **Verifikation** (siehe unten).

## Verifikation

- Round-Trip-Test: JSON laden → `WorkflowDefinition` erzeugen und mit dem Ergebnis von
  `DriverTourWorkflow.Build()` vergleichen (gleiche Stepnamen, gleicher StartStep,
  gleiche `StepType`/`ComponentType` pro Step). Muss identisch sein.
- Negativfälle: unbekannter Alias, fehlender StartStep, Step ohne Execute/Component,
  doppelter Stepname → jeweils klare deutsche Exception.
- DriverTour aus JSON im `WorkflowWizard` durchklicken (Rendering + Ablauf identisch zur
  Code-Variante).

## Erweiterbarkeit / Ausblick

- **Phase 2 — XML:** `XmlWorkflowConfigurationReader` ergänzen; DTO + Factory unverändert.
- **Phase 3 — deklarative Transitions:** `WorkflowStepConfigurationModel` um `Next` bzw.
  `Transitions` (Bedingung → Zielstep) erweitern. Erfordert, dass die Engine Transitions
  aus der Definition statt aus `StepResult` lesen kann — additive Erweiterung von
  `WorkflowStepDefinition`/`WorkflowRunner`, bestehendes Verhalten bleibt Default.
- **Phase 4 — Ad-hoc/User-Konfiguration:** Registry + JSON-Persistenz erlauben, dass ein
  User Steps/Reihenfolge/Bedingungen zur Laufzeit anpasst und als JSON speichert
  (baut auf vorhandener `IWorkflowStorage`-Infrastruktur auf).
```

