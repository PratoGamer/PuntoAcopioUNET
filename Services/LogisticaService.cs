using System;
using System.Collections.Generic;
using System.Text;

namespace PuntoAcopioUNET.Services
{
    // Clase Principal del Servicio de Logistica
    public class LogisticaService
    {

        // Lista para Almacenar la Secuencia de Numeros Originales
        public List<int> secuenciaOriginal { get; set; } = new List<int>();

        // Diccionario para los Costos de Operacion
        public Dictionary<string, int> operacion { get; set; } = new Dictionary<string, int>()
        {
            { "+1", 1 },
            { "-1", 1 },
            { "*2", 3 },
            { "/2", 2 }
        };

        // Resultado Ascendente

        // Resultado Descendente

        // Resultado Constante

        // Mejor Costo Encontrado
        private int mejorCostoAsc, mejorCostoDesc, mejorCostoConst;

        // Poda en Profundidad
        private int poda = 60;

        // Metodo para Ejecutar la Logistica
        public async Task CalcularOrden()
        {

        }

        // Metodo Recursivo con Memorizacion Ascendente


        // Metodo Recursivo con Memorizacion Descendente


        // Metodo Recursivo con Memorizacion Constante




    }

    // Clase para Guardar Datos de Secuencia
    public class Datos
    {
        public List<(int indice, string operacion, int valorAntes, int valorDespues)> Operaciones { get; set; } = new List<(int, string, int, int)>();
        public List<int> Costos { get; set; } = new List<int>();
        public List<int> Secuencia { get; set; } = new List<int>();
    }
}
