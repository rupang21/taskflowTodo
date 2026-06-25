using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Api.Controllers;
using ToDoApp.Api.Data;
using ToDoApp.Api.DTOs;
using ToDoApp.Api.Models;
using Xunit;

namespace ToDoApp.Tests
{
    public class TodosControllerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<ILogger<TodosController>> _loggerMock;
        private readonly TodosController _controller;
        private readonly string _testUserId = "test_user_123";

        public TodosControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _loggerMock = new Mock<ILogger<TodosController>>();

            _controller = new TodosController(_context, _loggerMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsOnlyUserTodos()
        {
            // Arrange
            _context.TodoItems.Add(new TodoItem { Title = "User1 Todo", UserId = _testUserId, Status = TodoStatus.Pending });
            _context.TodoItems.Add(new TodoItem { Title = "User2 Todo", UserId = "another_user", Status = TodoStatus.Pending });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll(null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<TodoResponseDto>>(okResult.Value);
            Assert.Single(items);
            Assert.Equal("User1 Todo", items.First().Title);
        }

        [Fact]
        public async Task Create_ValidTodo_ReturnsCreatedItem()
        {
            // Arrange
            var createDto = new CreateTodoDto
            {
                Title = "New Task",
                Description = "Task Description",
                Priority = TodoPriority.High
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var item = Assert.IsType<TodoResponseDto>(createdAtResult.Value);
            Assert.Equal("New Task", item.Title);
            Assert.Equal("Task Description", item.Description);
            Assert.Equal(TodoPriority.High, item.Priority);
            
            // Verify DB state
            var dbItem = await _context.TodoItems.FindAsync(item.Id);
            Assert.NotNull(dbItem);
            Assert.Equal(_testUserId, dbItem.UserId);
        }
    }
}
