namespace BlazorBootstrap;

public partial class LineChart<TChartDataset> : BaseChart<TChartDataset> where TChartDataset : ChartDataset
{
    #region Constructors

    public LineChart()
    {
        chartType = ChartType.Line;
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
