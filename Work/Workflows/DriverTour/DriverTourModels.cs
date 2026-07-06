namespace Work.Workflows.DriverTour;

public enum ShipmentStatus
{
  ErfolgreichZugestellt,
  PackstueckBeschaedigt,
  KundeNichtAngetroffen,
  AnnahmeVerweigert,
  TeilweiseZugestellt,
  Sonstiges
}

public static class ShipmentStatusInfo
{
  public sealed record Option(ShipmentStatus Status, string Label, bool RequiresSignature);

  public static readonly IReadOnlyList<Option> Options =
  [
    new(ShipmentStatus.ErfolgreichZugestellt, "Erfolgreich zugestellt", true),
    new(ShipmentStatus.PackstueckBeschaedigt, "Packstück beschädigt", false),
    new(ShipmentStatus.KundeNichtAngetroffen, "Kunde nicht angetroffen", false),
    new(ShipmentStatus.AnnahmeVerweigert, "Annahme verweigert", false),
    new(ShipmentStatus.TeilweiseZugestellt, "Teilweise zugestellt", true),
    new(ShipmentStatus.Sonstiges, "Sonstiger Status", false)
  ];

  public static string LabelFor(ShipmentStatus status) =>
    Options.FirstOrDefault(o => o.Status == status)?.Label ?? status.ToString();

  public static bool RequiresSignature(ShipmentStatus status) =>
    Options.FirstOrDefault(o => o.Status == status)?.RequiresSignature ?? false;
}

public sealed class Shipment
{
  public required string Id { get; init; }
  public required string TrackingNumber { get; init; }
  public required string Description { get; init; }
  public required string Recipient { get; init; }
  public bool IsCompleted { get; set; }
  public ShipmentStatus? Status { get; set; }
  public string? Signature { get; set; }
  public DateTime? CompletedAtUtc { get; set; }
}

public sealed class LoadCarrierBooking
{
  public required string Type { get; init; }
  public int Zugang { get; init; }
  public int Abgang { get; init; }
}

public sealed class Stop
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required string Address { get; init; }
  public List<Shipment> Shipments { get; init; } = [];
  public bool IsCompleted { get; set; }
  public string? ProcessingMode { get; set; } // "Bulk" oder "Einzeln"
  public List<LoadCarrierBooking> LoadCarrierBookings { get; init; } = [];

  public IEnumerable<Shipment> OpenShipments => Shipments.Where(s => !s.IsCompleted);
  public bool AllShipmentsCompleted => Shipments.All(s => s.IsCompleted);
}

public sealed class Tour
{
  public required string Id { get; init; }
  public required string Name { get; init; }
  public List<Stop> Stops { get; init; } = [];
}

public static class DriverTourSeedData
{
  public static readonly IReadOnlyList<string> LoadCarrierTypes =
  [
    "Europalette",
    "Gitterbox",
    "Rollcontainer",
    "Sonstiges Lademittel"
  ];

  public static Tour CreateDummyTour()
  {
    return new Tour
    {
      Id = "TOUR-4711",
      Name = "Tour 4711 – Berlin Mitte / Nord",
      Stops =
      [
        new Stop
        {
          Id = "STOP-1",
          Name = "Musterfirma GmbH",
          Address = "Alexanderplatz 1, 10178 Berlin",
          Shipments =
          [
            new Shipment { Id = "SDG-1001", TrackingNumber = "DE1001", Description = "Bürobedarf, 2 Kartons", Recipient = "Musterfirma GmbH" },
            new Shipment { Id = "SDG-1002", TrackingNumber = "DE1002", Description = "Elektronik, 1 Palette", Recipient = "Musterfirma GmbH" }
          ]
        },
        new Stop
        {
          Id = "STOP-2",
          Name = "Beispiel AG",
          Address = "Hauptstraße 22, 10827 Berlin",
          Shipments =
          [
            new Shipment { Id = "SDG-1003", TrackingNumber = "DE1003", Description = "Ersatzteile, 1 Karton", Recipient = "Beispiel AG" }
          ]
        },
        new Stop
        {
          Id = "STOP-3",
          Name = "Test KG",
          Address = "Industriering 5, 12043 Berlin",
          Shipments =
          [
            new Shipment { Id = "SDG-1004", TrackingNumber = "DE1004", Description = "Lebensmittel, gekühlt", Recipient = "Test KG" },
            new Shipment { Id = "SDG-1005", TrackingNumber = "DE1005", Description = "Möbel, 3 Pakete", Recipient = "Test KG" }
          ]
        }
      ]
    };
  }
}
