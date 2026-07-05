using System;
using System.Collections.Generic;
using System.Text;

namespace TicketPlanner.Helper;

public static class TicketPlannerHelper
{
  /// <summary>
  /// Generates initials from the provided name. If the name consists of multiple parts, it takes the first letter of the first two parts; if it consists of a single part, it takes the first two letters. If the name is empty or whitespace, it returns "??".
  /// </summary>
  /// <param name="name">Name to generate initals for.</param>
  /// <returns>Returns a string containing the initals.</returns>
  public static string GetInitials(string name)
  {
    if (string.IsNullOrWhiteSpace(name)) return "";
    var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
    return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpperInvariant();
  }
}
