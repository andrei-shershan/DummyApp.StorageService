using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.VerificationControllerTests;

public sealed class CompletedOrdersVerificationControllerTests
{
    [Fact]
    public async Task CreateCompletedOrdersToken_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var serviceMock = new Mock<ICompletedOrdersService>();
        var verificationServiceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(verificationServiceMock, serviceMock, loggerMock);

        var result = await controller.CreateCompletedOrdersToken(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        serviceMock.VerifyNoOtherCalls();
        verificationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCompletedOrdersToken_ReturnsBadRequest_WhenEmailIsInvalid()
    {
        var serviceMock = new Mock<ICompletedOrdersService>();
        var verificationServiceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(verificationServiceMock, serviceMock, loggerMock);

        var result = await controller.CreateCompletedOrdersToken(new CreateCompletedOrdersTokenRequest
        {
            Email = "adminexample.com",
            Token = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });

        Assert.IsType<BadRequestObjectResult>(result);
        serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCompletedOrdersToken_ReturnsBadRequest_WhenExpirationIsNotFuture()
    {
        var serviceMock = new Mock<ICompletedOrdersService>();
        var verificationServiceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(verificationServiceMock, serviceMock, loggerMock);

        var result = await controller.CreateCompletedOrdersToken(new CreateCompletedOrdersTokenRequest
        {
            Email = "admin@example.com",
            Token = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1)
        });

        Assert.IsType<BadRequestObjectResult>(result);
        serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateCompletedOrdersToken_ReturnsInternalServerError_WhenServiceFails()
    {
        var serviceMock = new Mock<ICompletedOrdersService>();
        serviceMock.Setup(x => x.CreateCompletedOrdersTokenAsync("admin@example.com", It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var verificationServiceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(verificationServiceMock, serviceMock, loggerMock);

        var result = await controller.CreateCompletedOrdersToken(new CreateCompletedOrdersTokenRequest
        {
            Email = "admin@example.com",
            Token = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateCompletedOrdersToken_ReturnsOk_WhenServiceSucceeds()
    {
        var serviceMock = new Mock<ICompletedOrdersService>();
        serviceMock.Setup(x => x.CreateCompletedOrdersTokenAsync("admin@example.com", It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var verificationServiceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(verificationServiceMock, serviceMock, loggerMock);

        var result = await controller.CreateCompletedOrdersToken(new CreateCompletedOrdersTokenRequest
        {
            Email = "admin@example.com",
            Token = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });

        Assert.IsType<OkResult>(result);
    }

    private static VerificationController CreateController(Mock<IVerificationCodeService> verificationServiceMock, Mock<ICompletedOrdersService> serviceMock, Mock<ILogger<VerificationController>> loggerMock)
    {
        var controller = new VerificationController(verificationServiceMock.Object, serviceMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }
}
