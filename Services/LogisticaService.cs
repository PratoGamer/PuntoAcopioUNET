using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoAcopioUNET.Services
{
    public class LogisticaService
    {
        // Entrada de la Secuencia Original
        public List<int> secuenciaOriginal { get; set; } = new List<int>();

        // Diccionario de Costos de Operaciones
        public Dictionary<string, int> operacion { get; set; } = new Dictionary<string, int>()
        {
            { "+1", 1 },
            { "-1", 1 },
            { "*2", 3 },
            { "/2", 2 }
        };

        // Propiedades para Resultados de cada Estrategia
        public Datos ResultadoAscendente { get; private set; }
        public Datos ResultadoDescendente { get; private set; }
        public Datos ResultadoConstante { get; private set; }

        // Limite de Poda 
        public int poda = int.MaxValue;

        // Variables Globales Auxiliares
        private int mejorCostoGlobal;
        private List<string> operacionesActuales = new List<string>();
        private List<int> valoresTransformadosActuales = new List<int>();
        private List<int> costosActuales = new List<int>();

        // Variables para Guardar la Mejor Solución Encontrada
        private List<string> mejoresOperaciones = new List<string>();
        private List<int> mejoresValoresTransformados = new List<int>();
        private List<int> mejoresCostos = new List<int>();

        // Variables de Costos
        private int costoMas1, costoMenos1, costoPor2, costoDiv2;

        // Constructor de la Clase LogisticaService
        public LogisticaService()
        {

            // Almacenar cada uno de los Valores
            costoMas1 = operacion["+1"];
            costoMenos1 = operacion["-1"];
            costoPor2 = operacion["*2"];
            costoDiv2 = operacion["/2"];

        }

        // Metodo Principal para Calcular el Orden de la Secuencia
        public async Task CalcularOrden()
        {

            // Verificar que la Secuencia no este Vacia o Nula
            if (secuenciaOriginal == null || secuenciaOriginal.Count == 0) return;

            // Convertir a un Array Estatico
            int[] seq = secuenciaOriginal.ToArray();

            // Ejecutar Cada una de las Estrategias de Ordenamiento
            EjecutarEstrategiaAscendente(seq);
            EjecutarEstrategiaDescendente(seq);
            EjecutarEstrategiaConstante(seq);

            // Esperar a que Todas las Tareas se Terminen de forma Asincrona
            await Task.CompletedTask;

        }

        // Metodo de Estrategia Ascendente
        private void EjecutarEstrategiaAscendente(int[] seq)
        {

            // Iniciar las Listas
            InicializarListasDeControl();

            // Inicio de la Recursion para Ascendente
            ResolverAscendenteDescendente(0, 0, -1, true, seq);

            // Si se Encuentra la Mejor Solucion, Construir el Objeto de Salida
            ResultadoAscendente = mejorCostoGlobal < poda ? ConstruirDatosDeSalida(seq) : null;

        }

        // Metodo de Estrategia Descendente
        private void EjecutarEstrategiaDescendente(int[] seq)
        {

            // Iniciar las Listas
            InicializarListasDeControl();

            // Inicio de la Recursion para Decendente
            ResolverAscendenteDescendente(0, 0, int.MaxValue, false, seq);

            // Si se Encuentra la Mejor Solucion, Construir el Objeto de Salida
            ResultadoDescendente = mejorCostoGlobal < poda ? ConstruirDatosDeSalida(seq) : null;

        }

        // Metodo de Estrategia Constante
        private void EjecutarEstrategiaConstante(int[] seq)
        {

            // Iniciar las Listas
            InicializarListasDeControl();

            // Inicio de la Recursion para Constante
            ResolverConstante(0, 0, -2, seq);

            // Si se Encuentra la Mejor Solucion, Construir el Objeto de Salida
            ResultadoConstante = mejorCostoGlobal < poda ? ConstruirDatosDeSalida(seq) : null;

        }

        // Metodo para Inicializar las Listas de Control
        private void InicializarListasDeControl()
        {
            mejorCostoGlobal = poda;

            operacionesActuales.Clear();
            valoresTransformadosActuales.Clear();
            costosActuales.Clear();

            mejoresOperaciones.Clear();
            mejoresValoresTransformados.Clear();
            mejoresCostos.Clear();
        }

        // Metodo Recursivo para Resolver Ascendente o Descendente
        private void ResolverAscendenteDescendente(int idx, int costoAcumulado, int ultimoValor, bool esAscendente, int[] seq)
        {

            // Poda del Costo Acumulado
            if (costoAcumulado >= mejorCostoGlobal) return;

            // Mejor Solucion Encontrada al Finalizar la Secuencia
            if (idx == seq.Length)
            {
                mejorCostoGlobal = costoAcumulado;
                mejoresOperaciones = new List<string>(operacionesActuales);
                mejoresValoresTransformados = new List<int>(valoresTransformadosActuales);
                mejoresCostos = new List<int>(costosActuales);
                return;
            }

            // Obtener el Valor Original del Elemento Actual
            int valOriginal = seq[idx];

            // 1. Opción "Ninguna" (Costo 0)
            if (idx == 0 || (esAscendente ? valOriginal > ultimoValor : valOriginal < ultimoValor))
            {
                ProbarPaso(idx, costoAcumulado, valOriginal, "Ninguna", 0, esAscendente, seq, false);
            }

            // 2. Opción "+1" (Costo 1)
            int valMas1 = valOriginal + 1;
            if (idx == 0 || (esAscendente ? valMas1 > ultimoValor : valMas1 < ultimoValor))
            {
                ProbarPaso(idx, costoAcumulado + costoMas1, valMas1, "+1", costoMas1, esAscendente, seq, false);
            }

            // 3. Opción "-1" (Costo 1)
            int valMenos1 = valOriginal - 1;
            if (valMenos1 >= 0 && (idx == 0 || (esAscendente ? valMenos1 > ultimoValor : valMenos1 < ultimoValor)))
            {
                ProbarPaso(idx, costoAcumulado + costoMenos1, valMenos1, "-1", costoMenos1, esAscendente, seq, false);
            }

            // 4. Opción "/2" (Costo 2, si es par)
            if (valOriginal % 2 == 0)
            {
                int valDiv2 = valOriginal / 2;
                if (idx == 0 || (esAscendente ? valDiv2 > ultimoValor : valDiv2 < ultimoValor))
                {
                    ProbarPaso(idx, costoAcumulado + costoDiv2, valDiv2, "/2", costoDiv2, esAscendente, seq, false);
                }
            }

            // 5. Opción "*2" (Costo 3)
            int valPor2 = valOriginal * 2;
            if (idx == 0 || (esAscendente ? valPor2 > ultimoValor : valPor2 < ultimoValor))
            {
                ProbarPaso(idx, costoAcumulado + costoPor2, valPor2, "*2", costoPor2, esAscendente, seq, false);
            }
        }

        // Metodo Recursivo para Resolver la Estrategia Constante
        private void ResolverConstante(int idx, int costoAcumulado, int valorPivote, int[] seq)
        {

            // Poda del Costo Acumulado
            if (costoAcumulado >= mejorCostoGlobal) return;

            // Mejor Solucion Encontrada al Finalizar la Secuencia
            if (idx == seq.Length)
            {
                mejorCostoGlobal = costoAcumulado;
                mejoresOperaciones = new List<string>(operacionesActuales);
                mejoresValoresTransformados = new List<int>(valoresTransformadosActuales);
                mejoresCostos = new List<int>(costosActuales);
                return;
            }

            // Obtener el Valor Original del Elemento Actual
            int valOriginal = seq[idx];

            // Lista de Valores Posibles y sus Costos en Local
            int valDiv2 = (valOriginal % 2 == 0) ? (valOriginal / 2) : -1;
            int[] valoresPosibles = { valOriginal, valOriginal + 1, valOriginal - 1, valDiv2, valOriginal * 2 };
            string[] opsPosibles = { "Ninguna", "+1", "-1", "/2", "*2" };
            int[] costosPosibles = { 0, costoMas1, costoMenos1, costoDiv2, costoPor2 };

            for (int i = 0; i < 5; i++)
            {
                int nuevoValor = valoresPosibles[i];

                // Opciones Invalidas
                if (nuevoValor < 0) continue;

                // Buscar el Valor del Pivote para el Paso Actual
                if (idx > 0 && nuevoValor != valorPivote) continue;

                // Pivote del Arbol
                int pivoteParaSiguientePaso = (idx == 0) ? nuevoValor : valorPivote;

                ProbarPaso(idx, costoAcumulado + costosPosibles[i], nuevoValor, opsPosibles[i], costosPosibles[i], false, seq, true, pivoteParaSiguientePaso);

            }
        }

        // Metodo para Probar un Paso en la Recursion
        private void ProbarPaso(int idx, int nuevoCostoAcumulado, int nuevoValor, string opStr, int costoOp,
            bool esAscendente, int[] seq, bool esEstrategiaConstante, int pivoteConstante = 0)
        {
            // Registrar la Operación Actual y el Valor Transformado
            operacionesActuales.Add(opStr);
            valoresTransformadosActuales.Add(nuevoValor);
            costosActuales.Add(costoOp);

            // Llama Recurvia para el Siguiente Paso
            if (esEstrategiaConstante)
            {
                ResolverConstante(idx + 1, nuevoCostoAcumulado, pivoteConstante, seq);
            }
            else
            {
                ResolverAscendenteDescendente(idx + 1, nuevoCostoAcumulado, nuevoValor, esAscendente, seq);
            }

            // Devolver al Estado Anterior o Limpiar Datos
            operacionesActuales.RemoveAt(operacionesActuales.Count - 1);
            valoresTransformadosActuales.RemoveAt(valoresTransformadosActuales.Count - 1);
            costosActuales.RemoveAt(costosActuales.Count - 1);
        }

        // Metodo para Construir el Objeto de Datos de Salida
        private Datos ConstruirDatosDeSalida(int[] originales)
        {

            var datos = new Datos();

            for (int i = 0; i < originales.Length; i++)
            {
                // Guardar los Mejores Resultados en el Objeto de Salida
                datos.Operaciones.Add((i, mejoresOperaciones[i], originales[i], mejoresValoresTransformados[i]));
                datos.Costos.Add(mejoresCostos[i]);
                datos.Secuencia.Add(mejoresValoresTransformados[i]);
            }

            return datos;

        }
    }

    public class Datos
    {
        public List<(int indice, string operacion, int valorAntes, int valorDespues)> Operaciones { get; set; } = new List<(int, string, int, int)>();
        public List<int> Costos { get; set; } = new List<int>();
        public List<int> Secuencia { get; set; } = new List<int>();
    }
}