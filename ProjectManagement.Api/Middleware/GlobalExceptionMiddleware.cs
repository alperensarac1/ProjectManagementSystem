using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Models;

namespace ProjectManagement.Api.Middleware;


public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<GlobalExceptionMiddleware>
        _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
        
            _logger.LogInformation(
                "HTTP isteği istemci tarafından iptal edildi. Path: {Path}",
                context.Request.Path);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
   
        if (context.Response.HasStarted)
        {
            _logger.LogError(
                exception,
                "Response başladıktan sonra hata oluştu. Path: {Path}",
                context.Request.Path);

            throw exception;
        }

        var statusCode =
            GetStatusCode(exception);

        var message =
            GetMessage(exception);

        var errors =
            GetErrors(exception);

    
        if (statusCode >=
            StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Beklenmeyen uygulama hatası oluştu. " +
                "Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Kontrollü uygulama hatası oluştu. " +
                "StatusCode: {StatusCode}, Method: {Method}, Path: {Path}",
                statusCode,
                context.Request.Method,
                context.Request.Path);
        }

        var response =
            ApiResponse<object>.Fail(
                message,
                errors);

        context.Response.Clear();

        context.Response.StatusCode =
            statusCode;

        context.Response.ContentType =
            "application/json; charset=utf-8";

   
        await context.Response.WriteAsJsonAsync(
            response,
            cancellationToken:
                context.RequestAborted);
    }

    private static int GetStatusCode(
        Exception exception)
    {
        return exception switch
        {
       
            RequestValidationException =>
                StatusCodes.Status400BadRequest,

        
            BusinessRuleException =>
                StatusCodes.Status400BadRequest,

         
            AuthenticationFailedException =>
                StatusCodes.Status401Unauthorized,

         
            UnauthorizedAccessAppException =>
                StatusCodes.Status401Unauthorized,

          
            ForbiddenException =>
                StatusCodes.Status403Forbidden,

          
            NotFoundException =>
                StatusCodes.Status404NotFound,

         
            ConflictException =>
                StatusCodes.Status409Conflict,

          
            DbUpdateConcurrencyException =>
                StatusCodes.Status409Conflict,

            DbUpdateException =>
                StatusCodes.Status409Conflict,


            _ =>
                StatusCodes.Status500InternalServerError
        };
    }

    private static string GetMessage(
        Exception exception)
    {

        if (exception is
            RequestValidationException or
            BusinessRuleException or
            AuthenticationFailedException or
            UnauthorizedAccessAppException or
            ForbiddenException or
            NotFoundException or
            ConflictException)
        {
            return exception.Message;
        }

        if (exception is DbUpdateConcurrencyException)
        {
            return
                "Kayıt başka bir işlem tarafından değiştirilmiştir. " +
                "Lütfen bilgileri yenileyip tekrar deneyiniz.";
        }

        if (exception is DbUpdateException)
        {
            return
                "Veritabanı işlemi sırasında bir çakışma oluştu.";
        }


        return
            "İşlem sırasında beklenmeyen bir sunucu hatası oluştu.";
    }


    private static IReadOnlyDictionary<string, string[]>?
        GetErrors(
            Exception exception)
    {
        return exception is RequestValidationException
            validationException
                ? validationException.Errors
                : null;
    }
}