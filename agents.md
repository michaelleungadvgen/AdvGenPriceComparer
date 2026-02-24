# Agents in Software Architecture

## Overview
Agents in software architecture refer to autonomous entities that can perceive their environment and take actions to achieve specific goals. They are fundamental components in distributed systems, artificial intelligence, and complex software applications.

## Key Concepts

### Agent Characteristics
- **Autonomy**: Agents operate independently without direct human intervention
- **Reactivity**: Agents can sense and respond to changes in their environment
- **Proactivity**: Agents can initiate actions based on their goals and beliefs
- **Social Ability**: Agents can interact and communicate with other agents

### Types of Agents
1. **Simple Reflex Agents**: Base actions on current percept
2. **Model-Based Reflex Agents**: Use internal state to handle partial observability
3. **Goal-Based Agents**: Use goals to guide decision-making
4. **Utility-Based Agents**: Maximize a utility function
5. **Learning Agents**: Improve performance through experience

## Architecture Patterns

### Multi-Agent Systems (MAS)
- **Coordination**: How agents collaborate and communicate
- **Communication Protocols**: Standards for agent interaction
- **Decentralization**: No central control point
- **Scalability**: Ability to handle increasing numbers of agents

### Agent Communication
- **Message Passing**: Direct communication between agents
- **Shared Memory**: Agents access common data structures
- **Blackboard Systems**: Centralized problem-solving space
- **Market-Based**: Auction-like mechanisms for resource allocation

## Implementation Considerations

### Design Principles
- **Modularity**: Each agent should have well-defined responsibilities
- **Interoperability**: Agents should work together seamlessly
- **Flexibility**: Systems should adapt to changing requirements
- **Robustness**: Agents should handle failures gracefully

### Technologies
- **Agent-Oriented Programming Languages**: Languages specifically designed for agent development
- **Middleware**: Platforms that facilitate agent communication (e.g., JADE, AgentSpeak)
- **Frameworks**: Existing frameworks that support agent-based development

## Common Use Cases

### E-commerce
- Price comparison agents
- Recommendation systems
- Automated bidding agents

### IoT Systems
- Smart home automation
- Sensor network management
- Resource allocation

### Business Intelligence
- Data mining agents
- Market analysis agents
- Decision support systems

## Best Practices

1. **Define Clear Agent Boundaries**: Each agent should have a well-defined scope
2. **Implement Proper Communication**: Use appropriate protocols for agent interaction
3. **Design for Scalability**: Consider how the system will grow
4. **Plan for Failures**: Implement fault tolerance mechanisms
5. **Monitor Performance**: Track agent behavior and system efficiency

## Challenges

- **Complexity Management**: Multi-agent systems can become very complex
- **Coordination Overhead**: Communication between agents can introduce delays
- **Debugging Difficulty**: Hard to trace issues in distributed systems
- **Scalability Issues**: Performance may degrade with increasing agent count
- **Security Concerns**: Agents may be vulnerable to malicious interference

## Future Trends

- **AI Integration**: More sophisticated learning and decision-making capabilities
- **Blockchain**: Secure agent interactions using distributed ledgers
- **Edge Computing**: Agents operating closer to data sources
- **Quantum Computing**: New possibilities for agent optimization

---

# AdvGenPriceComparer Project - Agent Guidelines

## Project-Specific Information

### Build Commands
```powershell
# Build the WPF project
cd AdvGenPriceComparer.WPF
dotnet build

# Run the application
dotnet run
```

### Project Structure
- **AdvGenPriceComparer.Core**: Core models and interfaces
- **AdvGenPriceComparer.Data.LiteDB**: LiteDB data access layer
- **AdvGenPriceComparer.WPF**: WPF desktop application

### Key Services Location
| Service | Location |
|---------|----------|
| JsonImportService | AdvGenPriceComparer.Data.LiteDB/Services/ |
| ServerConfigService | AdvGenPriceComparer.Core/Services/ |
| ExportService | AdvGenPriceComparer.WPF/Services/ |

### WPF Converters
XAML value converters are located in `AdvGenPriceComparer.WPF/Converters/`:
- **BooleanToVisibilityConverter**: Converts bool to Visibility
- **InverseBooleanConverter**: Inverts boolean values

Converters are registered in `App.xaml` as static resources.

### Dependency Injection
Services are registered in `AdvGenPriceComparer.WPF/App.xaml.cs` in the `ConfigureServices()` method.

### Task Coordination
- Use `multiagents.md` to track task assignments
- Update `plan.md` when completing tasks
- NEVER use `git push`, ONLY `git commit`

### Adding New Services
1. Create service class in appropriate project
2. Add to DI container in App.xaml.cs
3. Update multiagents.md with status
4. Update plan.md with progress

### Testing
- **TestExportWorkflow**: CLI test project for export workflow testing
  - Location: `TestExportWorkflow/`
  - Build: `dotnet build TestExportWorkflow/TestExportWorkflow.csproj`
  - Run: `dotnet run --project TestExportWorkflow/TestExportWorkflow.csproj`
  - Tests: 10 comprehensive export tests covering empty DB, filters, compression, etc.

### Repository Pattern
- Repositories are synchronous (not async) - use direct method calls
- Create repositories by passing DatabaseService to constructor:
  ```csharp
  var itemRepo = new ItemRepository(dbService);
  var placeRepo = new PlaceRepository(dbService);
  var priceRepo = new PriceRecordRepository(dbService);
  ```
- Repository methods: `Add()`, `GetAll()`, `GetById()`, `Update()`, `Delete()`