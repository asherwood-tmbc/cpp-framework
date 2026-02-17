using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPP.Framework.Data
{
    /// <summary>
    /// Abstract base class for all objects used to filter entity data for a data source context.
    /// </summary>
    public abstract class EFXDataSourceFilter : DataSourceFilter<EntityState>
    {
    }
}
