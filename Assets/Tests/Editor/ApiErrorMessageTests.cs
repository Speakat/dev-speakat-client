using System;
using System.Collections.Generic;
using NUnit.Framework;
using Speakat.Client;

public class ApiErrorMessageTests
{
    [Test]
    public void From_RegularException_ReturnsExceptionMessage()
    {
        var exception = new InvalidOperationException(nameof(ApiErrorMessageTests));

        Assert.That(ApiErrorMessage.From(exception), Is.EqualTo(nameof(ApiErrorMessageTests)));
    }

    [Test]
    public void From_ApiExceptionWithResponse_IncludesStatusAndResponse()
    {
        const int statusCode = 401;
        var exception = new ApiException(
            nameof(ApiErrorMessageTests),
            statusCode,
            nameof(ApiException.Response),
            new Dictionary<string, IEnumerable<string>>(),
            null);

        string message = ApiErrorMessage.From(exception);

        Assert.That(message, Does.Contain(statusCode.ToString()));
        Assert.That(message, Does.Contain(nameof(ApiException.Response)));
    }
}
