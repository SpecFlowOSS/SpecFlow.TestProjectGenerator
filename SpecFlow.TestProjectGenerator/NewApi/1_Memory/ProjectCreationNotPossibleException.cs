using System;
using System.Runtime.Serialization;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectCreationNotPossibleException : Exception
    {
        public ProjectCreationNotPossibleException()
        {
        }

        public ProjectCreationNotPossibleException(string message) : base(message)
        {
        }

        public ProjectCreationNotPossibleException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}