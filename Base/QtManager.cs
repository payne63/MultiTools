using Inventor;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using I = Inventor;

namespace SplittableDataGridSAmple.Base
{
    static class QtManager
    {
        private static List< DataIQT> Datas;
        public static List<DataIQT> GetQtDatas(string firstPathFullName)
        {
            Datas = new();
            RecursiveQtdatas(firstPathFullName,1);
            var dic = new Dictionary<string, DataIQT>();
            foreach ( DataIQT data in Datas )
            {
                if (dic.ContainsKey(data.FullPathName))
                {
                    dic[data.FullPathName].Qt += data.Qt;
                    continue;
                }
                dic.Add(data.FullPathName, data);
            }
            return dic.Select(x=>x.Value).ToList() ; 
        }

        private static void RecursiveQtdatas(string PathFullName, int qt)
        {
            var data = new DataIQT(PathFullName,qt);
            Datas.Add(data);
            if (data.bom.Count == 0 ) return; 
            foreach ( var bomElement in data.bom )
            {
                RecursiveQtdatas(bomElement.fullFileName, bomElement.Qt*qt);
            }
        }
    }
}
