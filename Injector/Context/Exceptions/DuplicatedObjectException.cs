using System;

namespace Injector.Context.Exceptions
{
    public class DuplicatedObjectException : Exception
    {
        public DuplicatedObjectException(string name) : base($"Duplicated object : name ({name})") {}
        public DuplicatedObjectException(Type type) : base($"Duplicated anonymous object : type ({type})") {}
    }
}