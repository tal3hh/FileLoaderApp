using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileLoaderApp.Models;

namespace FileLoaderApp.Service
{
    public interface ITradeLoader
    {
        List<TradeDataItem> LoadData(string filePath);
    }


}
