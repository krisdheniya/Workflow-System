# Infonetica Workflow Engine (State-Machine API)

A minimal backend service for defining and running configurable workflows as state machines. Example: food order processing for a food company.

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Run the API
```bash
dotnet run --project Infonetica.WorkflowEngine
```

### API Overview

#### 1. Define a Workflow
- **POST /workflows**
  - Body: `{ id, name, states, actions }`
  - See sample below.

#### 2. List Workflows
- **GET /workflows**

#### 3. Get Workflow by ID
- **GET /workflows/{id}**

#### 4. Start Workflow Instance
- **POST /workflows/{id}/instances**

#### 5. List Instances
- **GET /instances**

#### 6. Get Instance by ID
- **GET /instances/{id}**

#### 7. Execute Action on Instance
- **POST /instances/{id}/actions/{actionId}**

## Sample: Food Order Workflow

A sample workflow is preloaded on startup:
- **States:** created → preparing → delivering → delivered
- **Actions:** start-prep, send-out, mark-delivered

## Example: Create a Workflow
```json
{
  "id": "food-order",
  "name": "Food Order Processing",
  "states": [
    { "id": "created", "name": "Order Created", "isInitial": true, "isFinal": false, "enabled": true },
    { "id": "preparing", "name": "Preparing Food", "isInitial": false, "isFinal": false, "enabled": true },
    { "id": "delivering", "name": "Out for Delivery", "isInitial": false, "isFinal": false, "enabled": true },
    { "id": "delivered", "name": "Delivered", "isInitial": false, "isFinal": true, "enabled": true }
  ],
  "actions": [
    { "id": "start-prep", "name": "Start Preparing", "enabled": true, "fromStates": ["created"], "toState": "preparing" },
    { "id": "send-out", "name": "Send Out for Delivery", "enabled": true, "fromStates": ["preparing"], "toState": "delivering" },
    { "id": "mark-delivered", "name": "Mark as Delivered", "enabled": true, "fromStates": ["delivering"], "toState": "delivered" }
  ]
}
```

## Assumptions & Notes
- In-memory storage (no DB). Data resets on restart.
- Validation: unique IDs, one initial state, valid transitions, etc.
- Minimal error handling for clarity.
- Extendable for more features (e.g., persistence, more validation).

## Limitations
- No authentication or authorization.
- No database or persistent storage.
- No UI (API only).

---

**For exercise review:**
- See `Program.cs` for all logic (minimal API style).
- All requirements from the exercise are addressed. 