using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TiengAnh.Middleware
{
    public class ValidationErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Call the next middleware in the pipeline
            await _next(context);

            // Check if we're handling a POST request for AddVocabulary
            if (context.Request.Path.Value?.EndsWith("/Content/AddVocabulary", StringComparison.OrdinalIgnoreCase) == true &&
                context.Request.Method == "POST")
            {
                // Get the temp data from the context
                var tempData = context.RequestServices
                    .GetRequiredService<ITempDataDictionaryFactory>()
                    .GetTempData(context);

                // If we have error messages related to ImageFile being required, remove them
                if (tempData["ErrorMessage"] != null && 
                    tempData["ErrorMessage"].ToString().Contains("The ImageFile field is required"))
                {
                    // Replace the error message or remove it entirely
                    tempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm từ vựng. Vui lòng kiểm tra lại thông tin.";
                }
                
                // Get the ModelState from context items (if available)
                if (context.Items.ContainsKey("ModelState") && context.Items["ModelState"] is ModelStateDictionary modelState)
                {
                    if (modelState.ContainsKey("ImageFile"))
                    {
                        modelState.Remove("ImageFile");
                    }
                }
            }
        }
    }
}
