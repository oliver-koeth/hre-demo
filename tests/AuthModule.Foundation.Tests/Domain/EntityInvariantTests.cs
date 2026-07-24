using AuthModule.Foundation.Domain.Entities;
using FluentAssertions;

namespace AuthModule.Foundation.Tests.Domain;

public sealed class EntityInvariantTests
{
    [Fact]
    public void RolePermissionAssignment_Window_Is_Valid_When_ValidFrom_Less_Than_ValidUntil()
    {
        var now = DateTimeOffset.UtcNow;
        var assignment = new RolePermissionAssignment
        {
            AssignmentId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            ValidFrom = now,
            ValidUntil = now.AddHours(1),
        };

        assignment.HasValidWindow().Should().BeTrue();
        assignment.IsActiveAt(now.AddMinutes(30)).Should().BeTrue();
    }

    [Fact]
    public void RolePermissionAssignment_Window_Is_Invalid_When_ValidFrom_Greater_Than_ValidUntil()
    {
        var now = DateTimeOffset.UtcNow;
        var assignment = new RolePermissionAssignment
        {
            AssignmentId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            ValidFrom = now.AddHours(2),
            ValidUntil = now,
        };

        assignment.HasValidWindow().Should().BeFalse();
    }
}

