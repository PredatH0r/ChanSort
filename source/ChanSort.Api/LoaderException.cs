using System;

namespace ChanSort;

public class LoaderException : Exception
{
  public enum RecoveryMode { TryNext, Fail }

  public RecoveryMode Recovery { get; }

  private LoaderException(RecoveryMode recovery, string message, Exception inner) : base(message, inner)
  {
    Recovery = recovery;
  }

  /// <summary>
  /// In case the loader detects an unsupported file content (or knows another loader is responsible for loading it)
  /// </summary>
  public static LoaderException TryNext(string message, Exception inner = null) => throw new LoaderException(RecoveryMode.TryNext, message, inner);

  /// <summary>
  /// Stop all loading attempts for the file
  /// </summary>
  public static LoaderException Fail(string message, Exception inner = null) => throw new LoaderException(RecoveryMode.Fail, message, inner);
}

