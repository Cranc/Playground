window.ticketPlanner = {
  registerDropZone: function (elementId, dotNetRef, memberId) {
    const el = document.getElementById(elementId);
    if (!el) return;

    // Guard: avoid duplicate registrations for the same element
    if (el._ticketPlannerRegistered) return;
    el._ticketPlannerRegistered = true;

    function onDragOver(e) { e.preventDefault(); }

    function onDrop(e) {
      e.preventDefault();
      const data = e.dataTransfer.getData('text/plain');
      if (!data) return;

      try {
        // Try to parse as JSON (lane-to-lane move)
        const parsed = JSON.parse(data);
        if (parsed.ticketId && parsed.sourceMemberId) {
          dotNetRef.invokeMethodAsync('OnDropFromJs', memberId, parsed.ticketId, parsed.sourceMemberId);
          return;
        }
      } catch (err) {
        // Not JSON, treat as plain ticketId (from pool)
      }

      // Plain ticketId from pool
      dotNetRef.invokeMethodAsync('OnDropFromJs', memberId, data, null);
    }

    el.addEventListener('dragover', onDragOver);
    el.addEventListener('drop', onDrop);
  }
};
