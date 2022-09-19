namespace BlazorBootstrap;

public partial class BarChart<TChartDataset> : BaseChart<TChartDataset> where TChartDataset : IChartDataset
{
    #region Constructors

    public BarChart()
    {
        chartType = ChartType.Bar;
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
