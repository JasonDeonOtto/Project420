# CLAUDE.md - AI Assistant Development Guide

## About This File

This file provides comprehensive documentation for AI assistants (like Claude) working on this codebase. It contains essential information about project structure, development workflows, conventions, and best practices.

**Last Updated:** 2025-12-07
**Repository:** JasonDeonOtto/Project420
**Status:** Empty repository - awaiting initial project setup

---

## 🚀 Quick Start

### Repository Status
This is a newly initialized repository with no code yet. As development begins, this section will be updated with:
- Prerequisites and dependencies
- Installation instructions
- How to run the project locally
- How to run tests

### Initial Setup Commands
```bash
# Clone the repository
git clone <repository-url>
cd Project420

# Install dependencies (to be added when project structure is defined)
# npm install / pip install -r requirements.txt / etc.

# Run the project (to be added)
# npm start / python main.py / etc.
```

---

## 📁 Project Structure

> **Note:** This section will be populated as the codebase develops.

Expected structure template:
```
Project420/
├── src/                    # Source code
│   ├── components/         # Reusable components
│   ├── services/          # Business logic/services
│   ├── utils/             # Utility functions
│   └── config/            # Configuration files
├── tests/                 # Test files
├── docs/                  # Documentation
├── scripts/               # Build/deployment scripts
├── .github/               # GitHub workflows and templates
├── package.json           # Dependencies (if Node.js)
├── requirements.txt       # Dependencies (if Python)
├── README.md              # User-facing documentation
└── CLAUDE.md             # This file
```

---

## 🛠 Development Workflow

### Branching Strategy
- **Main/Master Branch:** Production-ready code
- **Feature Branches:** `feature/description` or `claude/session-id`
- **Bug Fix Branches:** `fix/description`
- **Claude Branches:** AI development branches follow pattern `claude/claude-md-*`

### Commit Conventions
Follow conventional commits format:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Example:**
```
feat(auth): add user authentication system

Implemented JWT-based authentication with login and logout endpoints.
Includes password hashing and token validation.

Closes #123
```

### Code Review Process
- All changes require review before merging to main
- Run tests and linting before creating PR
- Update documentation for user-facing changes
- Keep PRs focused and reasonably sized

---

## 🎯 AI Assistant Guidelines

### Before Making Changes
1. **Always read files before modifying them**
   - Use Read tool to understand existing code
   - Never propose changes to unread code
   - Understand context and existing patterns

2. **Search before creating**
   - Check if functionality already exists
   - Look for similar patterns in the codebase
   - Avoid duplicating code

3. **Plan complex tasks**
   - Use TodoWrite for multi-step tasks
   - Break down large features into smaller steps
   - Track progress transparently

### Code Quality Standards

#### General Principles
- **KISS (Keep It Simple, Stupid):** Avoid over-engineering
- **DRY (Don't Repeat Yourself):** Extract common patterns
- **YAGNI (You Aren't Gonna Need It):** Don't add speculative features
- **Single Responsibility:** Each function/class should do one thing well

#### Security
Always check for and prevent:
- SQL injection
- XSS (Cross-Site Scripting)
- CSRF (Cross-Site Request Forgery)
- Command injection
- Path traversal
- Insecure authentication/authorization
- Exposure of sensitive data
- OWASP Top 10 vulnerabilities

#### Error Handling
- Validate input at system boundaries (user input, APIs)
- Don't add error handling for impossible scenarios
- Trust internal code and framework guarantees
- Provide meaningful error messages
- Log errors appropriately

#### Testing
- Write tests for new features
- Update tests when modifying existing code
- Ensure tests pass before committing
- Aim for meaningful coverage, not just high percentages

### What NOT to Do
❌ **Avoid over-engineering:**
- Don't add features beyond requirements
- Don't create abstractions for single-use code
- Don't add comments to unchanged code
- Don't add type hints/docstrings unless modifying that code
- Don't add error handling for scenarios that can't happen

❌ **Avoid backwards-compatibility hacks:**
- Don't rename unused vars with `_` prefix
- Don't add `// removed` comments
- Don't keep dead code
- If unused, delete it completely

❌ **Avoid making assumptions:**
- Don't guess at URLs or endpoints
- Don't invent configuration values
- Don't assume project structure without verification
- Ask for clarification when uncertain

### File Operations
- **Prefer editing over creating:** Always modify existing files when possible
- **Read before writing:** Use Read tool before Edit/Write
- **Don't create unnecessary files:** No speculative documentation
- **Use appropriate tools:**
  - `Read` for reading files (not `cat`)
  - `Edit` for modifying files (not `sed`)
  - `Write` for new files only
  - `Grep` for searching content
  - `Glob` for finding files

---

## 🧪 Testing Strategy

> **To be defined** based on project technology stack

### Test Types
- **Unit Tests:** Test individual functions/components
- **Integration Tests:** Test component interactions
- **End-to-End Tests:** Test complete user workflows
- **Performance Tests:** Test system performance under load

### Running Tests
```bash
# To be added when test framework is chosen
# npm test / pytest / go test / etc.
```

### Test Coverage Goals
- Aim for meaningful coverage of critical paths
- 100% coverage is not the goal; quality over quantity
- Focus on testing business logic and edge cases

---

## 📦 Dependencies and Package Management

> **To be defined** based on project technology stack

### Adding Dependencies
1. Evaluate necessity and security
2. Check license compatibility
3. Consider bundle size impact (for frontend)
4. Document why dependency is needed
5. Pin versions for reproducibility

### Updating Dependencies
- Review changelogs before updating
- Test thoroughly after updates
- Update one major dependency at a time
- Document breaking changes

---

## 🔧 Configuration

> **To be defined** based on project needs

### Environment Variables
```bash
# Example template - update as needed
# DATABASE_URL=
# API_KEY=
# NODE_ENV=development
```

### Configuration Files
List and describe key configuration files as they're added:
- `.env` - Environment variables (never commit!)
- `.env.example` - Template for environment variables
- `config.json` - Application configuration
- etc.

---

## 🚢 Deployment

> **To be defined** based on deployment strategy

### Build Process
```bash
# To be added
# npm run build / make build / etc.
```

### Deployment Steps
1. TBD
2. TBD
3. TBD

### Environments
- **Development:** Local development environment
- **Staging:** Pre-production testing (if applicable)
- **Production:** Live environment

---

## 📝 Code Style and Conventions

> **To be defined** based on chosen language/framework

### Naming Conventions
- **Variables:** `camelCase` / `snake_case` (depending on language)
- **Constants:** `UPPER_SNAKE_CASE`
- **Functions:** `camelCase` / `snake_case`
- **Classes:** `PascalCase`
- **Files:** `kebab-case` or language convention

### Formatting
- Use consistent indentation (2 or 4 spaces)
- Configure and use a formatter (Prettier, Black, gofmt, etc.)
- Configure and use a linter (ESLint, Pylint, etc.)
- Keep line length reasonable (80-120 characters)

### Comments
- Write self-documenting code when possible
- Add comments only when logic isn't self-evident
- Document "why" not "what"
- Keep comments up-to-date with code changes

---

## 🗂 Key Files and Their Purpose

> **To be populated** as significant files are added

| File | Purpose | Notes |
|------|---------|-------|
| CLAUDE.md | AI assistant guide | This file |
| README.md | User documentation | To be created |
| .gitignore | Git ignore patterns | To be created |

---

## 🔍 Common Tasks

### Adding a New Feature
1. Create feature branch: `git checkout -b feature/feature-name`
2. Read relevant existing code
3. Plan implementation using TodoWrite
4. Implement feature following code quality standards
5. Write/update tests
6. Update documentation
7. Commit with descriptive message
8. Push and create pull request

### Fixing a Bug
1. Create fix branch: `git checkout -b fix/bug-description`
2. Read code to understand the bug
3. Write failing test that reproduces the bug (if possible)
4. Fix the bug
5. Verify test now passes
6. Commit with "fix:" prefix
7. Push and create pull request

### Refactoring
1. Ensure tests exist and pass
2. Make incremental changes
3. Run tests after each change
4. Commit frequently
5. Don't mix refactoring with feature work

---

## 📚 Resources and References

### Documentation
- Project README: To be created
- API documentation: To be created
- Architecture docs: To be created

### External Resources
- Language/framework documentation
- Best practices guides
- Security guidelines (OWASP, etc.)

---

## 🤝 Contributing

### For Human Developers
1. Read this guide and README.md
2. Set up development environment
3. Create feature branch
4. Make changes following conventions
5. Test thoroughly
6. Submit pull request

### For AI Assistants
1. **Always read this file first** when starting a new session
2. Follow all guidelines in "AI Assistant Guidelines" section
3. Use TodoWrite for complex tasks
4. Read code before modifying
5. Ask for clarification when uncertain
6. Update this file when conventions change

---

## 📋 Checklist for AI Assistants

Before making changes, verify:
- [ ] I have read the relevant code files
- [ ] I understand the existing patterns and conventions
- [ ] I have created a todo list for complex tasks
- [ ] My changes follow the code quality standards
- [ ] I have checked for security vulnerabilities
- [ ] I have not over-engineered the solution
- [ ] I have tested my changes (or written tests)
- [ ] My commit message follows conventions
- [ ] I have updated documentation if needed

---

## 🔄 Maintenance

### Keeping This File Updated
This file should be updated when:
- Project structure changes significantly
- New conventions are adopted
- New tools or frameworks are added
- Deployment process changes
- Common issues or patterns emerge

**Responsibility:** Any developer (human or AI) making structural changes should update this file.

---

## 📞 Getting Help

### For Questions About This Project
- Check README.md (when created)
- Review existing code and patterns
- Check git history for context
- Ask the project maintainer

### For Claude-Specific Issues
- Visit: https://github.com/anthropics/claude-code/issues
- Use `/help` command in Claude Code

---

## 🎯 Project Vision and Goals

> **To be defined** by project owner

### What This Project Does
*Description to be added when project scope is defined*

### Key Objectives
1. TBD
2. TBD
3. TBD

### Success Criteria
- TBD
- TBD
- TBD

---

## 📊 Project Status

**Current Phase:** Repository Initialization
**Next Steps:**
1. Define project scope and technology stack
2. Initialize project structure
3. Set up development environment
4. Create initial documentation
5. Begin feature development

---

## 🏷 Version History

### 2025-12-07 - Initial Creation
- Created CLAUDE.md template for empty repository
- Established structure for future documentation
- Defined AI assistant guidelines and best practices

---

*This file is a living document. Keep it updated as the project evolves.*
