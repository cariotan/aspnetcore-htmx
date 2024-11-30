using System.Text.Json.Serialization;

public class Result
{
	public bool IsSuccessful { get; init; }
	public bool IsNotSuccessful => !IsSuccessful;

	[JsonIgnore]
	public Error ErrorMessage
	{
		get => ErrorMessages.Any() ? ErrorMessages.First() : "";
		init
		{
			var errorMessages = new List<Error> { value };
			ErrorMessages = errorMessages;
		}
	}
	public IEnumerable<Error> ErrorMessages { get; init; } = new List<Error>();

	public static Result True => new() { IsSuccessful = true };
	public static Result False => new() { IsSuccessful = false };
	public static Result Error(string error) => new() { IsSuccessful = false, ErrorMessage = error };
	public static Result Errors(string error) => new() { IsSuccessful = false, ErrorMessages = new List<Error>{ new(error) } };
	public static Result Errors(IEnumerable<Error> errors) => new() { IsSuccessful = false, ErrorMessages = errors };
	public static Result<T> Create<T>(T value) => value is null ? Result<T>.False : Success(value);
	public static Result<T> Success<T>(T value) => new() { IsSuccessful = true, Value = value };

	public static implicit operator Result(bool value) => new() { IsSuccessful = value };
	public static implicit operator Result(Error error) => new() { IsSuccessful = false, ErrorMessage = error };
	public static implicit operator Result(List<Error> errors) => new() { IsSuccessful = false, ErrorMessages = errors };
}

public class Result<T> : Result
{
	public T Value { get; init; }
	
	public new static Result<T> Error(string error) => new() { IsSuccessful = false, ErrorMessage = error };
	
	// public static implicit operator Result<T>(Result r) => new() { IsSuccessful = r.IsSuccessful, ErrorMessage = r.ErrorMessage, ErrorMessages = r.ErrorMessages };
	
	public new static Result<T> True => new() { IsSuccessful = true };
	public new static Result<T> False => new() { IsSuccessful = false };

	public static Result<T> Create(T value) => value is null ? False : Success(value);

	public void IfSuccessful(Action<T> action)
	{
		if(IsSuccessful)
		{
			action(Value);
		}
	}

	public bool IsSuccess(out T value)
	{
		value = Value;
		return IsSuccessful;
	}

	public bool IfNotSuccess(out T value)
	{
		value = Value;
		return IsSuccessful is false;
	}

	public A IfSuccessful<A>(Func<T, A> action)
	{
		if(IsSuccessful)
		{
			return action(Value);
		}

		return default;
	}

	public async Task IfSuccessfulAsync(Func<T, Task> action)
	{
		if(IsSuccessful)
		{
			await action(Value);
		}
	}

	public TResult Match<TResult>(Func<T, TResult> f1, Func<IEnumerable<Error>, TResult> f2)
	{
		return IsSuccessful ? f1(Value) : f2(ErrorMessages.ToList());
	}

	public void IfSuccessfulOrElse(Action<T> f1, Action<IEnumerable<Error>> f2)
	{
		if(IsSuccessful)
		{
			f1(Value);
		}
		else
		{
			f2(ErrorMessages.ToList());
		}
	}
	
	public async Task MatchAsync(Func<T, Task> f1, Action<IEnumerable<Error>> f2)
	{
		if(IsSuccessful)
		{
			await f1(Value);
		}
		else
		{
			f2(ErrorMessages.ToList());
		}
	}

	public static implicit operator Result<T>(T value)
	{
		return new() { IsSuccessful = true, Value = value };
	}

	public static implicit operator Result<T>(bool value) => new() { IsSuccessful = value };
	public static implicit operator Result<T>(Error error) => new() { IsSuccessful = false, ErrorMessage = error };
	public static implicit operator Result<T>(List<Error> errors) => new() { IsSuccessful = false, ErrorMessages = errors };

	public static implicit operator T(Result<T> v)
	{
		return v.Value;
	}
}

public class Error
{
	public string Message { get; init; }

	public Error(string message)
	{
		Message = message;
	}

	public override string ToString() => Message;
	
	public static implicit operator Error(string error) => new(error);
	// https://stackoverflow.com/questions/751303/cannot-implicitly-convert-type-x-to-string-when-and-how-it-decides-that-it
	public static implicit operator string(Error error) => error?.ToString();
}

public static partial class ExtensionClasses
{
	public static Error Error(string errorMessage)
	{
		return new Error(errorMessage);
	}
}
}