using System.Collections.Generic;
using System.Linq;

namespace TrueLayer.TransactionData.Models.ServiceModels
{
    public class ServiceObjectResult<TResultType> : ServiceResult
    {
        protected ServiceObjectResult(TResultType result)
        {
            Result = result;
        }

        protected ServiceObjectResult(TResultType result, ICollection<string> errors) : base(errors)
        {
            Result = result;
        }
        
        public TResultType Result { get; }

        public static ServiceObjectResult<TResultType> Succeeded(TResultType result)
        {
            return new ServiceObjectResult<TResultType>(result);
        }
        
        public static ServiceObjectResult<TResultType> Failed(TResultType result, ICollection<string> errors)
        {
            return new ServiceObjectResult<TResultType>(result, errors);
        }
    }
    public class ServiceResult
    {
        protected ServiceResult()
        {
            
        }
        protected ServiceResult(ICollection<string> errors)
        {
            Errors = errors;
        }

        public bool Success => Errors == null;
        public ICollection<string> Errors { get; } 
        
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