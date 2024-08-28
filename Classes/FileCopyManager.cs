using System.Collections.Generic;
using System;
using FileCopyer.Interface;

namespace FileCopyer.Classes
{
    public class FileCopyManager
    {
        private static readonly Lazy<FileCopyManager> _instance = new Lazy<FileCopyManager>(() => new FileCopyManager());
        private readonly List<IFileOperation> _operations = new List<IFileOperation>();

        private FileCopyManager() { }

        public static FileCopyManager Instance => _instance.Value;

        // Add an operation to the list
        public void AddOperation(IFileOperation operation)
        {
            _operations.Add(operation);
        }

        // Get all operations
        public IEnumerable<IFileOperation> GetOperations()
        {
            return _operations;
        }

        // Execute a single operation
        public void ExecuteOperation(IFileOperation operation)
        {
            operation.Execute();
        }
    }
}
