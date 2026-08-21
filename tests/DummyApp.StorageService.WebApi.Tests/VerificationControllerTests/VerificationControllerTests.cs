using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.VerificationControllerTests;

public sealed class VerificationControllerTests
{
    [Fact]
    public async Task CreateVerificationCode_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.CreateVerificationCode(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateVerificationCode_ReturnsBadRequest_WhenEmailIsInvalid()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.CreateVerificationCode(new CreateVerificationCodeRequest { Email = "adminexample.com", Code = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(10) });

        Assert.IsType<BadRequestObjectResult>(result);
        serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateVerificationCode_ReturnsBadRequest_WhenServiceFails()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        serviceMock.Setup(x => x.CreateVerificationCodeAsync("admin@example.com", "123456", It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.CreateVerificationCode(new CreateVerificationCodeRequest { Email = "admin@example.com", Code = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(10) });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateVerificationCode_ReturnsOk_WhenServiceSucceeds()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        serviceMock.Setup(x => x.CreateVerificationCodeAsync("admin@example.com", "123456", It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.CreateVerificationCode(new CreateVerificationCodeRequest { Email = "admin@example.com", Code = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(10) });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task VerifyVerificationCode_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.VerifyVerificationCode(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VerifyVerificationCode_ReturnsBadRequest_WhenEmailOrCodeIsInvalid()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.VerifyVerificationCode(new VerifyVerificationCodeRequest { Email = "adminexample.com", Code = "12345" });

        Assert.IsType<BadRequestObjectResult>(result);
        serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VerifyVerificationCode_ReturnsBadRequest_WhenServiceFails()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        serviceMock.Setup(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456"))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.VerifyVerificationCode(new VerifyVerificationCodeRequest { Email = "admin@example.com", Code = "123456" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task VerifyVerificationCode_ReturnsOk_WhenServiceSucceeds()
    {
        var serviceMock = new Mock<IVerificationCodeService>();
        serviceMock.Setup(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456"))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(serviceMock, loggerMock);

        var result = await controller.VerifyVerificationCode(new VerifyVerificationCodeRequest { Email = "admin@example.com", Code = "123456" });

        Assert.IsType<OkResult>(result);
    }

    private static VerificationController CreateController(Mock<IVerificationCodeService> serviceMock, Mock<ILogger<VerificationController>> loggerMock)
    {
        var completedOrdersServiceMock = new Mock<ICompletedOrdersService>();

        return new VerificationController(serviceMock.Object, completedOrdersServiceMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}
