# Contributing to Ledger Vault

Thank you for your interest in contributing to Ledger Vault! This document provides guidelines and standards for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Code Style and Standards](#code-style-and-standards)
- [Architecture Patterns](#architecture-patterns)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Reporting Issues](#reporting-issues)

## Code of Conduct

This project and its contributors are governed by our Code of Conduct. By participating, you are expected to uphold this code.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Create a feature branch** for your changes
4. **Make your changes** following our coding standards
5. **Test your changes** thoroughly
6. **Submit a pull request**

## Development Setup

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022, VS Code, or Rider
- Git

### Local Setup

```bash
# Clone your fork
git clone https://github.com/alexandrubunea/ledger-vault.git
cd ledger-vault

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## Code Style and Standards

### C# Coding Standards

We follow Microsoft's C# coding conventions with some project-specific additions:

#### Naming Conventions
- **Classes**: PascalCase (e.g., `TransactionService`, `UserStateService`)
- **Methods**: PascalCase (e.g., `GetLastHash()`, `SaveTransaction()`)
- **Properties**: PascalCase (e.g., `FullUserName`, `CurrentBalance`)
- **Private fields**: camelCase with underscore prefix (e.g., `_subscribers`, `_hmacKey`)
- **Constants**: PascalCase (e.g., `Purpose`, `MaxBarWidth`)
- **Enums**: PascalCase (e.g., `TransactionType`, `HashStatus`)

#### Code Organization
Use explicit region organization for all classes:

```csharp
public class ExampleService
{
    #region PUBLIC PROPERTIES
    
    public string PublicProperty { get; set; }
    
    #endregion

    #region PUBLIC API
    
    public void PublicMethod()
    {
        // Implementation
    }
    
    #endregion

    #region PRIVATE PROPERTIES
    
    private readonly string _privateField;
    
    #endregion

    #region PRIVATE METHODS
    
    private void PrivateMethod()
    {
        // Implementation
    }
    
    #endregion
}
```

#### Using Statements
- Place using statements at the top of the file
- Group related using statements together
- Use global using statements where appropriate

#### Nullable Reference Types
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use nullable annotations appropriately (`string?`, `Transaction?`)
- Handle null cases explicitly

### File Organization

#### Project Structure
```
ledger-vault/
├── Crypto/           # Cryptographic operations
├── Data/            # Enums and data structures
├── Extensions/      # Extension methods
├── Factories/       # Factory pattern implementations
├── Messaging/       # Message classes for communication
├── Models/          # Domain models
├── Services/        # Business logic services
├── Styles/          # UI styling
├── ViewModels/      # MVVM view models
└── Views/           # UI views
```

#### File Naming
- **Services**: `{ServiceName}Service.cs` (e.g., `TransactionService.cs`)
- **ViewModels**: `{ViewModelName}ViewModel.cs` (e.g., `HomeViewModel.cs`)
- **Views**: `{ViewName}View.axaml` and `{ViewName}View.axaml.cs`
- **Models**: `{ModelName}.cs` (e.g., `Transaction.cs`)
- **Messages**: `{MessageName}Message.cs` (e.g., `AddToTransactionListMessage.cs`)

## Architecture Patterns

### MVVM Pattern
- Use `CommunityToolkit.Mvvm` for MVVM implementation
- Inherit from `ObservableObject` for view models
- Use `[ObservableProperty]` attribute for properties
- Implement `INotifyPropertyChanged` through the base class

### Dependency Injection
- Register all services in `Extensions/AddServices.cs`
- Use constructor injection for dependencies
- Prefer singleton services for stateless operations
- Use transient services for view models

### Factory Pattern
- Use factories only when multiple implementations need to be switched between
- Implement factories for view models that require dynamic creation or switching
- Simple, single-purpose view models don't need factories
- Keep factories focused on managing complex object creation scenarios

### Mediator Pattern
- Use `MediatorService<TMessage>` for loose coupling
- Define message classes in the `Messaging/` folder
- Keep messages simple and focused on single responsibilities

### Repository Pattern
- Implement repositories for data access
- Use async/await for database operations
- Handle exceptions gracefully with meaningful error messages
- Use parameterized queries to prevent SQL injection

## Testing Guidelines

### Testing
- **Manual testing** is required for all changes
- Test the application functionality with your changes
- Verify UI behavior and data flow
- Ensure no new warnings or errors are introduced
- Test database operations and cryptographic functions manually
- Verify transaction chain integrity and signature verification

*Note: Unit testing and automated testing frameworks are not currently implemented in this project. Focus on thorough manual testing and code review.*

## Pull Request Process

### Before Submitting

1. **Ensure your code compiles** without warnings
2. **Test the application** thoroughly with your changes
3. **Follow the coding standards** outlined above
4. **Update documentation** if needed
5. **Verify no new warnings** are introduced

### Pull Request Template

Use this template when creating a pull request:

```markdown
## Description

Brief description of the changes made.

## Type of Change

- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing

- [ ] Manual testing completed
- [ ] Application functionality verified
- [ ] No new warnings introduced
- [ ] Database operations tested (if applicable)
- [ ] Cryptographic functions verified (if applicable)

## Checklist

- [ ] My code follows the style guidelines of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have manually tested my changes thoroughly
- [ ] My changes work as expected and don't break existing functionality
- [ ] Any dependent changes have been merged and published

## Additional Notes

Any additional information or context for the reviewers.
```

### Review Process

1. **Automated checks** must pass (build, code style)
2. **Code review** by maintainers
3. **Address feedback** and make requested changes
4. **Maintainer approval** required for merge

## Commit Message Guidelines

Use conventional commit format:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, missing semicolons, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples
```
feat(transactions): add support for transaction categories
fix(auth): resolve password validation issue
docs(readme): update installation instructions
refactor(services): simplify transaction loading logic
```

## Reporting Issues

### Bug Reports

When reporting bugs, please include:

1. **Clear description** of the problem
2. **Steps to reproduce** the issue
3. **Expected behavior** vs. actual behavior
4. **Environment details** (OS, .NET version, etc.)
5. **Screenshots** if applicable
6. **Error messages** and stack traces

### Feature Requests

When requesting features:

1. **Clear description** of the desired functionality
2. **Use case** and benefits
3. **Implementation suggestions** if you have any
4. **Priority level** (low, medium, high)

## Getting Help

- **GitHub Issues**: For bug reports and feature requests
- **GitHub Discussions**: For questions and general discussion
- **Code Review**: Ask questions in pull request comments

## License

By contributing to Ledger Vault, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to Ledger Vault! Your contributions help make this project better for everyone.
