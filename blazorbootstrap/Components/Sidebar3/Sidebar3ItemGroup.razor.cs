namespace BlazorBootstrap;

    public partial class Sidebar3ItemGroup
    {
        #region Methods

        protected override void BuildClasses()
        {
            this.AddClass("flex-column");
            base.BuildClasses();
        }

        #endregion

        #region Properties, Indexers

        /// <inheritdoc />
        protected override bool ShouldAutoGenerateId => true;

        [CascadingParameter] public bool CollapseSidebar { get; set; }

        [Parameter] public IEnumerable<Sidebar3NavItem>? NavItems { get; set; }

        #endregion
    }
