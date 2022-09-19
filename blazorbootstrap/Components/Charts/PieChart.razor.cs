namespace BlazorBootstrap;

public partial class PieChart<TChartDataset> : BaseChart<TChartDataset> where TChartDataset : IChartDataset
{
    #region Constructors

    public PieChart()
    {
        chartType = ChartType.Pie;
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
