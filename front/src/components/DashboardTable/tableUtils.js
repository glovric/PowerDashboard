import { powerApi } from '@/api';

export const exportTableData = async (tableData, exportFormat) => {
  const payload = {
      exportFormat,
      rows: tableData.rows,
      headers: tableData.headers,
      columnKeys: tableData.columnKeys
  };

  const result = await powerApi.tableExport(payload);

  return result;
}

export const downloadData = (data, format, fileName = "export") => {

  const formatTypes = {
    csv: "text/csv",
    xlsx: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
  }

  const blob = new Blob([data], {
      type: formatTypes[format]
  });

  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `${fileName}.${format}`;

  document.body.appendChild(link);
  link.click();

  setTimeout(() => {
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }, 100);
}