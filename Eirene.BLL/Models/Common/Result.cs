using System;
using System.Collections.Generic;

namespace Eirene.BLL.Models.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, string? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException();
            if (!isSuccess && error == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(string error) => new Result(false, error);

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T? Value => _value;

        protected internal Result(T? value, bool isSuccess, string? error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, null);
        public new static Result<T> Failure(string error) => new Result<T>(default, false, error);
    }
}
