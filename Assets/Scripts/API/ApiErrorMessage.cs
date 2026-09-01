using System;
using Speakat.Client;

public static class ApiErrorMessage
{
    public static string From(Exception exception)
    {
        if (exception is ApiException apiException)
        {
            string response = string.IsNullOrWhiteSpace(apiException.Response)
                ? apiException.Message
                : apiException.Response;

            return $"HTTP {apiException.StatusCode}: {response}";
        }

        return exception?.Message ?? "Unknown error";
    }
}
