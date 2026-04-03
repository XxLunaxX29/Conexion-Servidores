using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace ConexionServidores
{
    /// <summary>
    /// Clase que carga archivos XML y los estructura en una tabla evitando duplicados y columnas vacías.
    /// </summary>
    public class XmlDatasetLoader
    {
        private DataTable _dataTable;

        public XmlDatasetLoader()
        {
            _dataTable = new DataTable();
        }

        /// <summary>
        /// Carga un archivo XML y lo convierte en una tabla de datos limpia.
        /// </summary>
        /// <param name="xmlFilePath">Ruta del archivo XML</param>
        /// <param name="elementName">Nombre del elemento XML a procesar (ej: "row", "record")</param>
        /// <returns>DataTable con los datos cargados</returns>
        public DataTable LoadXmlFile(string xmlFilePath, string elementName = "row")
        {
            try
            {
                if (!File.Exists(xmlFilePath))
                    throw new FileNotFoundException($"El archivo XML no existe: {xmlFilePath}");

                Console.WriteLine("\n? Leyendo archivo XML...");
                var xmlDoc = XDocument.Load(xmlFilePath);
                _dataTable = new DataTable();

                var elements = xmlDoc.Descendants(elementName).ToList();

                if (elements.Count == 0)
                    throw new InvalidOperationException($"No se encontraron elementos con el nombre: {elementName}");

                Console.WriteLine($"? Se encontraron {elements.Count} elementos XML");

                // Obtener todas las columnas posibles
                Console.WriteLine("? Extrayendo columnas...");
                var allColumns = ExtractAllColumns(elements);

                // Crear columnas en la tabla (solo las que tienen datos)
                foreach (var column in allColumns)
                {
                    _dataTable.Columns.Add(column, typeof(string));
                }

                Console.WriteLine($"? Se crearon {allColumns.Count} columnas");

                // Agregar filas evitando duplicados
                var addedRows = new HashSet<string>();
                int filasAgregadas = 0;
                int filasEnProceso = 0;

                Console.WriteLine("? Procesando filas...\n");

                foreach (var element in elements)
                {
                    filasEnProceso++;
                    var rowData = new List<string>();
                    var rowHash = "";

                    foreach (var column in allColumns)
                    {
                        var value = GetElementValue(element, column);
                        rowData.Add(value);
                        rowHash += value + "|";
                    }

                    // Verificar si la fila ya existe (evitar duplicados)
                    if (!addedRows.Contains(rowHash))
                    {
                        _dataTable.Rows.Add(rowData.ToArray());
                        addedRows.Add(rowHash);
                        filasAgregadas++;
                    }

                    // Mostrar progreso cada 10 filas
                    if (filasEnProceso % 10 == 0)
                    {
                        Console.Write($"\r? Procesadas {filasEnProceso} filas | Agregadas {filasAgregadas} filas");
                    }
                }

                Console.WriteLine($"\r? Carga completada: {filasEnProceso} leídas | {filasAgregadas} agregadas (sin duplicados)");

                return _dataTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el archivo XML: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Extrae todas las columnas únicas del conjunto de elementos XML.
        /// </summary>
        private List<string> ExtractAllColumns(List<XElement> elements)
        {
            var columns = new HashSet<string>();

            foreach (var element in elements)
            {
                // Obtener atributos
                foreach (var attr in element.Attributes())
                {
                    columns.Add(attr.Name.LocalName);
                }

                // Obtener elementos hijo
                foreach (var child in element.Elements())
                {
                    columns.Add(child.Name.LocalName);
                }
            }

            return columns.OrderBy(c => c).ToList();
        }

        /// <summary>
        /// Obtiene el valor de un elemento específico considerando atributos y elementos hijo.
        /// </summary>
        private string GetElementValue(XElement element, string columnName)
        {
            // Buscar en atributos
            var attr = element.Attribute(columnName);
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Value))
                return attr.Value.Trim();

            // Buscar en elementos hijo
            var child = element.Element(columnName);
            if (child != null && !string.IsNullOrWhiteSpace(child.Value))
                return child.Value.Trim();

            return string.Empty;
        }

        /// <summary>
        /// Obtiene la tabla de datos cargada.
        /// </summary>
        public DataTable GetDataTable()
        {
            return _dataTable;
        }

        /// <summary>
        /// Exporta la tabla a un archivo CSV.
        /// </summary>
        /// <param name="csvFilePath">Ruta del archivo CSV a guardar</param>
        public void ExportToCsv(string csvFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(csvFilePath, false, System.Text.Encoding.UTF8))
                {
                    // Escribir encabezados
                    var headers = _dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                    writer.WriteLine(string.Join(",", headers));

                    // Escribir datos
                    foreach (DataRow row in _dataTable.Rows)
                    {
                        var values = row.ItemArray.Select(v => $"\"{v}\"");
                        writer.WriteLine(string.Join(",", values));
                    }
                }

                Console.WriteLine($"Tabla exportada correctamente a: {csvFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al exportar a CSV: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Muestra la tabla en la consola de forma formateada.
        /// </summary>
        public void DisplayTable()
        {
            if (_dataTable.Rows.Count == 0)
            {
                Console.WriteLine("La tabla está vacía.");
                return;
            }

            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine($"Total de filas: {_dataTable.Rows.Count}");
            Console.WriteLine(new string('=', 80));

            // Mostrar encabezados
            foreach (DataColumn column in _dataTable.Columns)
            {
                Console.Write(column.ColumnName.PadRight(20) + "| ");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 80));

            // Mostrar datos
            foreach (DataRow row in _dataTable.Rows)
            {
                foreach (var cell in row.ItemArray)
                {
                    Console.Write(cell.ToString().PadRight(20).Substring(0, 20) + "| ");
                }
                Console.WriteLine();
            }

            Console.WriteLine(new string('=', 80));
        }
    }
}