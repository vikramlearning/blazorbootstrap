using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// A submitted week's review (project managers, AC-7f): the person, the week's hours and billable hours, and per project
/// its entries (day, task, hours, billable, note; plain line-item tables) with who approved it or who it waits for, and
/// "Approve KEY hours" for the projects the signed-in manager approves (<see cref="PbState.ApproveHours" />). "Return the
/// week" is an <see cref="EditForm" /> (rule WR-002: a reason of 10 to 300 characters, with a linked summary;
/// <see cref="PbState.ReturnWeek" />). Outcomes are announced in an always-rendered status region. An unknown or draft
/// week shows <see cref="PbNotFound" />.
/// </summary>
public partial class ApprovalDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private bool focusStatus;

    private ReturnForm form = new();

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        ResetForm();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Approve(int projectId)
    {
        var key = State.FindProject(projectId)?.Key ?? "The project's";
        var result = State.ApproveHours(Id, projectId);
        var sheet = State.FindTimesheet(Id);
        SetStatus(result switch
        {
            PbResult.Ok when sheet?.Status == PbTimesheetStatus.Approved => $"Approved {key} hours. Every project is approved, so the week is approved and its billable hours are ready to invoice.",
            PbResult.Ok => $"Approved {key} hours. The week still waits for the other projects.",
            PbResult.WrongState => "This week was returned or approved meanwhile. Nothing changed.",
            PbResult.NotAllowed => $"{key} hours are approved by its own project manager. Nothing changed.",
            PbResult.NotFound => "This week or project was deleted. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        }, result != PbResult.Ok);
        focusStatus = true;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void ResetForm()
    {
        form = new ReturnForm();
        editContext = new EditContext(form);
    }

    /// <summary>Returns the week once the reason is valid; the outcome is announced.</summary>
    private void ReturnWeek()
    {
        if (!editContext.Validate())
            return;

        var result = State.ReturnWeek(Id, form.Reason);
        SetStatus(result switch
        {
            PbResult.Ok => $"Returned the week to {State.FindPerson(State.FindTimesheet(Id)?.PersonId)?.Name ?? "its owner"} with your reason. They're told, and every approval is cleared.",
            PbResult.WrongState => "This week was approved or returned meanwhile. Nothing changed.",
            PbResult.NotAllowed => "None of this week's projects are yours to approve, so you can't return it.",
            PbResult.Invalid => $"Write a reason of {PbState.MinReturnReasonLength} to {PbState.MaxReturnReasonLength} characters.",
            _ => "That didn't work, and nothing changed. Try again.",
        }, result != PbResult.Ok);

        if (result == PbResult.Ok)
            ResetForm();

        focusStatus = true;
    }

    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the timesheet.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Why a week is returned: 10 to 300 characters the person reads.</summary>
    private sealed class ReturnForm
    {
        [Required(ErrorMessage = "Write why you return the week, so they know what to change.")]
        [StringLength(PbState.MaxReturnReasonLength, MinimumLength = PbState.MinReturnReasonLength, ErrorMessage = "Use 10 to 300 characters for the reason.")]
        public string Reason { get; set; } = string.Empty;
    }

    #endregion
}
