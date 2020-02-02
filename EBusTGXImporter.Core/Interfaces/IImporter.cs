using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.Core.Interfaces
{
    public interface IImporter
    {
        bool PreImportProcessing(string filePath);
        bool ProcessFile(string filePath);
        bool PostImportProcessing(string filePath);
        bool ValidateFile(string filePath);
    }
}
