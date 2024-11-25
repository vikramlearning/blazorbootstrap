using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BlazorBootstrap
{
    public partial class PasswordInput
    {
        #region Fields and Constants

        private FieldIdentifier fieldIdentifier;

        private string? oldValue;

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            oldValue = Value;

            AdditionalAttributes ??= new Dictionary<string, object>();

            fieldIdentifier = FieldIdentifier.Create(ValueExpression);

            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (oldValue != Value)
            {
                await ValueChanged.InvokeAsync(Value);

                EditContext?.NotifyFieldChanged(fieldIdentifier);

                oldValue = Value;
            }
        }

        public string InputTextType = "password";

        void OnShowHidePasswordButtonClick()
        {
            if (this.InputTextType == "password")
                this.InputTextType = "text";
            else
                this.InputTextType = "password";
        }

        /// <summary>
        /// Disables InputPassword.
        /// </summary>
        public void Disable() => Disabled = true;

        /// <summary>
        /// Enables InputPassword.
        /// </summary>
        public void Enable() => Disabled = false;

        /// <summary>
        /// This event is triggered only when the user changes the selection from the UI.
        /// </summary>
        /// <param name="args"></param>
        private async Task OnChange(ChangeEventArgs args)
        {
            Value = args.Value?.ToString();

            await ValueChanged.InvokeAsync(Value);

            EditContext?.NotifyFieldChanged(fieldIdentifier);

            oldValue = Value;
        }

        #endregion

        #region Properties, Indexers

        protected override string? ClassNames =>
            BuildClassNames(Class,
                (BootstrapClass.FormControl, true));

        /// <summary>
        /// Gets or sets the disabled state.
        /// </summary>
        /// <remarks>
        /// Default value is false.
        /// </remarks>
        [Parameter]
        public bool Disabled { get; set; }

        [CascadingParameter] private EditContext EditContext { get; set; } = default!;

        private string fieldCssClasses => EditContext?.FieldCssClass(fieldIdentifier) ?? "";


        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <remarks>
        /// Default value is null.
        /// </remarks>
        [Parameter]
        public string? Value { get; set; } = default!;

        /// <summary>
        /// This event is fired when the inputpassword value changes.
        /// </summary>
        [Parameter]
        public EventCallback<string?> ValueChanged { get; set; } = default!;

        [Parameter] public Expression<Func<string?>> ValueExpression { get; set; } = default!;

        #endregion
    }
}
