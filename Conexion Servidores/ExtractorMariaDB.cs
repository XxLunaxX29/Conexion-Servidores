using MySqlConnector;
using System.Data;

namespace ConexionServidores
{
    /// <summary>
    /// Clase para extraer datos de MariaDB y mostrarlos en tabla en consola.
    /// </summary>
    public class ExtractorMariaDB(string connectionString)
    {
        private const string BaseDatos = "ConexionSQL";

        /// <summary>
        /// Obtiene todos los productos desde MariaDB como DataTable.
        /// </summary>
        public async Task<(bool Success, DataTable Data, string Message)> ObtenerProductosAsync()
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString)
                {
                    Database = BaseDatos,
                    SslMode = MySqlSslMode.None
                };

                using var connection = new MySqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                string query = "SELECT Id, Nombre, Categoria, Valor, Cantidad, PrecioUnitario FROM Producto ORDER BY Id";

                using var cmd = new MySqlCommand(query, connection);
                using var adapter = new MySqlDataAdapter(cmd);

                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count == 0)
                {
                    return (false, dataTable, "No hay productos en la base de datos");
                }

                return (true, dataTable, $"Se obtuvieron {dataTable.Rows.Count} productos");
            }
            catch (MySqlException ex)
            {
                return (false, new DataTable(), $"Error de MariaDB: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, new DataTable(), $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene productos filtrados por categoría.
        /// </summary>
        public async Task<(bool Success, DataTable Data, string Message)> ObtenerProductosPorCategoriaAsync(string categoria)
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString)
                {
                    Database = BaseDatos,
                    SslMode = MySqlSslMode.None
                };

                using var connection = new MySqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                string query = @"
                SELECT Id, Nombre, Categoria, Valor, Cantidad, PrecioUnitario 
                FROM Producto 
                WHERE Categoria LIKE @Categoria 
                ORDER BY Id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Categoria", $"%{categoria}%");
                
                using var adapter = new MySqlDataAdapter(cmd);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count == 0)
                {
                    return (false, dataTable, $"No hay productos en la categoría '{categoria}'");
                }

                return (true, dataTable, $"Se obtuvieron {dataTable.Rows.Count} productos de la categoría '{categoria}'");
            }
            catch (Exception ex)
            {
                return (false, new DataTable(), $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene productos dentro de un rango de precios.
        /// </summary>
        public async Task<(bool Success, DataTable Data, string Message)> ObtenerProductosPorPrecioAsync(decimal precioMin, decimal precioMax)
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString)
                {
                    Database = BaseDatos,
                    SslMode = MySqlSslMode.None
                };

                using var connection = new MySqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                string query = @"
                SELECT Id, Nombre, Categoria, Valor, Cantidad, PrecioUnitario 
                FROM Producto 
                WHERE PrecioUnitario BETWEEN @PrecioMin AND @PrecioMax 
                ORDER BY PrecioUnitario";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@PrecioMin", precioMin);
                cmd.Parameters.AddWithValue("@PrecioMax", precioMax);
                
                using var adapter = new MySqlDataAdapter(cmd);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count == 0)
                {
                    return (false, dataTable, $"No hay productos entre ${precioMin:F2} y ${precioMax:F2}");
                }

                return (true, dataTable, $"Se obtuvieron {dataTable.Rows.Count} productos");
            }
            catch (Exception ex)
            {
                return (false, new DataTable(), $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene estadísticas de los productos.
        /// </summary>
        public async Task<(bool Success, Dictionary<string, object> Stats, string Message)> ObtenerEstadisticasAsync()
        {
            var stats = new Dictionary<string, object>();

            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString)
                {
                    Database = BaseDatos,
                    SslMode = MySqlSslMode.None
                };

                using var connection = new MySqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                string query = @"
                SELECT 
                    COUNT(*) as Total,
                    COUNT(DISTINCT Categoria) as CategoriasUnicas,
                    MIN(PrecioUnitario) as PrecioMinimo,
                    MAX(PrecioUnitario) as PrecioMaximo,
                    AVG(PrecioUnitario) as PrecioPromedio,
                    SUM(Cantidad) as CantidadTotal,
                    SUM(Valor) as ValorTotal,
                    AVG(Cantidad) as CantidadPromedio
                FROM Producto";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    stats["Total"] = reader["Total"];
                    stats["CategoriasUnicas"] = reader["CategoriasUnicas"];
                    stats["PrecioMinimo"] = Convert.ToDecimal(reader["PrecioMinimo"]);
                    stats["PrecioMaximo"] = Convert.ToDecimal(reader["PrecioMaximo"]);
                    stats["PrecioPromedio"] = Convert.ToDecimal(reader["PrecioPromedio"]);
                    stats["CantidadTotal"] = reader["CantidadTotal"];
                    stats["ValorTotal"] = Convert.ToDecimal(reader["ValorTotal"]);
                    stats["CantidadPromedio"] = Convert.ToDecimal(reader["CantidadPromedio"]);

                    return (true, stats, "Estadísticas obtenidas correctamente");
                }
            }
            catch (Exception ex)
            {
                return (false, stats, $"Error: {ex.Message}");
            }

            return (false, stats, "No se pudieron obtener las estadísticas");
        }

        /// <summary>
        /// Muestra un DataTable en la consola con paginación.
        /// </summary>
        public void MostrarTablaEnConsola(DataTable dataTable, string titulo = "DATOS DE MARIADB")
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                Console.WriteLine("\n? No hay datos para mostrar.");
                return;
            }

            int anchoColumna = CalcularAnchoColumna(dataTable);
            int filasPorPagina = 10;
            int pagina = 0;
            int totalPaginas = (int)Math.Ceiling((double)dataTable.Rows.Count / filasPorPagina);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("????????????????????????????????????????????????????????????????");
                Console.WriteLine($"?  {titulo}");
                Console.WriteLine($"?  PÁGINA {pagina + 1} de {totalPaginas} | Filas: {dataTable.Rows.Count} | Columnas: {dataTable.Columns.Count}");
                Console.WriteLine("????????????????????????????????????????????????????????????????\n");

                int inicio = pagina * filasPorPagina;
                int fin = Math.Min(inicio + filasPorPagina, dataTable.Rows.Count);

                MostrarTablaPaginada(dataTable, inicio, fin, anchoColumna);

                Console.WriteLine();
                Console.WriteLine("??????????????????????????????????????????");
                Console.WriteLine("?           OPCIONES DE NAVEGACIÓN       ?");
                Console.WriteLine("??????????????????????????????????????????");
                Console.WriteLine("?  [A] - Página anterior                 ?");
                Console.WriteLine("?  [S] - Página siguiente                ?");
                Console.WriteLine("?  [V] - Volver al menú principal        ?");
                Console.WriteLine("??????????????????????????????????????????");

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
                            pagina--;
                        else
                        {
                            Console.WriteLine("\n? No hay página anterior. Presione cualquier tecla...");
                            Console.ReadKey();
                        }
                        break;

                    case "S":
                        if (pagina < totalPaginas - 1)
                            pagina++;
                        else
                        {
                            Console.WriteLine("\n? No hay página siguiente. Presione cualquier tecla...");
                            Console.ReadKey();
                        }
                        break;

                    case "V":
                        return;

                    default:
                        Console.WriteLine("\n? Opción no válida. Presione cualquier tecla...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Calcula el ancho óptimo de columna.
        /// </summary>
        private int CalcularAnchoColumna(DataTable dataTable)
        {
            int ventanaAncho = Console.WindowWidth - 4;
            int numColumnas = dataTable.Columns.Count;
            int anchoDisponible = ventanaAncho / numColumnas;
            return Math.Min(anchoDisponible, 20);
        }

        /// <summary>
        /// Muestra una página de la tabla.
        /// </summary>
        private void MostrarTablaPaginada(DataTable dataTable, int filaInicio, int filaFin, int anchoColumna)
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
    }
}