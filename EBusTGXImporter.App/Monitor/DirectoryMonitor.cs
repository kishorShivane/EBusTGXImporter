using EBusTGXImporter.Core;
using EBusTGXImporter.Core.Interfaces;
using EBusTGXImporter.Helpers;
using EBusTGXImporter.Logger;
using System;
using System.IO;

namespace EBusTGXImporter.Monitors
{
    public class DirectoryMonitor
    {
        private FileSystemWatcher fileSystemWatcher;
        static ILogService logService = null;
        IImporter importerEngine;
        public DirectoryMonitor(string directoryToWatch, ILogService logger)
        {
            fileSystemWatcher = new FileSystemWatcher(directoryToWatch);
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.IncludeSubdirectories = true;

            // Instruct the file system watcher to call the FileCreated method
            // when there are files created at the folder.
            fileSystemWatcher.Created += new FileSystemEventHandler(FileCreated);
            logService = logger;

        } // end FileInputMonitor()

        private void FileCreated(Object sender, FileSystemEventArgs e)
        {
            if (AppHelper.IsXmlFile(e.Name))
            {
                logService.Info("Processing: XML file found - Start");
                importerEngine = new XmlImporter(logService);
                importerEngine.ProcessFile(e.FullPath);
                logService.Info("Processing: XML file found - End");
            }
            else
            {
                logService.Info("Processing: CSV file found - Start");
                importerEngine = new CsvImporter(logService);
                importerEngine.ProcessFile(e.FullPath);
                logService.Info("Processing: XML file found - End");
            }
        }
    }
}