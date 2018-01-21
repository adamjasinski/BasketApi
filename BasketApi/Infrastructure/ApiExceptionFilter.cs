using System.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BasketApi.Infrastructure
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public bool IncludeDetails { get; set; }
        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            if (exception is ResourceAccessNotPermittedException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Result = new EmptyResult();
                base.OnException(context);
                return;
            }

            // All unhandled exceptions should be translated to a JSON error
            context.HttpContext.Response.StatusCode = 500;
            dynamic errorRepresentation = new ExpandoObject();
            errorRepresentation.Error = exception.Message;
            errorRepresentation.ErrorType = exception.GetType().FullName;
            if (IncludeDetails)
            {
                errorRepresentation.Details = exception;
            }

            context.Result = new JsonResult(errorRepresentation);

            base.OnException(context);
        }
    }
}
