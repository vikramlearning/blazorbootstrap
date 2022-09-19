namespace BlazorBootstrap;

public partial class BubbleChart<TChartDataset> : BaseChart<TChartDataset> where TChartDataset : IChartDataset
{
    #region Constructors

    public BubbleChart()
    {
        chartType = ChartType.Bubble;
    }

    #endregion Constructors

    #region Members

    #endregion Members

    #region Methods

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
