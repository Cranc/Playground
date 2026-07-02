window.ticketPlanner = {
  registerDropZone: function (elementId, dotNetRef, memberId) {
    const el = document.getElementById(elementId);
    if (!el) return;
    el.addEventListener('dragover', function (e) { e.preventDefault(); });
    el.addEventListener('drop', function (e) {
      e.preventDefault();
      var ticketId = e.dataTransfer.getData('text/plain');
      if (!ticketId) return;
      try {
        dotNetRef.invokeMethodAsync('OnDropFromJs', memberId, ticketId);
      } catch (err) {
        console.error(err);
      }
    });
  }
};
