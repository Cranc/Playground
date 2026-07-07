# Umsetzungsplan: Visueller Workflow-Editor

Ziel: Workflows im Editor zusammenklicken → als `.json` exportieren → unverändert von der bestehenden Engine laden.

Rahmenbedingungen: Blazor **Server**, Editor als **separates Projekt** `WorkflowEngine.Editor`. Reflection zur Laufzeit ist erlaubt, deshalb **kein Source Generator** — ein Assembly-Scan beim Start genügt.

Leitprinzip: **Verhalten bleibt im Code (Interface), Metadaten und Kanten werden zu Daten (Attribute + JSON).**

---

## Phasenüberblick

| Phase | Inhalt | Ergebnis |
|-------|--------|----------|
| 0 | Editor-Projekt + Solution-Setup | leeres, referenziertes `WorkflowEngine.Editor` |
| 1 | Metadaten (Attribute) + Katalog (Scan) | durchsuchbarer `BlockDescriptor`-Katalog, Auto-Registrierung |
| 2 | Outcome/Transition-Modell + Runner-Refactor | Workflows **vollständig** datengetrieben (Kanten im JSON) |
| 3 | JSON-Writer + Validator | Export mit Garantie „lädt wieder" |
| 4 | Editor-UI | Toolbox, Canvas, Linking, Validierung, Export |

Reihenfolge ist bindend: Phase 2 ist das Nadelöhr. Ohne Kanten in den Daten hat der Editor nichts zu verlinken.

---

## Phase 0 — Projektsetup

- Neues Projekt `WorkflowEngine.Editor` (Blazor Server) anlegen, in `Work.slnx` aufnehmen.
- Referenzen: `WorkflowEngine`, `WorkflowEngine.Blazor` (für Page-Preview optional), plus die Assembly mit den konkreten Bausteinen (`Work`) — der Editor muss den Katalog dieser Assembly scannen können.
- DevExpress-Blazor wie in `Work` einbinden (Pages nutzen `DxButton` etc.), falls Live-Preview der Pages gewünscht ist. Für reines Node-Editing nicht nötig.

Akzeptanz: Projekt startet, kann `WorkflowEngine`-Typen referenzieren.

---

## Phase 1 — Metadaten & Katalog

### 1.1 Neue Attribute (`WorkflowEngine/Catalog/`)

```csharp
namespace WorkflowEngine.Catalog;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class WorkflowBlockAttribute : Attribute
{
    public string? Alias { get; init; }        // Default: Klassenname (wie heute in der Registry)
    public string? DisplayName { get; init; }
    public string Category { get; init; } = "Allgemein";
    public string? Description { get; init; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ConsumesContextAttribute : Attribute
{
    public ConsumesContextAttribute(string key, Type type) { Key = key; Type = type; }
    public string Key { get; }
    public Type Type { get; }
    public bool Required { get; init; } = true;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ProducesContextAttribute : Attribute
{
    public ProducesContextAttribute(string key, Type type) { Key = key; Type = type; }
    public string Key { get; }
    public Type Type { get; }
}

// Benannte Ausgänge = die Ports, an denen der Editor Kanten anhängt.
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OutcomeAttribute : Attribute
{
    public OutcomeAttribute(string name) { Name = name; }
    public string Name { get; }
    public string? DisplayName { get; init; }
}
```

### 1.2 Descriptor-Modell (`WorkflowEngine/Catalog/`)

```csharp
public enum BlockKind { Action, Page }   // IWorkflowStep vs. WorkflowStepComponent

public sealed record ContextPort(string Key, Type Type, bool Required);
public sealed record OutcomePort(string Name, string DisplayName);

public sealed record BlockDescriptor(
    string Alias,
    BlockKind Kind,
    string DisplayName,
    string Category,
    string? Description,
    Type ImplementationType,
    IReadOnlyList<ContextPort> Inputs,     // aus ConsumesContext
    IReadOnlyList<ContextPort> Outputs,    // aus ProducesContext
    IReadOnlyList<OutcomePort> Outcomes);  // aus Outcome; Pages: implizit ein Default-Outcome
```

### 1.3 Katalog-Aufbau per Scan

```csharp
public interface IBlockCatalog
{
    IReadOnlyList<BlockDescriptor> Blocks { get; }
    bool TryGet(string alias, out BlockDescriptor descriptor);
}

public sealed class BlockCatalogBuilder
{
    public IBlockCatalog Build(params Assembly[] assemblies)
    {
        // 1. Alle Typen mit [WorkflowBlock] ODER die IWorkflowStep/WorkflowStepComponent sind.
        // 2. Kind bestimmen: typeof(IWorkflowStep).IsAssignableFrom(t) => Action,
        //    typeof(ComponentBase).IsAssignableFrom(t) => Page.
        // 3. Attribute in ContextPort/OutcomePort übersetzen.
        // 4. Alias = Attribut-Alias ?? t.Name (kompatibel zu heutigem Registry-Default).
        // 5. Duplikat-Aliase => aussagekräftige Exception.
    }
}
```

### 1.4 Registry aus Katalog speisen

Die manuellen `RegisterStep<>()/RegisterComponent()`-Blöcke in `DriverTourWorkflowConfig` / `UserWizardWorkflowConfig` entfallen. Neue Erweiterung auf `WorkflowTypeRegistry`:

```csharp
public static WorkflowTypeRegistry FromCatalog(IBlockCatalog catalog)
{
    var reg = new WorkflowTypeRegistry();
    foreach (var b in catalog.Blocks)
    {
        if (b.Kind == BlockKind.Action) reg.RegisterStep(b.ImplementationType, b.Alias);
        else                            reg.RegisterComponent(b.ImplementationType, b.Alias);
    }
    return reg;
}
```

Before/After-Aktionen und Exception-Handler bleiben wie heute manuell registriert (sie sind Delegates ohne Typ-Identität). Optional später auch als benannte Blöcke modellierbar.

### 1.5 DI

`ServiceCollectionExtensions.AddWorkflowEngine(...)` erweitern: Katalog einmalig bauen und als Singleton registrieren; optional `params Assembly[]` bzw. Marker-Typen entgegennehmen.

**Akzeptanz Phase 1**
- Katalog listet alle DriverTour- und UserWizard-Bausteine korrekt mit Kind/Category/Ports.
- Bestehende Workflows laden weiterhin (Registry aus Katalog erzeugt dieselben Aliase).
- Unit-Test: Scan der `Work`-Assembly liefert erwartete Anzahl + Ports je Baustein.

---

## Phase 2 — Outcome/Transition-Modell (Kern)

Heute steckt das Sprungziel im Code: `StepResult.Ok("StopArrival")`, `stop.AllShipmentsCompleted ? "StopComplete" : "ShipmentSelection"`. Das wird getrennt in **Entscheidung (Code)** und **Ziel (Daten)**.

### 2.1 `StepResult` erweitern (abwärtskompatibel)

```csharp
public string? Outcome { get; init; }   // NEU: benannter Ausgang

public static StepResult Branch(string outcome, object? result = null)
    => new() { Success = true, Outcome = outcome, Result = result };
// NextStep bleibt bestehen (Migrationspfad).
```

### 2.2 Modell + Definition um Transitions erweitern

`WorkflowStepConfigurationModel`:
```csharp
public Dictionary<string, string> Transitions { get; set; } = new(); // Outcome -> Ziel-Step
public string? Next { get; set; }                                     // unbedingter Default-Übergang
```
`WorkflowStepDefinition`: analoges `IReadOnlyDictionary<string,string> Transitions` + `string? Next`.

`WorkflowStepBuilder`:
```csharp
public WorkflowStepBuilder On(string outcome, string targetStep) { ... }
public WorkflowStepBuilder Next(string targetStep) { ... }
```
`WorkflowDefinitionFactory.Create(...)`: `stepModel.Transitions`/`Next` durchreichen.

### 2.3 Runner: Zielauflösung

In `WorkflowRunner.ExecuteAndMoveAsync` die Auflösungskette ersetzen durch:
```
next =
    overrideNextStep
    ?? ResolveOutcome(step, execution.StepResult?.Outcome)   // NEU: Transitions[outcome]
    ?? execution.ExceptionNextStep
    ?? execution.StepResult?.NextStep                         // Kompatibilität (Altcode)
    ?? step.Next;                                             // unbedingter Default
```
`ResolveOutcome` schlägt fehl, wenn ein Outcome zurückkommt, für das keine Transition existiert → klare Exception (fängt fehlerhaftes JSON früh ab).

### 2.4 Migration DriverTour

- `CompleteShipmentStep` gibt `StepResult.Branch("MoreShipments")` bzw. `Branch("StopComplete")` zurück statt konkreter Namen.
- Steps mit genau einem Nachfolger (`InitializeTourStep` → `"StopArrival"`) nutzen `Next` in der JSON.
- Pages: Der Ausgang, den heute `AdvanceAsync("ShipmentStatus")` setzt, wird zum benannten Outcome des Page-Bausteins bzw. bleibt als expliziter Zielname (Pages steuern die Navigation aktiv). Empfehlung: Pages deklarieren ihre möglichen Outcomes per `[Outcome]`, und `AdvanceAsync` bekommt eine Outcome-Variante:
  ```csharp
  protected Task AdvanceViaAsync(string outcome) => Runner.AdvanceByOutcomeAsync(outcome);
  ```
- `DriverTourWorkflow.json` um `transitions`/`next` ergänzen.

Beispiel JSON nach Migration:
```json
{
  "name": "CompleteShipment",
  "execute": "CompleteShipmentStep",
  "transitions": {
    "MoreShipments": "ShipmentSelection",
    "StopComplete":  "StopComplete"
  }
}
```

**Akzeptanz Phase 2**
- DriverTour + UserWizard laufen ausschließlich über JSON-Transitions (keine hartkodierten Zielnamen mehr im Step-Code).
- Regressionstest: kompletter Durchlauf beider Workflows identisch zu vorher.
- Runner wirft aussagekräftig bei unbekanntem Outcome.

---

## Phase 3 — JSON-Writer + Validator

### 3.1 Writer

```csharp
public interface IWorkflowConfigurationWriter
{
    string Write(WorkflowConfigurationModel model);
    void Write(Stream target, WorkflowConfigurationModel model);
}

public sealed class JsonWorkflowConfigurationWriter : IWorkflowConfigurationWriter
{
    // System.Text.Json, camelCase, Indented, leere Transitions/Next weglassen.
}
```
Spiegelbild zum vorhandenen `JsonWorkflowConfigurationReader` — gleiche Optionen, damit Round-Trip verlustfrei ist.

### 3.2 Editor-Layout (Sidecar)

Runtime-Config bleibt sauber. Positionen kommen in eine separate Datei bzw. einen ignorierten Block:
```csharp
public sealed record NodeLayout(string StepName, double X, double Y);
public sealed record WorkflowEditorLayout(string WorkflowName, List<NodeLayout> Nodes);
// Datei: DriverTourWorkflow.layout.json
```

### 3.3 Validator (nutzt Katalog)

```csharp
public sealed record ValidationIssue(Severity Severity, string? StepName, string Message);

public sealed class WorkflowModelValidator
{
    public IReadOnlyList<ValidationIssue> Validate(WorkflowConfigurationModel model, IBlockCatalog catalog);
}
```
Regeln:
1. StartStep existiert; jeder Transition-/Next-Zielname existiert.
2. Jeder referenzierte Alias ist im Katalog.
3. **Datenfluss:** Für jeden Baustein muss jeder `Required`-Input von einem Vorgänger auf *allen* eingehenden Pfaden produziert werden (Graph-Traversierung ab StartStep). → das ist die „ohne Input nicht erzeugbar"-Regel.
4. Jeder deklarierte Outcome eines Bausteins ist verdrahtet; keine Transition auf unbekannten Outcome.
5. Warnungen: nicht erreichbare Steps, Steps ohne ausgehende Kante (außer bewusstes Ende).

**Export-Garantie:** Editor ruft vor dem Speichern `WorkflowModelValidator` + einmal `WorkflowDefinitionFactory.Create(model, registry)` auf. Läuft das durch, lädt das JSON garantiert.

**Akzeptanz Phase 3**
- Round-Trip-Test: `read(write(model)) == model`.
- Validator erkennt fehlenden Input, toten Zielnamen, unerreichbaren Step in Testfällen.

---

## Phase 4 — Editor-UI (`WorkflowEngine.Editor`)

### 4.1 Editor-seitiges Graph-Modell (getrennt vom Config-Modell)

```csharp
public sealed class EditorNode { public string Id; public string Alias; public double X, Y; }
public sealed class EditorEdge { public string FromNodeId; public string Outcome; public string ToNodeId; }
public sealed class EditorGraph { public string Name; public string? StartNodeId;
                                  public List<EditorNode> Nodes; public List<EditorEdge> Edges; }
```
Mapper in beide Richtungen: `EditorGraph ⇄ (WorkflowConfigurationModel + WorkflowEditorLayout)`. Step-`Name` im Config-Modell = `EditorNode.Id`.

### 4.2 Komponenten

- `Toolbox.razor` — `IBlockCatalog.Blocks` nach `Category` gruppiert; Drag auf Canvas erzeugt Node.
- `EditorCanvas.razor` — SVG-Fläche; rendert Nodes + Edges; hält Pan/Zoom.
- `BlockNode.razor` — Kachel mit Inputs links (Consumes), Outcomes rechts (Ports); Titel = DisplayName.
- `ConnectionLayer` — Edges als SVG-`<path>` (Bézier) zwischen Outcome-Port und Ziel-Node.
- `PropertyPanel.razor` — Retry, OnExceptionNextStep, Alias-Auswahl, StartStep-Markierung.
- `ValidationPanel.razor` — Live-Liste aus `WorkflowModelValidator`.

### 4.3 Interaktion

- **Verschieben:** Pointer-Events; für flüssiges Draggen dünnes JS-Interop-Modul (`editor.js`) das Positionen zurückschreibt — Blazor Server sonst zu latenzempfindlich bei jedem Mousemove.
- **Linken:** Mousedown auf Outcome-Port → temporäre Linie → Mouseup auf Ziel-Node erzeugt `EditorEdge`.
- **Input-Validierung beim Linken/Platzieren:** Beim Verbinden prüft der Editor sofort, ob die Required-Inputs des Zielbausteins durch die eingehenden Pfade gedeckt sind; unerfüllt → Port/Node rot + Eintrag im ValidationPanel. Ein Baustein mit ungedecktem Required-Input bleibt „ungültig" markiert und blockiert den Export.

### 4.4 Export

`EditorGraph → Model + Layout → Validator → (bei OK) Writer → Datei`. Speichern nach `wwwroot/json/{Name}.json` + `{Name}.layout.json`, plus Download-Button.

**Akzeptanz Phase 4**
- DriverTour lässt sich im Editor öffnen (aus JSON + Layout), umbauen, exportieren; das Ergebnis lädt und läuft.
- Baustein mit fehlendem Required-Input kann nicht exportiert werden.

---

## Offene Entscheidungen

1. **Context-Keys:** vorerst nur per Attribut deklarieren (kleiner Eingriff) — oder gleich auf getypte `ContextKey`-Definitionen umstellen (sauberer, breiter). Empfehlung: erst deklarieren, Umstellung als eigener Schritt.
2. **Pages-Navigation:** Pages steuern heute aktiv via `AdvanceAsync(zielName)`. Entweder auf benannte Outcomes umstellen (konsistent, editor-freundlich) oder Pages dürfen weiterhin freie Ziele setzen (dann sind ihre Kanten im Editor nicht vollständig darstellbar). Empfehlung: benannte Outcomes.
3. **Layout:** Sidecar-Datei (empfohlen) vs. `editor`-Block in derselben JSON.

## Risiken

- Pages mit dynamischer Navigation lassen sich nicht rein deklarativ abbilden — Outcome-Umstellung nötig, sonst „unsichtbare" Kanten.
- Datenfluss-Validierung bei Zyklen/Merge-Punkten: „auf allen Pfaden produziert" sauber definieren (Fixpunkt-Iteration über den Graphen).
- Blazor-Server-Latenz beim Draggen → JS-Interop für die reine Mausbewegung.
