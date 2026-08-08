using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace TravelBuddy.Statistics
{
    public class ApiCallLog : AggregateRoot<Guid>
    {
        public string Endpoint { get; set; }        // Qué endpoint llamamos (ej: /search)
        public int StatusCode { get; set; }         // Resultado (200, 400, 500)
        public long ResponseTimeMs { get; set; }    // Cuántos milisegundos tardó
        public bool IsSuccess { get; set; }         // Salio bien?
        public string ErrorMessage { get; set; }    // Si fallo, qué error dio
        public DateTime Timestamp { get; set; }     // Cuándo ocurrió

        protected ApiCallLog() { }

        public ApiCallLog(Guid id, string endpoint, int statusCode, long responseTimeMs, bool isSuccess, string errorMessage = null) : base(id)
        {
            Id = id;
            Endpoint = endpoint;
            StatusCode = statusCode;
            ResponseTimeMs = responseTimeMs;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Timestamp = DateTime.Now;
        }
    }
}