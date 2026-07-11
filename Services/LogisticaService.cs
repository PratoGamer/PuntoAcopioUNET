using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuntoAcopioUNET.Services
{
    public class LogisticaService
    {
        public List<int> secuenciaOriginal { get; set; } = new List<int>();

        public Dictionary<string, int> operacion { get; set; } = new Dictionary<string, int>()
        {
            { "+1", 1 },
            { "-1", 1 },
            { "*2", 3 },
            { "/2", 2 }
        };

        public Datos ResultadoAscendente { get; private set; }
        public Datos ResultadoDescendente { get; private set; }
        public Datos ResultadoConstante { get; private set; }

        // Poda inicial preventiva alta pero segura para evitar desbordamientos
        private int poda = 500;

        private int costoMas1, costoMenos1, costoPor2, costoDiv2;

        public LogisticaService()
        {
            costoMas1 = operacion["+1"];
            costoMenos1 = operacion["-1"];
            costoPor2 = operacion["*2"];
            costoDiv2 = operacion["/2"];
        }

        public async Task CalcularOrden()
        {
            if (secuenciaOriginal == null || secuenciaOriginal.Count == 0) return;

            int[] secuenciaBase = secuenciaOriginal.ToArray();

            // Ejecución asíncrona multihilo en paralelo
            var tareaAsc = Task.Run(() => CalcularAscendente(secuenciaBase));
            var tareaDesc = Task.Run(() => CalcularDescendente(secuenciaBase));
            var tareaConst = Task.Run(() => CalcularConstante(secuenciaBase));

            await Task.WhenAll(tareaAsc, tareaDesc, tareaConst);

            ResultadoAscendente = tareaAsc.Result;
            ResultadoDescendente = tareaDesc.Result;
            ResultadoConstante = tareaConst.Result;
        }

        private Datos CalcularAscendente(int[] secuenciaBase)
        {
            string[] opsElegidas = new string[secuenciaBase.Length];
            int[] valoresDespues = new int[secuenciaBase.Length];
            int[] costosAplicados = new int[secuenciaBase.Length];

            int mejorCosto = poda;
            string[] mejorOps = new string[secuenciaBase.Length];
            int[] mejorValores = new int[secuenciaBase.Length];
            int[] mejorCostos = new int[secuenciaBase.Length];

            // Para Ascendente, el criterio es: cada número debe ser >= que el anterior transformado
            ResolverEstrategia(0, 0, -1, true, secuenciaBase, opsElegidas, valoresDespues, costosAplicados, ref mejorCosto, mejorOps, mejorValores, mejorCostos);

            return mejorCosto < poda ? ConstruirDatos(secuenciaBase, mejorOps, mejorValores, mejorCostos) : null;
        }

        private Datos CalcularDescendente(int[] secuenciaBase)
        {
            string[] opsElegidas = new string[secuenciaBase.Length];
            int[] valoresDespues = new int[secuenciaBase.Length];
            int[] costosAplicados = new int[secuenciaBase.Length];

            int mejorCosto = poda;
            string[] mejorOps = new string[secuenciaBase.Length];
            int[] mejorValores = new int[secuenciaBase.Length];
            int[] mejorCostos = new int[secuenciaBase.Length];

            // Para Descendente, el criterio es: cada número debe ser <= que el anterior transformado
            ResolverEstrategia(0, 0, int.MaxValue, false, secuenciaBase, opsElegidas, valoresDespues, costosAplicados, ref mejorCosto, mejorOps, mejorValores, mejorCostos);

            return mejorCosto < poda ? ConstruirDatos(secuenciaBase, mejorOps, mejorValores, mejorCostos) : null;
        }

        private Datos CalcularConstante(int[] secuenciaBase)
        {
            string[] opsElegidas = new string[secuenciaBase.Length];
            int[] valoresDespues = new int[secuenciaBase.Length];
            int[] costosAplicados = new int[secuenciaBase.Length];

            int mejorCosto = poda;
            string[] mejorOps = new string[secuenciaBase.Length];
            int[] mejorValores = new int[secuenciaBase.Length];
            int[] mejorCostos = new int[secuenciaBase.Length];

            // Enviamos -2 como bandera de que el pivote constante aún no se ha definido (se define en el índice 0)
            ResolverConstanteRecursivo(0, 0, -2, secuenciaBase, opsElegidas, valoresDespues, costosAplicados, ref mejorCosto, mejorOps, mejorValores, mejorCostos);

            return mejorCosto < poda ? ConstruirDatos(secuenciaBase, mejorOps, mejorValores, mejorCostos) : null;
        }

        // ====================================================================
        // NÚCLEO ÚNICO PARA ASCENDENTE / DESCENDENTE (Poda por comparación de último estado)
        // ====================================================================
        private void ResolverEstrategia(int idx, int costoAcumulado, int ultimoValorTransformado, bool esAscendente,
            int[] seq, string[] ops, int[] valsDespues, int[] costos,
            ref int mejorCostoGlobal, string[] mOps, int[] mVals, int[] mCostos)
        {
            // PODA POR COSTO: Si ya igualamos o superamos el mejor costo global, matamos la rama de inmediato
            if (costoAcumulado >= mejorCostoGlobal) return;

            // CASO BASE: Llegamos al final con éxito manteniendo el criterio en cada paso
            if (idx == seq.Length)
            {
                mejorCostoGlobal = costoAcumulado;
                Array.Copy(ops, mOps, seq.Length);
                Array.Copy(valsDespues, mVals, seq.Length);
                Array.Copy(costos, mCostos, seq.Length);
                return;
            }

            int valOriginal = seq[idx];

            // Evaluamos las 5 opciones en orden de costo (0 primero para potenciar la poda rápida)

            // 1. NINGUNA (Costo 0)
            if (idx == 0 || (esAscendente ? valOriginal >= ultimoValorTransformado : valOriginal <= ultimoValorTransformado))
            {
                AsignarYResolver(idx, costoAcumulado, valOriginal, "Ninguna", 0, esAscendente, seq, ops, valsDespues, costos, ref mejorCostoGlobal, mOps, mVals, mCostos);
            }

            // 2. +1 (Costo 1)
            int valMas1 = valOriginal + 1;
            if (idx == 0 || (esAscendente ? valMas1 >= ultimoValorTransformado : valMas1 <= ultimoValorTransformado))
            {
                AsignarYResolver(idx, costoAcumulado + costoMas1, valMas1, "+1", costoMas1, esAscendente, seq, ops, valsDespues, costos, ref mejorCostoGlobal, mOps, mVals, mCostos);
            }

            // 3. -1 (Costo 1)
            int valMenos1 = valOriginal - 1;
            if (valMenos1 >= 0 && (idx == 0 || (esAscendente ? valMenos1 >= ultimoValorTransformado : valMenos1 <= ultimoValorTransformado)))
            {
                AsignarYResolver(idx, costoAcumulado + costoMenos1, valMenos1, "-1", costoMenos1, esAscendente, seq, ops, valsDespues, costos, ref mejorCostoGlobal, mOps, mVals, mCostos);
            }

            // 4. /2 (Costo 2, solo si es par)
            if (valOriginal % 2 == 0)
            {
                int valDiv2 = valOriginal / 2;
                if (idx == 0 || (esAscendente ? valDiv2 >= ultimoValorTransformado : valDiv2 <= ultimoValorTransformado))
                {
                    AsignarYResolver(idx, costoAcumulado + costoDiv2, valDiv2, "/2", costoDiv2, esAscendente, seq, ops, valsDespues, costos, ref mejorCostoGlobal, mOps, mVals, mCostos);
                }
            }

            // 5. *2 (Costo 3)
            int valPor2 = valOriginal * 2;
            if (idx == 0 || (esAscendente ? valPor2 >= ultimoValorTransformado : valPor2 <= ultimoValorTransformado))
            {
                AsignarYResolver(idx, costoAcumulado + costoPor2, valPor2, "*2", costoPor2, esAscendente, seq, ops, valsDespues, costos, ref mejorCostoGlobal, mOps, mVals, mCostos);
            }
        }

        private void AsignarYResolver(int idx, int nuevoCosto, int nuevoValor, string opStr, int costoOp, bool esAscendente,
            int[] seq, string[] ops, int[] valsDespues, int[] costos, ref int mejorCostoGlobal, string[] mOps, int[] mVals, int[] mCostos)
        {
            ops[idx] = opStr;
            valsDespues[idx] = nuevoValor;
            costos[idx] = costoOp;

            ResolverEstrategia(idx + 1, nuevoCosto, nuevoValor, esAscendente, seq, ops, valsDespues, costos, ref mejorCostoGlobal, mOps, mVals, mCostos);
        }

        // ====================================================================
        // ALGORITMO EXCLUSIVO PARA COSTO CONSTANTE
        // ====================================================================
        private void ResolverConstanteRecursivo(int idx, int costoAcumulado, int valorPivote, int[] seq,
            string[] ops, int[] valsDespues, int[] costos, ref int mejorCostoGlobal, string[] mOps, int[] mVals, int[] mCostos)
        {
            if (costoAcumulado >= mejorCostoGlobal) return;

            if (idx == seq.Length)
            {
                mejorCostoGlobal = costoAcumulado;
                Array.Copy(ops, mOps, seq.Length);
                Array.Copy(valsDespues, mVals, seq.Length);
                Array.Copy(costos, mCostos, seq.Length);
                return;
            }

            int valOriginal = seq[idx];

            // Arreglos locales para iterar las 5 opciones de forma limpia
            int valDiv2 = (valOriginal % 2 == 0) ? (valOriginal / 2) : -1;
            int[] opcionesValores = { valOriginal, valOriginal + 1, valOriginal - 1, valDiv2, valOriginal * 2 };
            string[] opcionesOps = { "Ninguna", "+1", "-1", "/2", "*2" };
            int[] opcionesCostos = { 0, costoMas1, costoMenos1, costoDiv2, costoPor2 };

            for (int i = 0; i < 5; i++)
            {
                int nuevoValor = opcionesValores[i];
                if (nuevoValor < 0) continue; // Descarta divisiones impares o restas negativas

                // Si no es el primer elemento, el valor DEBE ser igual al pivote establecido al inicio
                if (idx > 0 && nuevoValor != valorPivote) continue;

                ops[idx] = opcionesOps[i];
                valsDespues[idx] = nuevoValor;
                costos[idx] = opcionesCostos[i];

                ResolverConstanteRecursivo(idx + 1, costoAcumulado + opcionesCostos[i], idx == 0 ? nuevoValor : valorPivote, seq, ops, valsDespues, costos, ref mejorCostoGlobal, mOps, mVals, mCostos);
            }
        }

        private Datos ConstruirDatos(int[] originales, string[] ops, int[] despues, int[] costos)
        {
            var datos = new Datos();
            for (int i = 0; i < originales.Length; i++)
            {
                datos.Operaciones.Add((i, ops[i], originales[i], despues[i]));
                datos.Costos.Add(costos[i]);
                datos.Secuencia.Add(despues[i]);
            }
            return datos;
        }
    }

    // Clase para Guardar Datos de Secuencia
    public class Datos
    {
        public List<(int indice, string operacion, int valorAntes, int valorDespues)> Operaciones { get; set; } = new List<(int, string, int, int)>();
        public List<int> Costos { get; set; } = new List<int>();
        public List<int> Secuencia { get; set; } = new List<int>();
    }
}