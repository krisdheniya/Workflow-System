// creating a application for a food delivery system with workflow engine
// contains default workflows which can be changed by the user
var appBuilder = WebApplication.CreateBuilder(args);

// Adding Swagger
appBuilder.Services.AddEndpointsApiExplorer();
appBuilder.Services.AddSwaggerGen();

var app = appBuilder.Build();

// checking for development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var workflowBlueprints = new List<WorkflowBlueprint>();
var activeWorkflowProcesses = new List<WorkflowProcess>();

// default workflow 
if (!workflowBlueprints.Any())
{
    var orderStages = new List<WorkflowState>
    {
        // state: ID, display Name, isInitial?, isFinal?, isActive?
        new("created", "Order Created", true, false, true),
        new("preparing", "Preparing Food", false, false, true),
        new("delivering", "Out for Delivery", false, false, true),
        new("delivered", "Delivered", false, true, true)
    };
    var orderTransitions = new List<WorkflowAction>
    {
        // action: ID, displayName, isActive?, fromStates, toState
        new("start-prep", "Start Preparing", true, new() { "created" }, "preparing"),
        new("send-out", "Send Out for Delivery", true, new() { "preparing" }, "delivering"),
        new("mark-delivered", "Mark as Delivered", true, new() { "delivering" }, "delivered")
    };

    workflowBlueprints.Add(new WorkflowBlueprint(
        "food-order",
        "Food Order Processing",
        orderStages,
        orderTransitions
    ));
}

// post request for adding workflows
app.MapPost("/workflows", (WorkflowBlueprint newBlueprint) =>
{
    //checking the new workflow
    if (newBlueprint.States.Count(s => s.IsInitial) != 1)
        return Results.BadRequest("A workflow must have exactly one starting state.");
    if (newBlueprint.States.Select(s => s.Id).Distinct().Count() != newBlueprint.States.Count)
        return Results.BadRequest("Each state must have a unique ID.");
    if (newBlueprint.Actions.Select(a => a.Id).Distinct().Count() != newBlueprint.Actions.Count)
        return Results.BadRequest("Each action must have a unique ID.");
    if (workflowBlueprints.Any(existing => existing.Id == newBlueprint.Id))
        return Results.BadRequest($"A workflow blueprint with ID '{newBlueprint.Id}' already exists.");

    workflowBlueprints.Add(newBlueprint);
    return Results.Created($"/workflows/{newBlueprint.Id}", newBlueprint);
});

// get request for workflow
app.MapGet("/workflows", () => workflowBlueprints);

//get request for workflow by id
app.MapGet("/workflows/{blueprintId}", (string blueprintId) =>
{
    var foundBlueprint = workflowBlueprints.FirstOrDefault(b => b.Id == blueprintId);
    return foundBlueprint is not null ? Results.Ok(foundBlueprint) : Results.NotFound("Workflow blueprint not found.");
});

// post request for starting a new process
app.MapPost("/workflows/{blueprintId}/instances", (string blueprintId) =>
{
    var blueprint = workflowBlueprints.FirstOrDefault(b => b.Id == blueprintId);
    if (blueprint is null)
        return Results.NotFound("Workflow blueprint not found.");

    var startingState = blueprint.States.First(s => s.IsInitial);
    var newProcess = new WorkflowProcess(
        Guid.NewGuid().ToString(), // generating a unique ID for this new process
        blueprint.Id,              
        startingState.Id,          
        new List<(string ActionId, DateTime Timestamp)>() // starting with an empty history
    );

    activeWorkflowProcesses.Add(newProcess);
    return Results.Created($"/instances/{newProcess.Id}", newProcess);
});

// get request for all active workflow processes
app.MapGet("/instances", () => activeWorkflowProcesses);

// get request for workflow process by id
app.MapGet("/instances/{processId}", (string processId) =>
{
    var foundProcess = activeWorkflowProcesses.FirstOrDefault(p => p.Id == processId);
    return foundProcess is not null ? Results.Ok(foundProcess) : Results.NotFound("Workflow process not found.");
});

// post request by entering processId and actionId
app.MapPost("/instances/{processId}/actions/{actionId}", (string processId, string actionId) =>
{
    var currentProcess = activeWorkflowProcesses.FirstOrDefault(p => p.Id == processId);
    if (currentProcess is null)
        return Results.NotFound("Workflow process not found.");

    var processBlueprint = workflowBlueprints.FirstOrDefault(b => b.Id == currentProcess.BlueprintId);
    if (processBlueprint is null)
        return Results.NotFound("Associated workflow blueprint not found."); 

    var actionToPerform = processBlueprint.Actions.FirstOrDefault(a => a.Id == actionId);
    if (actionToPerform is null)
        return Results.BadRequest($"Action '{actionId}' not defined in this workflow's blueprint.");

    if (!actionToPerform.IsEnabled)
        return Results.BadRequest($"Action '{actionId}' is currently disabled.");

    // checking if the action can be performed from the process's current state.
    if (!actionToPerform.FromStates.Contains(currentProcess.CurrentStateId))
        return Results.BadRequest($"Action '{actionId}' cannot be executed from the current state '{currentProcess.CurrentStateId}'.");

    var currentStateDefinition = processBlueprint.States.First(s => s.Id == currentProcess.CurrentStateId);
    if (currentStateDefinition.IsFinal)
        return Results.BadRequest("Cannot execute actions from a final (completed) state.");

    // updating process states and recording the action
    currentProcess.CurrentStateId = actionToPerform.ToState;
    currentProcess.History.Add((actionToPerform.Id, DateTime.UtcNow)); // Log when the action happened

    return Results.Ok(currentProcess);
});

app.Run(); // starting the application

public record WorkflowState(
    string Id,        // unique identifier 
    string Name,      // eg. "order created"
    bool IsInitial,   // true if this is the starting point
    bool IsFinal,     // true if this is an end point 
    bool IsEnabled    // true if this state is currently active
);

// defines an action that can move a workflow process from one state to another.
public record WorkflowAction(
    string Id,           // unique identifier 
    string Name,         // eg. "approve request"
    bool IsEnabled,      // true if this action is currently active/valid
    List<string> FromStates, // list of states from which this action can be taken
    string ToState       // the state the process moves to after this action
);

// defines a complete workflow type, combining its states and actions.
public record WorkflowBlueprint(
    string Id,              // unique ID for this workflow type
    string Name,            // display name for the workflow
    List<WorkflowState> States,     // possible states in this workflow
    List<WorkflowAction> Actions   // possible actions in this workflow
);

// represents a single active item or entity moving through a workflow.
public class WorkflowProcess
{
    public string Id { get; set; }               // unique ID for this specific process
    public string BlueprintId { get; set; }      
    public string CurrentStateId { get; set; }   
    // history of actions with timestamps.
    public List<(string ActionId, DateTime Timestamp)> History { get; set; }

    // constructor to create a new workflow process instance.
    public WorkflowProcess(string id, string blueprintId, string currentStateId, List<(string, DateTime)> history)
    {
        Id = id;
        BlueprintId = blueprintId;
        CurrentStateId = currentStateId;
        History = history;
    }
}