using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Controllers;
using ServiceHub.Core.Models;
using ServiceHub.Data.Models;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new HomeController(_mockLogger.Object, _mockUserManager.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Plans_ReturnsViewResult()
        {
            var testUser = new ApplicationUser { Id = "testUserId", UserName = "testuser" };
            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);

            var result = await _controller.Plans();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ApplicationUser>(viewResult.Model);
            Assert.Equal(testUser, viewResult.Model);
        }

        [Fact]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.TraceIdentifier = "test-trace-id";

            var result = _controller.Error();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);

            Assert.True(model.RequestId == Activity.Current?.Id || model.RequestId == "test-trace-id");
        }
    }
}
