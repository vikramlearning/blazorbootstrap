using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBootstrap;

/// <summary>
/// Data provider (delegate).
/// </summary>
public delegate Task<GridDataProviderResult<TItem>> GridDataProviderDelegate<TItem>(GridDataProviderRequest<TItem> request);

