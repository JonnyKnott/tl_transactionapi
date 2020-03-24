using System.Collections.Generic;
using System.Linq;

namespace TrueLayer.TransactionData.Models.ServiceModels
{
    public class ServiceResult
    {
        private ServiceResult()
        {
            
        }
        private ServiceResult(ICollection<string> errors)
        {
            Errors = errors;
        }

        public bool Success => Errors == null || !Errors.Any();
        public ICollection<string> Errors { get; private set; } 
        
        public static ServiceResult Succeeded()
        {
            return new ServiceResult();
        }

        public static ServiceResult Failed(ICollection<string> errors)
        {
            return new ServiceResult(errors);
        }
    }
}