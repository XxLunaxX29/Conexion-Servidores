using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace ConexionServidores
{
    /// <summary>
    /// Clase que extrae información primordial de los datos cargados (id, nombre, categoría, valor, cantidad, precioUnitario).
    /// Normaliza campos independientemente de la fuente de datos con sinónimos adaptados.
    /// </summary>
    public class DataExtractor
    {
        private DataTable _dataTable;
        private Dictionary<string, string> _mapeoColumnas; // Mapea nombres "estándar" a nombres reales de columnas

        public DataExtractor()
        {
            _mapeoColumnas = new Dictionary<string, string>();
        }

        /// <summary>
        /// Configura el extractor con un DataTable y detecta automáticamente los campos primordiales.
        /// </summary>
        public void ConfigurarConDataTable(DataTable dataTable)
        {
            _dataTable = dataTable ?? throw new ArgumentNullException(nameof(dataTable));
            DetectarColumnasAutomaticamente();
        }

        /// <summary>
        /// Detecta automáticamente las columnas que corresponden a id, nombre, categoría, valor, cantidad y precioUnitario.
        /// </summary>
        private void DetectarColumnasAutomaticamente()
        {
            _mapeoColumnas.Clear();

            var nombresColumnas = _dataTable.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName.ToLower())
                .ToList();

            // Detectar ID
            var columnasId = EncontrarColumnasPorSinonimos(nombresColumnas, new[] 
            { 
                "id", "id_venta", "venta", "identificador", "codigo", "code" 
            });
            if (columnasId.Count > 0)
            {
                _mapeoColumnas["id"] = ObtenerNombreOriginal(columnasId[0]);
            }

            // Detectar Nombre
            var columnasNombre = EncontrarColumnasPorSinonimos(nombresColumnas, new[] 
            { 
                "nombre", "nombre_producto", "producto", "titulo", "title", "name", "descripcion", "description" 
            });
            if (columnasNombre.Count > 0)
            {
                _mapeoColumnas["nombre"] = ObtenerNombreOriginal(columnasNombre[0]);
            }

            // Detectar Categoría
            var columnasCategoria = EncontrarColumnasPorSinonimos(nombresColumnas, new[] 
            { 
                "categoria", "categoria_producto", "category", "grupo", "tipo", "type", "clase", "class" 
            });
            if (columnasCategoria.Count > 0)
            {
                _mapeoColumnas["categoria"] = ObtenerNombreOriginal(columnasCategoria[0]);
            }

            // Detectar Cantidad
            var columnasCantidad = EncontrarColumnasPorSinonimos(nombresColumnas, new[] 
            { 
                "cantidad", "qty", "quantity", "unidades" 
            });
            if (columnasCantidad.Count > 0)
            {
                _mapeoColumnas["cantidad"] = ObtenerNombreOriginal(columnasCantidad[0]);
            }

            // Detectar Precio Unitario
            var columnasPrecioUnitario = EncontrarColumnasPorSinonimos(nombresColumnas, new[] 
            { 
                "precio_unitario", "preciounitario", "precio_unit", "preciou", "unit_price", "unitprice", "precio" 
            });
            if (columnasPrecioUnitario.Count > 0)
            {
                _mapeoColumnas["preciounitario"] = ObtenerNombreOriginal(columnasPrecioUnitario[0]);
            }

            // Detectar Valor
            var columnasValor = EncontrarColumnasPorSinonimos(nombresColumnas, new[] 
            { 
                "total", "total_venta", "precio_unitario", "precio", "valor", "value", "amount" 
            });
            if (columnasValor.Count > 0)
            {
                _mapeoColumnas["valor"] = ObtenerNombreOriginal(columnasValor[0]);
            }

            MostrarResultadosDeteccion();
        }

        /// <summary>
        /// Encuentra columnas que coincidan con alguno de los sinónimos proporcionados.
        /// </summary>
        private List<string> EncontrarColumnasPorSinonimos(List<string> columnas, string[] sinonimos)
        {
            var resultado = new List<string>();

            foreach (var sinonimo in sinonimos)
            {
                var encontrada = columnas.FirstOrDefault(c => c == sinonimo);
                if (encontrada != null)
                {
                    resultado.Add(encontrada);
                    break; // Tomar la primera coincidencia exacta
                }
            }

            // Si no hay coincidencia exacta, buscar parcial
            if (resultado.Count == 0)
            {
                foreach (var sinonimo in sinonimos)
                {
                    var encontrada = columnas.FirstOrDefault(c => c.Contains(sinonimo));
                    if (encontrada != null)
                    {
                        resultado.Add(encontrada);
                        break; // Tomar la primera coincidencia parcial
                    }
                }
            }

            return resultado;
        }

        /// <summary>
        /// Obtiene el nombre original de la columna en el DataTable.
        /// </summary>
        private string ObtenerNombreOriginal(string nombreMinuscula)
        {
            return _dataTable.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => c.ColumnName.ToLower() == nombreMinuscula)
                ?.ColumnName ?? nombreMinuscula;
        }

        /// <summary>
        /// Muestra los resultados de la detección automática en consola.
        /// </summary>
        private void MostrarResultadosDeteccion()
        {
            Console.WriteLine("\n??????????????????????????????????????????");
            Console.WriteLine("?  DETECCIÓN DE CAMPOS PRIMORDIALES      ?");
            Console.WriteLine("??????????????????????????????????????????\n");

            if (_mapeoColumnas.Count == 0)
            {
                Console.WriteLine("? No se detectaron campos primordiales.");
                Console.WriteLine("Columnas disponibles en el DataTable:");
                for (int i = 0; i < _dataTable.Columns.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {_dataTable.Columns[i].ColumnName}");
                }
            }
            else
            {
                foreach (var mapeo in _mapeoColumnas)
                {
                    Console.WriteLine($"? {mapeo.Key.ToUpper()} ? '{mapeo.Value}'");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Permite mapear manualmente un campo estándar a una columna específica.
        /// </summary>
        public void MapearColumnaPersonalizada(string campoEstandar, string nombreColumnaReal)
        {
            if (!_dataTable.Columns.Contains(nombreColumnaReal))
                throw new ArgumentException($"La columna '{nombreColumnaReal}' no existe en el DataTable.");

            _mapeoColumnas[campoEstandar.ToLower()] = nombreColumnaReal;
            Console.WriteLine($"? Mapeo personalizado: {campoEstandar} ? {nombreColumnaReal}");
        }

        /// <summary>
        /// Extrae los datos primordiales del DataTable.
        /// </summary>
        public List<ProductoPrimordial> ExtraerDatos()
        {
            var productos = new List<ProductoPrimordial>();

            foreach (DataRow row in _dataTable.Rows)
            {
                var producto = new ProductoPrimordial();

                if (_mapeoColumnas.TryGetValue("id", out string columnaId))
                    producto.Id = row[columnaId]?.ToString();

                if (_mapeoColumnas.TryGetValue("nombre", out string columnaNombre))
                    producto.Nombre = row[columnaNombre]?.ToString();

                if (_mapeoColumnas.TryGetValue("categoria", out string columnaCategoria))
                    producto.Categoria = row[columnaCategoria]?.ToString();

                if (_mapeoColumnas.TryGetValue("valor", out string columnaValor))
                {
                    if (decimal.TryParse(row[columnaValor]?.ToString(), out decimal valorNumerico))
                        producto.Valor = valorNumerico;
                    else
                        producto.Valor = 0;
                }

                if (_mapeoColumnas.TryGetValue("cantidad", out string columnaCantidad))
                {
                    if (int.TryParse(row[columnaCantidad]?.ToString(), out int cantidadNumerica))
                        producto.Cantidad = cantidadNumerica;
                    else
                        producto.Cantidad = 0;
                }

                if (_mapeoColumnas.TryGetValue("preciounitario", out string columnaPrecioUnitario))
                {
                    if (decimal.TryParse(row[columnaPrecioUnitario]?.ToString(), out decimal precioUnitarioNumerico))
                        producto.PrecioUnitario = precioUnitarioNumerico;
                    else
                        producto.PrecioUnitario = 0;
                }

                // Solo agregar si tiene al menos un campo completo
                if (!string.IsNullOrEmpty(producto.Id) || !string.IsNullOrEmpty(producto.Nombre))
                {
                    productos.Add(producto);
                }
            }

            return productos;
        }

        /// <summary>
        /// Extrae datos primordiales desde una lista de objetos dinámicos (ExpandoObject).
        /// </summary>
        public List<ProductoPrimordial> ExtraerDatos(List<dynamic> datos)
        {
            var productos = new List<ProductoPrimordial>();

            foreach (var item in datos)
            {
                var producto = new ProductoPrimordial();
                var propiedades = ((ExpandoObject)item).Cast<KeyValuePair<string, object>>().ToDictionary(p => p.Key, p => p.Value);

                // Buscar y asignar ID
                var claveId = EncontrarClaveEnDiccionario(propiedades, new[] { "id", "id_venta", "venta", "identificador", "codigo", "code" });
                if (claveId != null)
                    producto.Id = propiedades[claveId]?.ToString();

                // Buscar y asignar Nombre
                var claveNombre = EncontrarClaveEnDiccionario(propiedades, new[] { "nombre", "nombre_producto", "producto", "titulo", "title", "name", "descripcion", "description" });
                if (claveNombre != null)
                    producto.Nombre = propiedades[claveNombre]?.ToString();

                // Buscar y asignar Categoría
                var claveCategoria = EncontrarClaveEnDiccionario(propiedades, new[] { "categoria", "categoria_producto", "category", "grupo", "tipo", "type", "clase", "class" });
                if (claveCategoria != null)
                    producto.Categoria = propiedades[claveCategoria]?.ToString();

                // Buscar y asignar Cantidad
                var claveCantidad = EncontrarClaveEnDiccionario(propiedades, new[] { "cantidad", "qty", "quantity", "unidades" });
                if (claveCantidad != null)
                {
                    if (int.TryParse(propiedades[claveCantidad]?.ToString(), out int cantidadNumerica))
                        producto.Cantidad = cantidadNumerica;
                }

                // Buscar y asignar Precio Unitario
                var clavePrecioUnitario = EncontrarClaveEnDiccionario(propiedades, new[] { "precio_unitario", "preciounitario", "precio_unit", "preciou", "unit_price", "unitprice", "precio" });
                if (clavePrecioUnitario != null)
                {
                    if (decimal.TryParse(propiedades[clavePrecioUnitario]?.ToString(), out decimal precioUnitarioNumerico))
                        producto.PrecioUnitario = precioUnitarioNumerico;
                }

                // Buscar y asignar Valor
                var claveValor = EncontrarClaveEnDiccionario(propiedades, new[] { "total", "total_venta", "precio_unitario", "precio", "valor", "value", "amount" });
                if (claveValor != null)
                {
                    if (decimal.TryParse(propiedades[claveValor]?.ToString(), out decimal valorNumerico))
                        producto.Valor = valorNumerico;
                }

                // Solo agregar si tiene al menos un campo completo
                if (!string.IsNullOrEmpty(producto.Id) || !string.IsNullOrEmpty(producto.Nombre))
                {
                    productos.Add(producto);
                }
            }

            return productos;
        }

        /// <summary>
        /// Busca una clave en el diccionario que coincida con alguno de los términos proporcionados.
        /// Prioriza coincidencias exactas sobre coincidencias parciales.
        /// </summary>
        private string EncontrarClaveEnDiccionario(Dictionary<string, object> diccionario, string[] terminos)
        {
            // Primero buscar coincidencia exacta
            foreach (var termino in terminos)
            {
                var clave = diccionario.Keys.FirstOrDefault(k => k.ToLower() == termino);
                if (clave != null)
                    return clave;
            }

            // Luego buscar coincidencia parcial
            foreach (var termino in terminos)
            {
                var clave = diccionario.Keys.FirstOrDefault(k => k.ToLower().Contains(termino));
                if (clave != null)
                    return clave;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el mapeo de columnas actual.
        /// </summary>
        public Dictionary<string, string> ObtenerMapeo()
        {
            return new Dictionary<string, string>(_mapeoColumnas);
        }

        /// <summary>
        /// Limpia el mapeo de columnas.
        /// </summary>
        public void LimpiarMapeo()
        {
            _mapeoColumnas.Clear();
        }

        /// <summary>
        /// Muestra los datos primordiales en formato tabular en la consola.
        /// </summary>
        public void MostrarDatosPrimordiales(List<ProductoPrimordial> productos)
        {
            if (productos.Count == 0)
            {
                Console.WriteLine("\n? No hay datos para mostrar.");
                return;
            }

            Console.WriteLine("\n????????????????????????????????????????????????????????????????????????????????????????");
            Console.WriteLine($"?  DATOS PRIMORDIALES: {productos.Count} registros");
            Console.WriteLine("????????????????????????????????????????????????????????????????????????????????????????\n");

            // Encabezados
            Console.WriteLine($"{"ID",-12} | {"NOMBRE",-20} | {"CATEGORÍA",-15} | {"VALOR",-12} | {"CANTIDAD",-10} | {"P.UNITARIO",-12}");
            Console.WriteLine(new string('-', 95));

            // Datos
            foreach (var producto in productos)
            {
                string id = producto.Id ?? "N/A";
                string nombre = producto.Nombre ?? "N/A";
                string categoria = producto.Categoria ?? "N/A";
                string valor = producto.Valor > 0 ? $"${producto.Valor:F2}" : "N/A";
                string cantidad = producto.Cantidad > 0 ? producto.Cantidad.ToString() : "N/A";
                string precioUnitario = producto.PrecioUnitario > 0 ? $"${producto.PrecioUnitario:F2}" : "N/A";

                Console.WriteLine($"{id,-12} | {nombre,-20} | {categoria,-15} | {valor,-12} | {cantidad,-10} | {precioUnitario,-12}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Exporta los datos primordiales a un nuevo DataTable.
        /// </summary>
        public DataTable ExportarADataTable(List<ProductoPrimordial> productos)
        {
            var dataTable = new DataTable("DatosPrimordiales");

            dataTable.Columns.Add("ID", typeof(string));
            dataTable.Columns.Add("Nombre", typeof(string));
            dataTable.Columns.Add("Categoria", typeof(string));
            dataTable.Columns.Add("Valor", typeof(decimal));
            dataTable.Columns.Add("Cantidad", typeof(int));
            dataTable.Columns.Add("PrecioUnitario", typeof(decimal));

            foreach (var producto in productos)
            {
                dataTable.Rows.Add(producto.Id, producto.Nombre, producto.Categoria, producto.Valor, producto.Cantidad, producto.PrecioUnitario);
            }

            return dataTable;
        }
    }

    /// <summary>
    /// Clase que representa un producto con datos primordiales.
    /// </summary>
    public class ProductoPrimordial
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Valor { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public override string ToString()
        {
            return $"ID: {Id}, Nombre: {Nombre}, Categoría: {Categoria}, Valor: ${Valor:F2}, Cantidad: {Cantidad}, P.Unitario: ${PrecioUnitario:F2}";
        }
    }
}