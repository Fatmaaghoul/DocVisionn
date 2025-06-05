using System.Text;

namespace Docvision.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Loguer la requête
            var request = await FormatRequest(context.Request);
            _logger.LogInformation("Requête HTTP : {Request}", request);

            // Capturer la réponse
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Loguer la réponse
            var response = await FormatResponse(context.Response);
            _logger.LogInformation("Réponse HTTP : {Response}", response);

            // Restaurer le flux de réponse original
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering(); // Permet de relire le corps
            var body = string.Empty;
            using (var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Réinitialiser la position
            }

            return $"[{request.Method}] {request.Scheme}://{request.Host}{request.Path}{request.QueryString}\nCorps : {body}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"Code de statut : {response.StatusCode}\nCorps : {text}";
        }
    }
}
