using System;

namespace CodexTest.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string? message = null) : base(message) { }
}
