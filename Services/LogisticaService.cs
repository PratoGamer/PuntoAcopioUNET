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
        public int poda = int.MaxValue - 100;

        // Cantidad de Operaciones por Elemento
        private int maxOperaciones = 5;

        // Diccionario de Memorizacion
        private Dictionary<string, int> memorizacion = new Dictionary<string, int>();

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
            ResolverAscendenteDescendente(0, 0, -1, true, seq, seq[0], 0, "Ninguna", 0);

            // Si se Encuentra la Mejor Solucion, Construir el Objeto de Salida
            ResultadoAscendente = mejorCostoGlobal < poda ? ConstruirDatosDeSalida(seq) : null;

        }

        // Metodo de Estrategia Descendente
        private void EjecutarEstrategiaDescendente(int[] seq)
        {

            // Iniciar las Listas
            InicializarListasDeControl();

            // Inicio de la Recursion para Decendente
            ResolverAscendenteDescendente(0, 0, poda, false, seq, seq[0], 0, "Ninguna", 0);

            // Si se Encuentra la Mejor Solucion, Construir el Objeto de Salida
            ResultadoDescendente = mejorCostoGlobal < poda ? ConstruirDatosDeSalida(seq) : null;

        }

        // Metodo de Estrategia Constante
        private void EjecutarEstrategiaConstante(int[] seq)
        {

            // Iniciar las Listas
            InicializarListasDeControl();

            // Inicio de la Recursion para Constante
            ResolverConstante(0, 0, -2, seq, seq[0], 0, "Ninguna", 0);

            // Si se Encuentra la Mejor Solucion, Construir el Objeto de Salida
            ResultadoConstante = mejorCostoGlobal < poda ? ConstruirDatosDeSalida(seq) : null;

        }

        // Metodo para Inicializar las Listas de Control
        private void InicializarListasDeControl()
        {

            // Limpiar Todos los Datos
            mejorCostoGlobal = poda;
            memorizacion.Clear();

            operacionesActuales.Clear();
            valoresTransformadosActuales.Clear();
            costosActuales.Clear();

            mejoresOperaciones.Clear();
            mejoresValoresTransformados.Clear();
            mejoresCostos.Clear();

        }

        // Metodo Recursivo para Resolver Ascendente o Descendente
        private void ResolverAscendenteDescendente(int idx, int costoAcumulado, int ultimoValor, bool esAscendente, int[] seq, int valActual, int opsHechas, string opAcumulada, int costoAcumuladoElemento)
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

            // Poda de Memorizacion por Token
            string estado = $"{idx}_{ultimoValor}_{valActual}";
            if (memorizacion.ContainsKey(estado) && costoAcumulado >= memorizacion[estado]) return;
            memorizacion[estado] = costoAcumulado;

            // Verificar que ya Termino de Realizar Cambios en el Elemento Actual
            if (idx == 0 || (esAscendente ? valActual > ultimoValor : valActual < ultimoValor))
            {
                operacionesActuales.Add(opAcumulada);
                valoresTransformadosActuales.Add(valActual);
                costosActuales.Add(costoAcumuladoElemento);

                // Avanzamos al siguiente índice físico de la secuencia
                int siguienteValorOriginal = (idx + 1 < seq.Length) ? seq[idx + 1] : 0;
                ResolverAscendenteDescendente(idx + 1, costoAcumulado, valActual, esAscendente, seq, siguienteValorOriginal, 0, "Ninguna", 0);

                // Volver a los Estados Iniciales
                operacionesActuales.RemoveAt(operacionesActuales.Count - 1);
                valoresTransformadosActuales.RemoveAt(valoresTransformadosActuales.Count - 1);
                costosActuales.RemoveAt(costosActuales.Count - 1);
            }

            // Realizar Operaciones en el Elemento Actual
            if (opsHechas < maxOperaciones)
            {
                string prefijoOp = opAcumulada == "Ninguna" ? "" : opAcumulada + " -> ";

                // 1. Probar -1
                if (valActual > 0)
                {
                    ResolverAscendenteDescendente(idx, costoAcumulado + costoMenos1, ultimoValor, esAscendente, seq, valActual - 1, opsHechas + 1, prefijoOp + "-1", costoAcumuladoElemento + costoMenos1);
                }

                // 2. Probar +1
                if (valActual < int.MaxValue - 1)
                {
                    ResolverAscendenteDescendente(idx, costoAcumulado + costoMas1, ultimoValor, esAscendente, seq, valActual + 1, opsHechas + 1, prefijoOp + "+1", costoAcumuladoElemento + costoMas1);
                }

                // 3. Probar /2
                if (valActual % 2 == 0)
                {
                    ResolverAscendenteDescendente(idx, costoAcumulado + costoDiv2, ultimoValor, esAscendente, seq, valActual / 2, opsHechas + 1, prefijoOp + "/2", costoAcumuladoElemento + costoDiv2);
                }

                // 4. Probar *2
                if (valActual < int.MaxValue / 2)
                {
                    ResolverAscendenteDescendente(idx, costoAcumulado + costoPor2, ultimoValor, esAscendente, seq, valActual * 2, opsHechas + 1, prefijoOp + "*2", costoAcumuladoElemento + costoPor2);
                }
            }
        }

        // Metodo Recursivo para Resolver la Estrategia Constante
        private void ResolverConstante(int idx, int costoAcumulado, int valorPivote, int[] seq, int valActual, int opsHechas, string opAcumulada, int costoAcumuladoElemento)
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

            // Poda de Memorizacion por Token
            string estado = $"{idx}_{valorPivote}_{valActual}";
            if (memorizacion.ContainsKey(estado) && costoAcumulado >= memorizacion[estado]) return;
            memorizacion[estado] = costoAcumulado;

            // Fijar Pivote Buscando el Constante
            if (idx == 0 || valActual == valorPivote)
            {
                int nuevoPivote = (idx == 0) ? valActual : valorPivote;

                operacionesActuales.Add(opAcumulada);
                valoresTransformadosActuales.Add(valActual);
                costosActuales.Add(costoAcumuladoElemento);

                int siguienteValorOriginal = (idx + 1 < seq.Length) ? seq[idx + 1] : 0;
                ResolverConstante(idx + 1, costoAcumulado, nuevoPivote, seq, siguienteValorOriginal, 0, "Ninguna", 0);

                operacionesActuales.RemoveAt(operacionesActuales.Count - 1);
                valoresTransformadosActuales.RemoveAt(valoresTransformadosActuales.Count - 1);
                costosActuales.RemoveAt(costosActuales.Count - 1);
            }

            // Realizar Operaciones en el Elemento Actual
            if (opsHechas < maxOperaciones)
            {
                string prefijoOp = opAcumulada == "Ninguna" ? "" : opAcumulada + " -> ";

                if (valActual > 0)
                    ResolverConstante(idx, costoAcumulado + costoMenos1, valorPivote, seq, valActual - 1, opsHechas + 1, prefijoOp + "-1", costoAcumuladoElemento + costoMenos1);

                if (valActual < int.MaxValue - 1)
                    ResolverConstante(idx, costoAcumulado + costoMas1, valorPivote, seq, valActual + 1, opsHechas + 1, prefijoOp + "+1", costoAcumuladoElemento + costoMas1);

                if (valActual % 2 == 0)
                    ResolverConstante(idx, costoAcumulado + costoDiv2, valorPivote, seq, valActual / 2, opsHechas + 1, prefijoOp + "/2", costoAcumuladoElemento + costoDiv2);

                if (valActual < int.MaxValue / 2)
                    ResolverConstante(idx, costoAcumulado + costoPor2, valorPivote, seq, valActual * 2, opsHechas + 1, prefijoOp + "*2", costoAcumuladoElemento + costoPor2);
            }
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