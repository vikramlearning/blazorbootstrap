namespace BlazorBootstrap;

public partial class DoughnutChart<TChartDataset> : BaseChart<TChartDataset> where TChartDataset : IChartDataset
{
    #region Constructors

    public DoughnutChart()
    {
        chartType = ChartType.Doughnut;
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
