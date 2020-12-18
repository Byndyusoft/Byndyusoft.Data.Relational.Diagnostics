[![(License)](https://img.shields.io/github/license/Byndyusoft/Byndyusoft.Data.Relational.Diagnostics.svg)](LICENSE.txt)
[![Nuget](http://img.shields.io/nuget/v/Byndyusoft.Data.Relational.Diagnostics.svg?maxAge=10800)](https://www.nuget.org/packages/Byndyusoft.Data.Relational.Diagnostics/) [![NuGet downloads](https://img.shields.io/nuget/dt/Byndyusoft.Data.Relational.Diagnostics.svg)](https://www.nuget.org/packages/Byndyusoft.Data.Relational.Diagnostics/) 

# Usage

You can enable `DbConnection` diagnosting by calling `AddDiagnosting()` extension method over it.

```csharp
await using var connection = new NpgsqlConnection(connectionString).AddDiagnosting();
```

Here is a simple diagnostic observer:

```csharp
private sealed class Observer :
		IObserver<DiagnosticListener>,
		IObserver<KeyValuePair<string, object>>
{
	private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

	void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
	{
		if (diagnosticListener.Name == nameof(DbDiagnosticSource))
			_subscriptions.Add(diagnosticListener.Subscribe(this));
	}

	void IObserver<DiagnosticListener>.OnError(Exception error) { }

	void IObserver<DiagnosticListener>.OnCompleted()
	{
		_subscriptions.ForEach(x => x.Dispose());
		_subscriptions.Clear();
	}

	void IObserver<KeyValuePair<string, object>>.OnCompleted() { }

	public void OnError(Exception error) { }

	void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value) =>
		Console.WriteLine(value);
}
```

Next you can consume diagnostic events. You can read more at [DiagnosticSourceUsersGuide](https://github.com/dotnet/runtime/blob/master/src/libraries/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md).

```
var observer = new Observer();
using var subscription = DiagnosticListener.AllListeners.Subscribe(observer);
```

## Installing

```shell
dotnet add package Byndyusoft.Data.Relational.Diagnostics
```

# Contributing

To contribute, you will need to setup your local environment, see [prerequisites](#prerequisites). For the contribution and workflow guide, see [package development lifecycle](#package-development-lifecycle).

A detailed overview on how to contribute can be found in the [contributing guide](CONTRIBUTING.md).

## Prerequisites

Make sure you have installed all of the following prerequisites on your development machine:

- Git - [Download & Install Git](https://git-scm.com/downloads). OSX and Linux machines typically have this already installed.
- .NET (version 5.0 or higher) - [Download & Install .NET Core](https://dotnet.microsoft.com/download/dotnet/5.0).

## General folders layout

### src
- source code

### tests

- unit-tests

## Package development lifecycle

- Implement package logic in `src`
- Add or addapt unit-tests (prefer before and simultaneously with coding) in `tests`
- Add or change the documentation as needed
- Open pull request in the correct branch. Target the project's `master` branch

# Maintainers

[github.maintain@byndyusoft.com](mailto:github.maintain@byndyusoft.com)
