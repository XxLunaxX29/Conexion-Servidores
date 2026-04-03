using ConexionServidores;
using System.Data;

var xmlLoader = new XmlDatasetLoader();
var csvLoader = new CsvDatasetLoader();
var jsonLoader = new JsonDatasetLoader();
DataTable dataTable = null;
bool archivosCargados = false;
string tipoArchivoActual = "";
object loaderActual = null;

while (true)
{
    Console.Clear();
    Console.WriteLine("╔══════════════════════════╗");
    Console.WriteLine("║    GESTOR DE DATASETS    ║");
    Console.WriteLine("╚══════════════════════════╝\n");

    if (archivosCargados)
    {
        Console.WriteLine($"✓ Archivo cargado ({tipoArchivoActual}): {dataTable.Rows.Count} filas | {dataTable.Columns.Count} columnas");
    }
    else
    {
        Console.WriteLine("✗ Sin archivo cargado\n");
    }

    Console.WriteLine("Seleccione una opción:");
    Console.WriteLine("1. Cargar archivo XML");
    Console.WriteLine("2. Cargar archivo CSV");
    Console.WriteLine("3. Cargar archivo JSON");
    Console.WriteLine("4. Mostrar datos");
    Console.WriteLine("5. Filtrar datos");
    Console.WriteLine("6. Generar gráfico");
    Console.WriteLine("7. Salir");
    Console.Write("\nOpción: ");

    string opcion = Console.ReadLine();

    switch (opcion)
    {
        case "1":
            CargarArchivoXml(ref xmlLoader, ref dataTable, ref archivosCargados, ref tipoArchivoActual, ref loaderActual);
            break;

        case "2":
            CargarArchivoCsv(ref csvLoader, ref dataTable, ref archivosCargados, ref tipoArchivoActual, ref loaderActual);
            break;

        case "3":
            CargarArchivoJson(ref jsonLoader, ref dataTable, ref archivosCargados, ref tipoArchivoActual, ref loaderActual);
            break;

        case "4":
            if (ValidarDatos(archivosCargados))
                MostrarDatos(dataTable);
            break;

        case "5":
            if (ValidarDatos(archivosCargados))
                FiltrarDatos(dataTable);
            break;

        case "6":
            if (ValidarDatos(archivosCargados))
                GenerarGrafico(dataTable);
            break;

        case "7":
            Console.WriteLine("\n✓ Saliendo del programa...");
            return;

        default:
            Console.WriteLine("\n✗ Opción no válida. Presione cualquier tecla...");
            Console.ReadKey();
            break;
    }
}

// ==================== MÉTODOS ====================

static bool ValidarDatos(bool archivosCargados)
{
    if (!archivosCargados)
    {
        Console.WriteLine("\n✗ Debe cargar un archivo primero. Presione cualquier tecla...");
        Console.ReadKey();
        return false;
    }
    return true;
}

static int CalcularAnchoColumna(DataTable dataTable)
{
    int ventanaAncho = Console.WindowWidth - 4;
    int numColumnas = dataTable.Columns.Count;
    int anchoDisponible = ventanaAncho / numColumnas;
    return Math.Min(anchoDisponible, 20);
}

static void MostrarTabla(DataTable dataTable, int filaInicio, int filaFin, int anchoColumna)
{
    // Mostrar encabezados
    foreach (DataColumn column in dataTable.Columns)
    {
        string nombre = column.ColumnName.Length > anchoColumna 
            ? column.ColumnName.Substring(0, anchoColumna - 2) + ".." 
            : column.ColumnName;
        Console.Write(nombre.PadRight(anchoColumna) + "| ");
    }
    Console.WriteLine();
    Console.WriteLine(new string('-', Console.WindowWidth - 1));

    // Mostrar datos
    for (int i = filaInicio; i < filaFin; i++)
    {
        foreach (var cell in dataTable.Rows[i].ItemArray)
        {
            string valor = cell.ToString();
            if (valor.Length > anchoColumna)
                valor = valor.Substring(0, anchoColumna - 2) + "..";
            Console.Write(valor.PadRight(anchoColumna) + "| ");
        }
        Console.WriteLine();
    }
}

static void CargarArchivoXml(ref XmlDatasetLoader loader, ref DataTable dataTable, ref bool archivosCargados, ref string tipoArchivoActual, ref object loaderActual)
{
    Console.Clear();
    Console.WriteLine("╔═════════════════════════════════════════════╗");
    Console.WriteLine("║       CARGAR ARCHIVO XML                    ║");
    Console.WriteLine("╚═════════════════════════════════════════════╝\n");

    Console.Write("Ingrese la ruta del archivo XML (ej: datos.xml): ");
    string xmlPath = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(xmlPath))
    {
        Console.WriteLine("✗ Ruta no válida. Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    Console.Write("Ingrese el nombre del elemento XML (ej: row): ");
    string elementName = Console.ReadLine() ?? "row";

    try
    {
        dataTable = loader.LoadXmlFile(xmlPath, elementName);
        archivosCargados = true;
        tipoArchivoActual = "XML";
        loaderActual = loader;
        Console.WriteLine($"\n✓ Archivo cargado correctamente. Total de filas: {dataTable.Rows.Count} | Columnas: {dataTable.Columns.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ Error: {ex.Message}");
    }

    Console.WriteLine("Presione cualquier tecla...");
    Console.ReadKey();
}

static void CargarArchivoCsv(ref CsvDatasetLoader loader, ref DataTable dataTable, ref bool archivosCargados, ref string tipoArchivoActual, ref object loaderActual)
{
    Console.Clear();
    Console.WriteLine("╔═════════════════════════════════════════════╗");
    Console.WriteLine("║       CARGAR ARCHIVO CSV                    ║");
    Console.WriteLine("╚═════════════════════════════════════════════╝\n");

    Console.Write("Ingrese la ruta del archivo CSV (ej: datos.csv): ");
    string csvPath = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(csvPath))
    {
        Console.WriteLine("✗ Ruta no válida. Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    Console.Write("¿El archivo tiene encabezados? (s/n): ");
    string tieneEncabezado = Console.ReadLine()?.ToLower() ?? "s";

    try
    {
        dataTable = loader.LoadCsvFile(csvPath, tieneEncabezado == "s");
        archivosCargados = true;
        tipoArchivoActual = "CSV";
        loaderActual = loader;
        Console.WriteLine($"\n✓ Archivo cargado correctamente. Total de filas: {dataTable.Rows.Count} | Columnas: {dataTable.Columns.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ Error: {ex.Message}");
    }

    Console.WriteLine("Presione cualquier tecla...");
    Console.ReadKey();
}

static void CargarArchivoJson(ref JsonDatasetLoader loader, ref DataTable dataTable, ref bool archivosCargados, ref string tipoArchivoActual, ref object loaderActual)
{
    Console.Clear();
    Console.WriteLine("╔═════════════════════════════════════════════╗");
    Console.WriteLine("║       CARGAR ARCHIVO JSON                   ║");
    Console.WriteLine("╚═════════════════════════════════════════════╝\n");

    Console.Write("Ingrese la ruta del archivo JSON (ej: datos.json): ");
    string jsonPath = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(jsonPath))
    {
        Console.WriteLine("✗ Ruta no válida. Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    try
    {
        dataTable = loader.LoadJsonFile(jsonPath);
        archivosCargados = true;
        tipoArchivoActual = "JSON";
        loaderActual = loader;
        Console.WriteLine($"\n✓ Archivo cargado correctamente. Total de filas: {dataTable.Rows.Count} | Columnas: {dataTable.Columns.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ Error: {ex.Message}");
    }

    Console.WriteLine("Presione cualquier tecla...");
    Console.ReadKey();
}

static void MostrarDatos(DataTable dataTable)
{
    Console.Clear();
    Console.WriteLine("╔═════════════════════════════════════════════╗");
    Console.WriteLine("║         MOSTRAR DATOS CARGADOS              ║");
    Console.WriteLine("╚═════════════════════════════════════════════╝\n");

    if (dataTable.Rows.Count == 0)
    {
        Console.WriteLine("La tabla está vacía.");
        Console.WriteLine("Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    int anchoColumna = CalcularAnchoColumna(dataTable);
    int filasPorPagina = 10;
    int pagina = 0;
    int totalPaginas = (int)Math.Ceiling((double)dataTable.Rows.Count / filasPorPagina);

    while (true)
    {
        Console.Clear();
        Console.WriteLine("╔═════════════════════════════════════════════╗");
        Console.WriteLine($"║ PÁGINA {pagina + 1} de {totalPaginas} | Filas: {dataTable.Rows.Count} | Columnas: {dataTable.Columns.Count}   ║");
        Console.WriteLine("╚═════════════════════════════════════════════╝\n");

        int inicio = pagina * filasPorPagina;
        int fin = Math.Min(inicio + filasPorPagina, dataTable.Rows.Count);

        MostrarTabla(dataTable, inicio, fin, anchoColumna);

        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║           OPCIONES DE NAVEGACIÓN       ║");
        Console.WriteLine("╠════════════════════════════════════════╣");
        Console.WriteLine("║  [A] - Página anterior                 ║");
        Console.WriteLine("║  [S] - Página siguiente                ║");
        Console.WriteLine("║  [V] - Volver al menú principal        ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        
        if (pagina == 0)
            Console.WriteLine("  (No hay página anterior)");
        
        if (pagina >= totalPaginas - 1)
            Console.WriteLine("  (No hay página siguiente)");
        
        Console.Write("\nIngrese una opción [A/S/V]: ");

        string nav = Console.ReadLine()?.ToUpper() ?? "";

        switch (nav)
        {
            case "A":
                if (pagina > 0)
                {
                    pagina--;
                }
                else
                {
                    Console.WriteLine("\n✗ No hay página anterior. Presione cualquier tecla...");
                    Console.ReadKey();
                }
                break;

            case "S":
                if (pagina < totalPaginas - 1)
                {
                    pagina++;
                }
                else
                {
                    Console.WriteLine("\n✗ No hay página siguiente. Presione cualquier tecla...");
                    Console.ReadKey();
                }
                break;

            case "V":
                return;

            default:
                Console.WriteLine("\n✗ Opción no válida. Ingrese [A], [S] o [V]. Presione cualquier tecla...");
                Console.ReadKey();
                break;
        }
    }
}

static void FiltrarDatos(DataTable dataTable)
{
    Console.Clear();
    Console.WriteLine("╔════════════════════════════════════════╗");
    Console.WriteLine("║           FILTRAR DATOS                ║");
    Console.WriteLine("╚════════════════════════════════════════╝\n");

    var filtros = new Dictionary<string, string>();
    bool agregarMasFiltros = true;

    while (agregarMasFiltros)
    {
        Console.WriteLine("\nColumnas disponibles:");
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {dataTable.Columns[i].ColumnName}");
        }

        Console.Write("\nSeleccione el número de columna (0 para terminar): ");
        if (!int.TryParse(Console.ReadLine(), out int columnaIndex))
        {
            Console.WriteLine("✗ Entrada no válida. Presione cualquier tecla...");
            Console.ReadKey();
            return;
        }

        // Si selecciona 0, termina de agregar filtros
        if (columnaIndex == 0)
        {
            if (filtros.Count == 0)
            {
                Console.WriteLine("✗ Debe agregar al menos un filtro. Presione cualquier tecla...");
                Console.ReadKey();
                continue;
            }
            agregarMasFiltros = false;
            break;
        }

        // Validar que la columna existe
        if (columnaIndex < 1 || columnaIndex > dataTable.Columns.Count)
        {
            Console.WriteLine("✗ Opción no válida. Presione cualquier tecla...");
            Console.ReadKey();
            continue;
        }

        string columna = dataTable.Columns[columnaIndex - 1].ColumnName;

        // Verificar si ya existe un filtro para esta columna
        if (filtros.ContainsKey(columna))
        {
            Console.WriteLine($"\n⚠ Ya hay un filtro para '{columna}'. ¿Desea reemplazarlo? (s/n): ");
            string respuesta = Console.ReadLine()?.ToLower() ?? "n";
            if (respuesta != "s")
                continue;
        }

        Console.Write($"Ingrese el valor a buscar en '{columna}': ");
        string valor = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(valor))
        {
            Console.WriteLine("✗ El valor no puede estar vacío. Presione cualquier tecla...");
            Console.ReadKey();
            continue;
        }

        filtros[columna] = valor;
        Console.WriteLine($"✓ Filtro agregado para '{columna}'");

        if (filtros.Count > 0)
        {
            Console.WriteLine($"\nFiltros actuales ({filtros.Count}):");
            foreach (var filtro in filtros)
            {
                Console.WriteLine($"  • {filtro.Key} = '{filtro.Value}'");
            }

            Console.Write("\n¿Desea agregar otro filtro? (s/n): ");
            string continuar = Console.ReadLine()?.ToLower() ?? "n";
            if (continuar != "s")
                agregarMasFiltros = false;
        }
    }

    // Aplicar todos los filtros
    var filasFiltradas = dataTable.AsEnumerable().ToList();

    foreach (var filtro in filtros)
    {
        filasFiltradas = filasFiltradas
            .Where(row => row[filtro.Key].ToString().Contains(filtro.Value, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    Console.Clear();
    Console.WriteLine($"╔════════════════════════════════════════╗");
    Console.WriteLine($"║  RESULTADOS: {filasFiltradas.Count} registros encontrados  ║");
    Console.WriteLine($"╚════════════════════════════════════════╝\n");

    if (filtros.Count > 0)
    {
        Console.WriteLine("Filtros aplicados:");
        foreach (var filtro in filtros)
        {
            Console.WriteLine($"  • {filtro.Key} contiene '{filtro.Value}'");
        }
        Console.WriteLine();
    }

    if (filasFiltradas.Count == 0)
    {
        Console.WriteLine("No se encontraron registros.");
        Console.WriteLine("Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    int anchoColumna = CalcularAnchoColumna(dataTable);

    // Mostrar encabezados
    foreach (DataColumn column in dataTable.Columns)
    {
        string nombre = column.ColumnName.Length > anchoColumna 
            ? column.ColumnName.Substring(0, anchoColumna - 2) + ".." 
            : column.ColumnName;
        Console.Write(nombre.PadRight(anchoColumna) + "| ");
    }
    Console.WriteLine();
    Console.WriteLine(new string('-', Console.WindowWidth - 1));

    // Mostrar datos filtrados
    foreach (var row in filasFiltradas)
    {
        foreach (var cell in row.ItemArray)
        {
            string valor = cell.ToString();
            if (valor.Length > anchoColumna)
                valor = valor.Substring(0, anchoColumna - 2) + "..";
            Console.Write(valor.PadRight(anchoColumna) + "| ");
        }
        Console.WriteLine();
    }

    Console.WriteLine("\nPresione cualquier tecla...");
    Console.ReadKey();
}

static void GenerarGrafico(DataTable dataTable)
{
    Console.Clear();
    Console.WriteLine("╔════════════════════════════════════════╗");
    Console.WriteLine("║         GENERAR GRÁFICO                ║");
    Console.WriteLine("╚════════════════════════════════════════╝\n");

    // Identificar columnas numéricas y parcialmente numéricas
    var columnasNumericas = new List<string>();
    var columnasParcialesNumericas = new Dictionary<string, double>();

    for (int i = 0; i < dataTable.Columns.Count; i++)
    {
        string nombreColumna = dataTable.Columns[i].ColumnName;
        int valoresNumericos = 0;
        int totalValores = 0;

        // Contar valores numéricos en la columna
        foreach (DataRow row in dataTable.Rows)
        {
            totalValores++;
            if (double.TryParse(row[i].ToString(), out _))
            {
                valoresNumericos++;
            }
        }

        // Si tiene al menos 80% de valores numéricos, se considera válida
        double porcentajeNumerico = (double)valoresNumericos / totalValores * 100;

        if (valoresNumericos == totalValores)
        {
            columnasNumericas.Add(nombreColumna);
        }
        else if (porcentajeNumerico >= 80)
        {
            columnasParcialesNumericas[nombreColumna] = porcentajeNumerico;
        }
    }

    if (columnasNumericas.Count == 0 && columnasParcialesNumericas.Count == 0)
    {
        Console.WriteLine("✗ No hay columnas numéricas para graficar. Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    // Mostrar columnas disponibles
    Console.WriteLine("Columnas numéricas disponibles:\n");
    int indice = 1;

    // Mostrar columnas completamente numéricas
    if (columnasNumericas.Count > 0)
    {
        Console.WriteLine("✓ Completamente numéricas:");
        foreach (var columna in columnasNumericas)
        {
            Console.WriteLine($"{indice}. {columna}");
            indice++;
        }
        Console.WriteLine();
    }

    // Mostrar columnas parcialmente numéricas
    if (columnasParcialesNumericas.Count > 0)
    {
        Console.WriteLine("⚠ Parcialmente numéricas (con algunos valores no numéricos):");
        foreach (var columna in columnasParcialesNumericas)
        {
            Console.WriteLine($"{indice}. {columna.Key} ({columna.Value:F1}% numéricos)");
            indice++;
        }
        Console.WriteLine();
    }

    // Seleccionar columna
    Console.Write("Seleccione la columna a graficar (número): ");
    if (!int.TryParse(Console.ReadLine(), out int columnaIndex))
    {
        Console.WriteLine("✗ Entrada no válida. Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    string columnaSeleccionada = null;
    int contador = 1;

    // Buscar la columna seleccionada
    foreach (var columna in columnasNumericas)
    {
        if (contador == columnaIndex)
        {
            columnaSeleccionada = columna;
            break;
        }
        contador++;
    }

    if (columnaSeleccionada == null)
    {
        foreach (var columna in columnasParcialesNumericas)
        {
            if (contador == columnaIndex)
            {
                columnaSeleccionada = columna.Key;
                break;
            }
            contador++;
        }
    }

    if (columnaSeleccionada == null)
    {
        Console.WriteLine("✗ Opción no válida. Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    // Obtener valores numéricos de la columna seleccionada
    var valores = new List<double>();

    for (int i = 0; i < dataTable.Rows.Count; i++)
    {
        if (double.TryParse(dataTable.Rows[i][columnaSeleccionada].ToString(), out double valor))
        {
            valores.Add(valor);
        }
    }

    if (valores.Count == 0)
    {
        Console.WriteLine("✗ No se encontraron valores numéricos en la columna seleccionada. Presione cualquier tecla...");
        Console.ReadKey();
        return;
    }

    // Seleccionar tipo de gráfico
    Console.WriteLine("\nTipos de gráficos disponibles:");
    Console.WriteLine("1. Valores individuales (serie de datos)");
    Console.WriteLine("2. Histograma de frecuencias (contar repeticiones)");
    Console.Write("\nSeleccione el tipo de gráfico (1 o 2): ");
    
    string tipoGrafico = Console.ReadLine();

    if (tipoGrafico == "1")
    {
        MostrarGraficoValoresIndividuales(columnaSeleccionada, valores, dataTable.Rows.Count);
    }
    else if (tipoGrafico == "2")
    {
        MostrarHistogramaFrecuencias(columnaSeleccionada, valores);
    }
    else
    {
        Console.WriteLine("✗ Opción no válida. Presione cualquier tecla...");
        Console.ReadKey();
    }
}

static void MostrarGraficoValoresIndividuales(string columnaSeleccionada, List<double> valores, int totalFilas)
{
    // Mostrar información sobre saltos de valores
    if (valores.Count < totalFilas)
    {
        Console.WriteLine($"\n⚠ Nota: Se saltaron {totalFilas - valores.Count} filas con valores no numéricos.");
        Console.ReadKey();
    }

    // Generar gráfico
    Console.Clear();
    Console.WriteLine($"╔════════════════════════════════════════╗");
    Console.WriteLine($"║  GRÁFICO: {columnaSeleccionada}");
    Console.WriteLine($"║  Mostrando {Math.Min(15, valores.Count)} de {valores.Count} valores");
    Console.WriteLine($"╚════════════════════════════════════════╝\n");

    // Gráfico de barras
    double maxValor = valores.Max();
    double minValor = valores.Min();

    for (int i = 0; i < Math.Min(15, valores.Count); i++)
    {
        double valor = valores[i];
        double porcentaje = (valor - minValor) / (maxValor - minValor);
        int barraLength = (int)(porcentaje * 50);
        
        // Asegurar que al menos haya una pequeña barra si el valor no es el mínimo
        if (barraLength == 0 && valor > minValor)
            barraLength = 1;

        Console.WriteLine($"{i + 1:D3}. {new string('█', barraLength)} {valor:F2}");
    }

    // Mostrar estadísticas
    Console.WriteLine();
    Console.WriteLine("╔════════════════════════════════════════╗");
    Console.WriteLine($"║  Máximo: {maxValor:F2}");
    Console.WriteLine($"║  Mínimo: {minValor:F2}");
    Console.WriteLine($"║  Promedio: {valores.Average():F2}");
    Console.WriteLine($"║  Total de valores: {valores.Count}");
    Console.WriteLine("╚════════════════════════════════════════╝");

    Console.WriteLine("\nPresione cualquier tecla...");
    Console.ReadKey();
}

static void MostrarHistogramaFrecuencias(string columnaSeleccionada, List<double> valores)
{
    Console.Clear();
    Console.WriteLine($"╔════════════════════════════════════════╗");
    Console.WriteLine($"║  HISTOGRAMA DE FRECUENCIAS: {columnaSeleccionada}");
    Console.WriteLine($"╚════════════════════════════════════════╝\n");

    // Contar frecuencias de cada valor
    var frecuencias = new Dictionary<double, int>();
    
    foreach (var valor in valores)
    {
        if (frecuencias.ContainsKey(valor))
        {
            frecuencias[valor]++;
        }
        else
        {
            frecuencias[valor] = 1;
        }
    }

    // Ordenar por frecuencia descendente
    var frecuenciasOrdenadas = frecuencias.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();

    // Encontrar la máxima frecuencia para escalar el gráfico
    int maxFrecuencia = frecuenciasOrdenadas.Max(x => x.Value);

    // Mostrar histograma (máximo 20 valores únicos)
    int limite = Math.Min(20, frecuenciasOrdenadas.Count);
    
    Console.WriteLine($"Mostrando {limite} de {frecuenciasOrdenadas.Count} valores únicos\n");

    for (int i = 0; i < limite; i++)
    {
        double valor = frecuenciasOrdenadas[i].Key;
        int frecuencia = frecuenciasOrdenadas[i].Value;
        double porcentaje = (double)frecuencia / maxFrecuencia;
        int barraLength = (int)(porcentaje * 50);

        // Asegurar que al menos haya una barra
        if (barraLength == 0)
            barraLength = 1;

        Console.WriteLine($"{valor:F2} │ {new string('█', barraLength)} {frecuencia} veces ({(double)frecuencia / valores.Count * 100:F1}%)");
    }

    // Mostrar estadísticas
    Console.WriteLine();
    Console.WriteLine("╔════════════════════════════════════════╗");
    Console.WriteLine($"║  Total de valores: {valores.Count}");
    Console.WriteLine($"║  Valores únicos: {frecuenciasOrdenadas.Count}");
    Console.WriteLine($"║  Valor más frecuente: {frecuenciasOrdenadas[0].Key:F2} ({frecuenciasOrdenadas[0].Value} veces)");
    Console.WriteLine($"║  Valor menos frecuente: {frecuenciasOrdenadas.Last().Key:F2} ({frecuenciasOrdenadas.Last().Value} veces)");
    Console.WriteLine($"║  Promedio: {valores.Average():F2}");
    Console.WriteLine($"║  Máximo: {valores.Max():F2}");
    Console.WriteLine($"║  Mínimo: {valores.Min():F2}");
    Console.WriteLine("╚════════════════════════════════════════╝");

    Console.WriteLine("\nPresione cualquier tecla...");
    Console.ReadKey();
}