function export_file(fileName, content) {
  var link = document.createElement('a');
  link.download = fileName;
  link.href = 'data:text/plain;charset=utf-8,' + encodeURIComponent(content);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}