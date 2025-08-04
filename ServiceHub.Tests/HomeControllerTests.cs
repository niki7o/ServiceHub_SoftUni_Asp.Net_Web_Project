//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Moq;
//using ServiceHub.Controllers;
//using ServiceHub.Core.Models;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace ServiceHub.Tests
//{
//    public class HomeControllerTests
//    {
//        private readonly Mock<ILogger<HomeController>> _mockLogger;
//        private readonly HomeController _controller;

//        public HomeControllerTests()
//        {
//            _mockLogger = new Mock<ILogger<HomeController>>();
//            _controller = new HomeController(_mockLogger.Object);
//        }

//        [Fact]
//        public void Index_ReturnsViewResult()
//        {
          
//            var result = _controller.Index();

          
//            Assert.IsType<ViewResult>(result);
//        }

//        [Fact]
//        public void Plans_ReturnsViewResult()
//        {
           
//            var result = _controller.Plans();

            
//            Assert.IsType<ViewResult>(result);
//        }

//        [Fact]
//        public void Error_ReturnsViewResult_WithErrorViewModel()
//        {
            
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext()
//            };
//            _controller.HttpContext.TraceIdentifier = "test-trace-id";

            
//            var result = _controller.Error();

            
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
//            Assert.NotNull(model.RequestId);
           
//            Assert.True(model.RequestId == Activity.Current?.Id || model.RequestId == "test-trace-id");
//        }
//    }
//}
