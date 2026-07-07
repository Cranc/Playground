// Dünnes JS-Interop-Modul fürs Node-Dragging: Positionsänderungen laufen rein clientseitig
// (SVG-Transform), damit Blazor Server nicht bei jedem Mousemove eine Server-Roundtrip braucht.
// Erst bei PointerUp wird die finale Position per Callback ins .NET-Modell übernommen.
export function makeDraggable(element, startX, startY, dotNetRef) {
  let dragging = false;
  let pointerId = null;
  let originClientX = 0;
  let originClientY = 0;
  let currentX = startX;
  let currentY = startY;

  function isPortOrButton(target) {
    return !!(target.closest('.port') || target.closest('.delete-btn'));
  }

  function onPointerDown(e) {
    if (isPortOrButton(e.target)) {
      return;
    }

    dragging = true;
    pointerId = e.pointerId;
    originClientX = e.clientX;
    originClientY = e.clientY;
    element.setPointerCapture(pointerId);
  }

  function onPointerMove(e) {
    if (!dragging || e.pointerId !== pointerId) {
      return;
    }

    const dx = e.clientX - originClientX;
    const dy = e.clientY - originClientY;
    currentX = startX + dx;
    currentY = startY + dy;
    element.setAttribute('transform', `translate(${currentX}, ${currentY})`);
  }

  function onPointerUp(e) {
    if (!dragging || e.pointerId !== pointerId) {
      return;
    }

    dragging = false;
    element.releasePointerCapture(pointerId);
    dotNetRef.invokeMethodAsync('OnDragEnd', currentX, currentY);
  }

  element.addEventListener('pointerdown', onPointerDown);
  element.addEventListener('pointermove', onPointerMove);
  element.addEventListener('pointerup', onPointerUp);
  element.addEventListener('pointercancel', onPointerUp);

  return {
    updateOrigin(x, y) {
      startX = x;
      startY = y;
      currentX = x;
      currentY = y;
    },
    dispose() {
      element.removeEventListener('pointerdown', onPointerDown);
      element.removeEventListener('pointermove', onPointerMove);
      element.removeEventListener('pointerup', onPointerUp);
      element.removeEventListener('pointercancel', onPointerUp);
    }
  };
}

export function downloadFile(fileName, content) {
  const blob = new Blob([content], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
